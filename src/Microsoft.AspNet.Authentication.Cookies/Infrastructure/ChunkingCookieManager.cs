﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.Http;
using Microsoft.Framework.Internal;
using Microsoft.Framework.WebEncoders;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNet.Authentication.Cookies.Infrastructure
{
    /// <summary>
    /// This handles cookies that are limited by per cookie length. It breaks down long cookies for responses, and reassembles them
    /// from requests.
    /// </summary>
    public class ChunkingCookieManager : ICookieManager
    {
        public ChunkingCookieManager()
        {
            // Lowest common denominator. Safari has the lowest known limit (4093), and we leave little extra just in case.
            // See http://browsercookielimits.x64.me/.
            ChunkSize = 4090;
            ThrowForPartialCookies = true;
        }

        /// <summary>
        /// The maximum size of cookie to send back to the client. If a cookie exceeds this size it will be broken down into multiple
        /// cookies. Set this value to null to disable this behavior. The default is 4090 characters, which is supported by all
        /// common browsers.
        ///
        /// Note that browsers may also have limits on the total size of all cookies per domain, and on the number of cookies per domain.
        /// </summary>
        public int? ChunkSize { get; set; }

        /// <summary>
        /// Throw if not all chunks of a cookie are available on a request for re-assembly.
        /// </summary>
        public bool ThrowForPartialCookies { get; set; }

        // Parse the "chunks:XX" to determine how many chunks there should be.
        private static int ParseChunksCount(string value)
        {
            if (value != null && value.StartsWith("chunks:", StringComparison.Ordinal))
            {
                var chunksCountString = value.Substring("chunks:".Length);
                int chunksCount;
                if (int.TryParse(chunksCountString, NumberStyles.None, CultureInfo.InvariantCulture, out chunksCount))
                {
                    return chunksCount;
                }
            }
            return 0;
        }

        /// <summary>
        /// Get the reassembled cookie. Non chunked cookies are returned normally.
        /// Cookies with missing chunks just have their "chunks:XX" header returned.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <returns>The reassembled cookie, if any, or null.</returns>
        public string GetRequestCookie([NotNull] HttpContext context, [NotNull] string key)
        {
            var requestCookies = context.Request.Cookies;
            var value = requestCookies[key];
            var chunksCount = ParseChunksCount(value);
            if (chunksCount > 0)
            {
                var quoted = false;
                var chunks = new string[chunksCount];
                for (var chunkId = 1; chunkId <= chunksCount; chunkId++)
                {
                    var chunk = requestCookies[key + "C" + chunkId.ToString(CultureInfo.InvariantCulture)];
                    if (chunk == null)
                    {
                        if (ThrowForPartialCookies)
                        {
                            var totalSize = 0;
                            for (int i = 0; i < chunkId - 1; i++)
                            {
                                totalSize += chunks[i].Length;
                            }
                            throw new FormatException(
                                string.Format(CultureInfo.CurrentCulture, Resources.Exception_ImcompleteChunkedCookie, chunkId - 1, chunksCount, totalSize));
                        }
                        // Missing chunk, abort by returning the original cookie value. It may have been a false positive?
                        return value;
                    }
                    if (IsQuoted(chunk))
                    {
                        // Note: Since we assume these cookies were generated by our code, then we can assume that if one cookie has quotes then they all do.
                        quoted = true;
                        chunk = RemoveQuotes(chunk);
                    }
                    chunks[chunkId - 1] = chunk;
                }
                var merged = string.Join(string.Empty, chunks);
                if (quoted)
                {
                    merged = Quote(merged);
                }
                return merged;
            }
            return value;
        }

        /// <summary>
        /// Appends a new response cookie to the Set-Cookie header. If the cookie is larger than the given size limit
        /// then it will be broken down into multiple cookies as follows:
        /// Set-Cookie: CookieName=chunks:3; path=/
        /// Set-Cookie: CookieNameC1=Segment1; path=/
        /// Set-Cookie: CookieNameC2=Segment2; path=/
        /// Set-Cookie: CookieNameC3=Segment3; path=/
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="options"></param>
        public void AppendResponseCookie([NotNull] HttpContext context, [NotNull] string key, string value, [NotNull] CookieOptions options)
        {
            var escapedKey = UrlEncoder.Default.UrlEncode(key);

            var template = new SetCookieHeaderValue(escapedKey)
            {
                Domain = options.Domain,
                Expires = options.Expires,
                HttpOnly = options.HttpOnly,
                Path = options.Path,
                Secure = options.Secure,
            };

            var templateLength = template.ToString().Length;

            value = value ?? string.Empty;
            var quoted = false;
            if (IsQuoted(value))
            {
                quoted = true;
                value = RemoveQuotes(value);
            }
            var escapedValue = UrlEncoder.Default.UrlEncode(value);

            // Normal cookie
            var responseHeaders = context.Response.Headers;
            if (!ChunkSize.HasValue || ChunkSize.Value > templateLength + escapedValue.Length + (quoted ? 2 : 0))
            {
                template.Value = quoted ? Quote(escapedValue) : escapedValue;
                responseHeaders.AppendValues(Constants.Headers.SetCookie, template.ToString());
            }
            else if (ChunkSize.Value < templateLength + (quoted ? 2 : 0) + 10)
            {
                // 10 is the minimum data we want to put in an individual cookie, including the cookie chunk identifier "CXX".
                // No room for data, we can't chunk the options and name
                throw new InvalidOperationException(Resources.Exception_CookieLimitTooSmall);
            }
            else
            {
                // Break the cookie down into multiple cookies.
                // Key = CookieName, value = "Segment1Segment2Segment2"
                // Set-Cookie: CookieName=chunks:3; path=/
                // Set-Cookie: CookieNameC1="Segment1"; path=/
                // Set-Cookie: CookieNameC2="Segment2"; path=/
                // Set-Cookie: CookieNameC3="Segment3"; path=/
                var dataSizePerCookie = ChunkSize.Value - templateLength - (quoted ? 2 : 0) - 3; // Budget 3 chars for the chunkid.
                var cookieChunkCount = (int)Math.Ceiling(escapedValue.Length * 1.0 / dataSizePerCookie);

                template.Value = "chunks:" + cookieChunkCount.ToString(CultureInfo.InvariantCulture);
                responseHeaders.AppendValues(Constants.Headers.SetCookie, template.ToString());

                var chunks = new string[cookieChunkCount];
                var offset = 0;
                for (var chunkId = 1; chunkId <= cookieChunkCount; chunkId++)
                {
                    var remainingLength = escapedValue.Length - offset;
                    var length = Math.Min(dataSizePerCookie, remainingLength);
                    var segment = escapedValue.Substring(offset, length);
                    offset += length;

                    template.Name = escapedKey + "C" + chunkId.ToString(CultureInfo.InvariantCulture);
                    template.Value = quoted ? Quote(segment) : segment;
                    chunks[chunkId - 1] = template.ToString();
                }
                responseHeaders.AppendValues(Constants.Headers.SetCookie, chunks);
            }
        }

        /// <summary>
        /// Deletes the cookie with the given key by setting an expired state. If a matching chunked cookie exists on
        /// the request, delete each chunk.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="key"></param>
        /// <param name="options"></param>
        public void DeleteCookie([NotNull] HttpContext context, [NotNull] string key, [NotNull] CookieOptions options)
        {
            var escapedKey = UrlEncoder.Default.UrlEncode(key);
            var keys = new List<string>();
            keys.Add(escapedKey + "=");

            var requestCookie = context.Request.Cookies[key];
            var chunks = ParseChunksCount(requestCookie);
            if (chunks > 0)
            {
                for (int i = 1; i <= chunks + 1; i++)
                {
                    var subkey = escapedKey + "C" + i.ToString(CultureInfo.InvariantCulture);
                    keys.Add(subkey + "=");
                }
            }

            var domainHasValue = !string.IsNullOrEmpty(options.Domain);
            var pathHasValue = !string.IsNullOrEmpty(options.Path);

            Func<string, bool> rejectPredicate;
            Func<string, bool> predicate = value => keys.Any(k => value.StartsWith(k, StringComparison.OrdinalIgnoreCase));
            if (domainHasValue)
            {
                rejectPredicate = value => predicate(value) && value.IndexOf("domain=" + options.Domain, StringComparison.OrdinalIgnoreCase) != -1;
            }
            else if (pathHasValue)
            {
                rejectPredicate = value => predicate(value) && value.IndexOf("path=" + options.Path, StringComparison.OrdinalIgnoreCase) != -1;
            }
            else
            {
                rejectPredicate = value => predicate(value);
            }

            var responseHeaders = context.Response.Headers;
            var existingValues = responseHeaders.GetValues(Constants.Headers.SetCookie);
            if (existingValues != null)
            {
                responseHeaders.SetValues(Constants.Headers.SetCookie, existingValues.Where(value => !rejectPredicate(value)).ToArray());
            }

            AppendResponseCookie(
                context,
                key,
                string.Empty,
                new CookieOptions()
                {
                    Path = options.Path,
                    Domain = options.Domain,
                    Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                });

            for (int i = 1; i <= chunks; i++)
            {
                AppendResponseCookie(
                    context,
                    key + "C" + i.ToString(CultureInfo.InvariantCulture),
                    string.Empty,
                    new CookieOptions()
                    {
                        Path = options.Path,
                        Domain = options.Domain,
                        Expires = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    });
            }
        }

        private static bool IsQuoted([NotNull] string value)
        {
            return value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"';
        }

        private static string RemoveQuotes([NotNull] string value)
        {
            return value.Substring(1, value.Length - 2);
        }

        private static string Quote([NotNull] string value)
        {
            return '"' + value + '"';
        }
    }
}

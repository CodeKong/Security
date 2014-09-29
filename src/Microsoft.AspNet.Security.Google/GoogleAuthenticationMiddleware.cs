// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Security.DataHandler;
using Microsoft.AspNet.Security.DataProtection;
using Microsoft.AspNet.Security.Infrastructure;
using Microsoft.AspNet.Security.OAuth;
using Microsoft.Framework.Logging;
using Microsoft.Framework.OptionsModel;

namespace Microsoft.AspNet.Security.Google
{
    /// <summary>
    /// An ASP.NET middleware for authenticating users using Google OAuth 2.0.
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Justification = "Middleware are not disposable.")]
    public class GoogleAuthenticationMiddleware : OAuthAuthenticationMiddleware<GoogleAuthenticationOptions, IGoogleAuthenticationNotifications>
    {
        /// <summary>
        /// Initializes a new <see cref="GoogleAuthenticationMiddleware"/>.
        /// </summary>
        /// <param name="next">The next middleware in the HTTP pipeline to invoke.</param>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="options">Configuration options for the middleware.</param>
        public GoogleAuthenticationMiddleware(
            RequestDelegate next,
            IDataProtectionProvider dataProtectionProvider,
            ILoggerFactory loggerFactory,
            IOptionsAccessor<ExternalAuthenticationOptions> externalOptions,
            IOptionsAccessor<GoogleAuthenticationOptions> options,
            string optionsName)
            : base(next, dataProtectionProvider, loggerFactory, externalOptions, options.GetNamedOptions(optionsName))
        {
            if (Options.Notifications == null)
            {
                Options.Notifications = new GoogleAuthenticationNotifications();
            }
            if (string.IsNullOrEmpty(Options.SignInAsAuthenticationType))
            {
                Options.SignInAsAuthenticationType = externalOptions.Options.SignInAsAuthenticationType;
            }

            if (Options.Scope.Count == 0)
            {
                // Google OAuth 2.0 asks for non-empty scope. If user didn't set it, set default scope to 
                // "openid profile email" to get basic user information.
                // TODO: Should we just add these by default when we create the Options?
                Options.Scope.Add("openid");
                Options.Scope.Add("profile");
                Options.Scope.Add("email");
            }
        }

        /// <summary>
        /// Provides the <see cref="AuthenticationHandler"/> object for processing authentication-related requests.
        /// </summary>
        /// <returns>An <see cref="AuthenticationHandler"/> configured with the <see cref="GoogleAuthenticationOptions"/> supplied to the constructor.</returns>
        protected override AuthenticationHandler<GoogleAuthenticationOptions> CreateHandler()
        {
            return new GoogleAuthenticationHandler(Backchannel, Logger);
        }
    }
}

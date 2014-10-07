﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Security.Infrastructure;
using Microsoft.AspNet.Security.MicrosoftAccount;
using System;

namespace Microsoft.AspNet.Builder
{
    /// <summary>
    /// Extension methods for using <see cref="MicrosoftAccountAuthenticationMiddleware"/>
    /// </summary>
    public static class MicrosoftAccountAuthenticationExtensions
    {
        ///// <summary>
        ///// Authenticate users using Microsoft Account.
        ///// </summary>
        ///// <param name="app">The <see cref="IApplicationBuilder"/> passed to the configure method.</param>
        ///// <param name="clientId">The application client ID assigned by the Microsoft authentication service.</param>
        ///// <param name="clientSecret">The application client secret assigned by the Microsoft authentication service.</param>
        ///// <returns>The updated <see cref="IApplicationBuilder"/>.</returns>
        //public static IApplicationBuilder UseMicrosoftAccountAuthentication([NotNull] this IApplicationBuilder app, [NotNull] string clientId, [NotNull] string clientSecret)
        //{
        //    return app.UseMicrosoftAccountAuthentication(
        //        new MicrosoftAccountAuthenticationOptions
        //        {
        //            ClientId = clientId,
        //            ClientSecret = clientSecret,
        //        });
        //}

        ///// <summary>
        ///// Authenticate users using Microsoft Account.
        ///// </summary>
        ///// <param name="app">The <see cref="IApplicationBuilder"/> passed to the configure method.</param>
        ///// <param name="options">The middleware configuration options.</param>
        ///// <returns>The updated <see cref="IApplicationBuilder"/>.</returns>
        //public static IApplicationBuilder UseMicrosoftAccountAuthentication([NotNull] this IApplicationBuilder app, [NotNull] MicrosoftAccountAuthenticationOptions options)
        //{
        //    if (string.IsNullOrEmpty(options.SignInAsAuthenticationType))
        //    {
        //        options.SignInAsAuthenticationType = app.GetDefaultSignInAsAuthenticationType();
        //    }
        //    return app.UseMiddleware<MicrosoftAccountAuthenticationMiddleware>(options);
        //}

        public static IApplicationBuilder UseMicrosoftAccountAuthentication([NotNull] this IApplicationBuilder app, Action<MicrosoftAccountAuthenticationOptions> configureOptions = null, string optionsName = "")
        {
            return app.UseMiddleware<MicrosoftAccountAuthenticationMiddleware>(new OptionsConfiguration<MicrosoftAccountAuthenticationOptions>
            {
                Name = optionsName,
                ConfigureOptions = configureOptions
            });
        }
    }
}

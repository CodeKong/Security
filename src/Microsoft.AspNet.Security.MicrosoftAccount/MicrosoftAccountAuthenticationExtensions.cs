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
        public static IServiceCollection ConfigureMicrosoftAccountAuthentication([NotNull] this IServiceCollection services, [NotNull] Action<MicrosoftAccountAuthenticationOptions> configure)
        {
            return services.ConfigureOptions(configure);
        }

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

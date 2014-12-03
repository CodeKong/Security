//// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Security.Claims;
//using System.Threading.Tasks;

//namespace Microsoft.AspNet.Security
//{
//    public static class AuthorizationServiceExtensions
//    {
//        /// <summary>
//        /// Checks if a user has specific claims.
//        /// </summary>
//        /// <param name="claim">The claim to check against a specific user.</param>
//        /// <param name="user">The user to check claims against.</param>
//        /// <returns><value>true</value> when the user fulfills one of the claims, <value>false</value> otherwise.</returns>
//        public static Task<bool> AuthorizeAsync([NotNull] this IAuthorizationService service, Claim claim, ClaimsPrincipal user)
//        {
//            return service.AuthorizeAsync(new Claim[] { claim }, user);
//        }

//        /// <summary>
//        /// Checks if a user has specific claims.
//        /// </summary>
//        /// <param name="claim">The claim to check against a specific user.</param>
//        /// <param name="user">The user to check claims against.</param>
//        /// <returns><value>true</value> when the user fulfills one of the claims, <value>false</value> otherwise.</returns>
//        public static bool Authorize([NotNull] this IAuthorizationService service, Claim claim, ClaimsPrincipal user)
//        {
//            return service.Authorize(new Claim[] { claim }, user);
//        }

//        /// <summary>
//        /// Checks if a user has specific claims for a specific context obj.
//        /// </summary>
//        /// <param name="claim">The claim to check against a specific user.</param>
//        /// <param name="user">The user to check claims against.</param>
//        /// <param name="resource">The resource the claims should be check with.</param>
//        /// <returns><value>true</value> when the user fulfills one of the claims, <value>false</value> otherwise.</returns>
//        public static Task<bool> AuthorizeAsync([NotNull] this IAuthorizationService service, Claim claim, ClaimsPrincipal user, object resource)
//        {
//            return service.AuthorizeAsync(new Claim[] { claim }, user, resource);
//        }

//        /// <summary>
//        /// Checks if a user has specific claims for a specific context obj.
//        /// </summary>
//        /// <param name="claim">The claimsto check against a specific user.</param>
//        /// <param name="user">The user to check claims against.</param>
//        /// <param name="resource">The resource the claims should be check with.</param>
//        /// <returns><value>true</value> when the user fulfills one of the claims, <value>false</value> otherwise.</returns>
//        public static bool Authorize([NotNull] this IAuthorizationService service, Claim claim, ClaimsPrincipal user, object resource)
//        {
//            return service.Authorize(new Claim[] { claim }, user, resource);
//        }

//        /// <summary>
//        /// Checks if a user has specific claims.
//        /// </summary>
//        /// <param name="claims">The claims to check against a specific user.</param>
//        /// <param name="user">The user to check claims against.</param>
//        /// <returns><value>true</value> when the user fulfills one of the claims, <value>false</value> otherwise.</returns>
//        public static Task<bool> AuthorizeAsync([NotNull] this IAuthorizationService service, IEnumerable<Claim> claims, ClaimsPrincipal user)
//        {
//            return service.AuthorizeAsync(claims, user, null);
//        }

//        /// <summary>
//        /// Checks if a user has specific claims.
//        /// </summary>
//        /// <param name="claims">The claims to check against a specific user.</param>
//        /// <param name="user">The user to check claims against.</param>
//        /// <returns><value>true</value> when the user fulfills one of the claims, <value>false</value> otherwise.</returns>
//        public static  bool Authorize([NotNull] this IAuthorizationService service, IEnumerable<Claim> claims, ClaimsPrincipal user)
//        {
//            return service.Authorize(claims, user, null);
//        }
//    }
//}
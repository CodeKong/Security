// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Http.Security;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Security.Test
{
    public class DefaultAuthorizationServiceTests
    {
        private IAuthorizationService BuildAuthorizationService(Action<IServiceCollection> setupServices = null)
        {
            var services = new ServiceCollection();
            services.AddAuthorization();
            if (setupServices != null)
            {
                setupServices(services);
            }
            return services.BuildServiceProvider().GetRequiredService<IAuthorizationService>();
        }

        [Fact]
        public void AuthorizeCombineThrowsOnUnknownPolicy()
        {
            Assert.Throws<InvalidOperationException>(() => AuthorizationPolicy.Combine(new AuthorizationOptions(), new AuthorizeAttribute[] {
                new AuthorizeAttribute { Policy = "Wut" }
            }));
        }

        [Fact]
        public async Task Authorize_ShouldAllowIfClaimIsPresent()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequiresClaim("Permission", "CanViewPage"));
                });
            });
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim("Permission", "CanViewPage") }, "Basic"));

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Basic");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldAllowIfClaimIsPresentWithSpecifiedAuthType()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequiresClaim("Permission", "CanViewPage"));
                });
            });
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { new Claim("Permission", "CanViewPage") }, "Basic"));

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Basic");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldAllowIfClaimIsAmongValues()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequiresClaim("Permission", "CanViewPage", "CanViewAnything"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                        new Claim("Permission", "CanViewAnything")
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Basic");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldFailWhenAllRequirementsNotHandled()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequiresClaim("Permission", "CanViewPage", "CanViewAnything"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("SomethingElse", "CanViewPage"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfClaimTypeIsNotPresent()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequiresClaim("Permission", "CanViewPage", "CanViewAnything"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("SomethingElse", "CanViewPage"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfClaimValueIsNotPresent()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequiresClaim("Permission", "CanViewPage"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfNoClaims()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequiresClaim("Permission", "CanViewPage"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[0],
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfUserIsNull()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequiresClaim("Permission", "CanViewPage"));
                });
            });

            // Act
            var allowed = await authorizationService.AuthorizeAsync(null, null, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfNotCorrectAuthType()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequiresClaim("Permission", "CanViewPage"));
                });
            });
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldAllowWithNoAuthType()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequiresClaim("Permission", "CanViewPage"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewPage"),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Basic");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_ShouldNotAllowIfUnknownPolicy()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Permission", "CanViewComment"),
                    },
                    null)
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_CustomRolePolicy()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresRole("Administrator")
                .RequiresClaim(ClaimTypes.Role, "User");
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "User"),
                        new Claim(ClaimTypes.Role, "Administrator")
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, policy.Build());

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_HasAnyClaimOfTypePolicy()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim(ClaimTypes.Role);
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, ""),
                    },
                    "Basic")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, policy.Build());

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_PolicyCanAuthenticationTypeWithNameClaim()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder("AuthType").RequiresClaim(ClaimTypes.Name);
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "Name") }, "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, policy.Build());

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task Authorize_PolicyWillFilterAuthenticationType()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder("Bogus").RequiresClaim(ClaimTypes.Name);
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "Name") }, "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, policy.Build());

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task Authorize_PolicyCanFilterMultipleAuthenticationType()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder("One", "Two").RequiresClaim(ClaimTypes.Name, "one").RequiresClaim(ClaimTypes.Name, "two");
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal();
            user.AddIdentity(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "one") }, "One"));
            user.AddIdentity(new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Name, "two") }, "Two"));

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, policy.Build());

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanRequireSingleRole()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder("AuthType").RequiresRole("Admin");
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Role, "Admin") }, "AuthType")
            );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, policy.Build());

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanRequireOneOfManyRoles()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder("AuthType").RequiresRole("Admin", "Users");
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(new Claim[] { new Claim(ClaimTypes.Role, "Users") }, "AuthType"));

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, policy.Build());

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task RolePolicyCanBlockWrongRole()
        {
            // Arrange
            var policy = new AuthorizationPolicyBuilder().RequiresClaim("Permission", "CanViewPage");
            var authorizationService = BuildAuthorizationService();
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Role, "Nope"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, policy.Build());

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task RolePolicyCanBlockNoRole()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => policy.RequiresRole("Admin", "Users"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task PolicyFailsWithNoRequirements()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Basic", policy => { });
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Name, "Name"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Basic");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task CanApproveAnyAuthenticatedUser()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Any", policy => policy.RequireAuthenticatedUser());
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim(ClaimTypes.Name, "Name"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Any");

            // Assert
            Assert.True(allowed);
        }

        [Fact]
        public async Task CanBlockNonAuthenticatedUser()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Any", policy => policy.RequireAuthenticatedUser());
                });
            });
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Any");

            // Assert
            Assert.False(allowed);
        }

        public class CustomRequirement : IAuthorizationRequirement { }
        public class CustomHandler : AuthorizationHandler<CustomRequirement>
        {
            public override Task HandleAsync(AuthorizationContext context, CustomRequirement requirement)
            {
                context.Succeed(requirement);
                return Task.FromResult(true);
            }
        }

        [Fact]
        public async Task CustomReqWithNoHandlerFails()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Custom", policy => policy.Requirements.Add(new CustomRequirement()));
                });
            });
            var user = new ClaimsPrincipal();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Custom");

            // Assert
            Assert.False(allowed);
        }

        [Fact]
        public async Task CustomReqWithHandlerSucceeds()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddTransient<IAuthorizationHandler, CustomHandler>();
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Custom", policy => policy.Requirements.Add(new CustomRequirement()));
                });
            });
            var user = new ClaimsPrincipal();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Custom");

            // Assert
            Assert.True(allowed);
        }

        public class PassThroughRequirement : AuthorizationHandler<PassThroughRequirement>, IAuthorizationRequirement
        {
            public PassThroughRequirement(bool succeed)
            {
                Succeed = succeed;
            }

            public bool Succeed { get; set; }

            public override Task HandleAsync(AuthorizationContext context, PassThroughRequirement requirement)
            {
                if (Succeed) {
                    context.Succeed(requirement);
                }
                return Task.FromResult(0);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task PassThroughRequirementWillSucceedWithoutCustomHandler(bool shouldSucceed)
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    options.AddPolicy("Passthrough", policy => policy.Requirements.Add(new PassThroughRequirement(shouldSucceed)));
                });
            });
            var user = new ClaimsPrincipal();

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Passthrough");

            // Assert
            Assert.Equal(shouldSucceed, allowed);
        }

        public async Task CanCombinePolicies()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    var basePolicy = new AuthorizationPolicyBuilder().RequiresClaim("Base", "Value").Build();
                    options.AddPolicy("Combined", policy => policy.Combine(basePolicy).RequiresClaim("Claim", "Exists"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Base", "Value"),
                        new Claim("Claim", "Exists")
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Combined");

            // Assert
            Assert.True(allowed);
        }

        public async Task CombinePoliciesWillFailIfBasePolicyFails()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    var basePolicy = new AuthorizationPolicyBuilder().RequiresClaim("Base", "Value").Build();
                    options.AddPolicy("Combined", policy => policy.Combine(basePolicy).RequiresClaim("Claim", "Exists"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Claim", "Exists")
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Combined");

            // Assert
            Assert.False(allowed);
        }

        public async Task CombinedPoliciesWillFailIfExtraRequirementFails()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.ConfigureAuthorization(options =>
                {
                    var basePolicy = new AuthorizationPolicyBuilder().RequiresClaim("Base", "Value").Build();
                    options.AddPolicy("Combined", policy => policy.Combine(basePolicy).RequiresClaim("Claim", "Exists"));
                });
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("Base", "Value"),
                    },
                    "AuthType")
                );

            // Act
            var allowed = await authorizationService.AuthorizeAsync(user, null, "Combined");

            // Assert
            Assert.False(allowed);
        }

        public class ExpenseReport { }

        public static class Operations
        {
            public static OperationAuthorizationRequirement Edit = new OperationAuthorizationRequirement { Name = "Edit" };
            public static OperationAuthorizationRequirement Create = new OperationAuthorizationRequirement { Name = "Create" };
            public static OperationAuthorizationRequirement Delete = new OperationAuthorizationRequirement { Name = "Delete" };
        }

        public class ExpenseReportAuthorizationHandler : AuthorizationHandler<OperationAuthorizationRequirement, ExpenseReport>
        {
            public ExpenseReportAuthorizationHandler(IEnumerable<OperationAuthorizationRequirement> authorized)
            {
                _allowed = authorized;
            }

            private IEnumerable<OperationAuthorizationRequirement> _allowed;

            public override Task HandleAsync(AuthorizationContext context, OperationAuthorizationRequirement requirement, ExpenseReport resource)
            {
                if (_allowed.Contains(requirement))
                {
                    context.Succeed(requirement);
                }
                return Task.FromResult(0);
            }
        }

        public class SuperUserHandler : AuthorizationHandler<OperationAuthorizationRequirement>
        {
            public override Task HandleAsync(AuthorizationContext context, OperationAuthorizationRequirement requirement)
            {
                if (context.User.HasClaim("SuperUser", "yes"))
                {
                    context.Succeed(requirement);
                }
                return Task.FromResult(0);
            }
        }

        public async Task CanAuthorizeAllSuperuserOperations()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddInstance<IAuthorizationHandler>(new ExpenseReportAuthorizationHandler(new OperationAuthorizationRequirement[] { Operations.Edit }));
                services.AddTransient<IAuthorizationHandler, SuperUserHandler>();
            });
            var user = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new Claim[] {
                        new Claim("SuperUser", "yes"),
                    },
                    "AuthType")
                );

            // Act
            // Assert
            Assert.True(await authorizationService.AuthorizeAsync(user, null, Operations.Edit));
            Assert.True(await authorizationService.AuthorizeAsync(user, null, Operations.Delete));
            Assert.True(await authorizationService.AuthorizeAsync(user, null, Operations.Create));
        }

        public async Task CanAuthorizeOnlyAllowedOperations()
        {
            // Arrange
            var authorizationService = BuildAuthorizationService(services =>
            {
                services.AddInstance<IAuthorizationHandler>(new ExpenseReportAuthorizationHandler(new OperationAuthorizationRequirement[] { Operations.Edit }));
                services.AddTransient<IAuthorizationHandler, SuperUserHandler>();
            });
            var user = new ClaimsPrincipal();

            // Act
            // Assert
            Assert.True(await authorizationService.AuthorizeAsync(user, null, Operations.Edit));
            Assert.False(await authorizationService.AuthorizeAsync(user, null, Operations.Delete));
            Assert.False(await authorizationService.AuthorizeAsync(user, null, Operations.Create));
        }
    }
}
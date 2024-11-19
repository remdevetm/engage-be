using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace ApiAuthServer
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource("roles", "User roles", new[] { "role" })
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new List<ApiScope>
            {
                new ApiScope("UserAuthServiceAPI", "User Auth Service API")
            };

        public static IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("UserAuthServiceAPI", "User Auth Service API")
                {
                    Scopes = { "UserAuthServiceAPI" },
                    UserClaims = { "role" }
                }
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "EngageClient",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },
                    AllowedScopes = { "UserAuthServiceAPI" },
                    Claims = new List<ClientClaim>
                    {
                        new ClientClaim("role", "Admin")
                    },
                    AlwaysSendClientClaims = true
                }
            };

        public static List<TestUser> TestUsers =>
            new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "admin",
                    Password = "password",
                    Claims =
                    {
                        new Claim("role", "Admin")
                    }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "agent",
                    Password = "password",
                    Claims =
                    {
                        new Claim("role", "Agent")
                    }
                }
            };
    }
}


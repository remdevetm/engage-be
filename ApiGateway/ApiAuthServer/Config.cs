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
                // If you need specific identity resources, you can enable them here.
                // new IdentityResource("client_role", "User roles", new[] { "client_role" })
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
                //UserClaims = { "role" },
                //ApiSecrets = { new Secret("secret".Sha256()) }
            }
            };

        public static IEnumerable<Client> Clients =>
            new List<Client>
            {
                new Client
                {
                    ClientId = "AdminClient",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("admin-secret".Sha256())
                    },
                    AllowedScopes = { "UserAuthServiceAPI" },
                    Claims = new List<ClientClaim>
                    {
                        new ClientClaim("role", "Admin")
                    },
                    AlwaysSendClientClaims = true
                },
                new Client
                {
                    ClientId = "AgentClient",
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets =
                    {
                        new Secret("agent-secret".Sha256())
                    },
                    AllowedScopes = { "UserAuthServiceAPI" },
                    Claims = new List<ClientClaim>
                    {
                        new ClientClaim("role", "Agent")
                    },
                    AlwaysSendClientClaims = true
                }
            };



    }
}

//using System.Collections.Generic;
//using System.Security.Claims;
//using System.Text.Json;
//using IdentityModel;
//using IdentityServer4;
//using IdentityServer4.Test;

//namespace DI.TokenService.Core
//{
//    public class TestUsers
//    {
//        public static List<TestUser> Users
//        {
//            get
//            {
//                var address = new
//                {
//                    street_address = "5 Villaret street",
//                    city = "Harrison",
//                    post_code = 2148,
//                    country = "Australia"
//                };

//                return new List<TestUser>
//                {
//                    new()
//                    {
//                        SubjectId = "818727",
//                        Username = "admin",
//                        Password = "admin",
//                        Claims =
//                        {
//                            new Claim(JwtClaimTypes.Name, "Boards Admin"),
//                            new Claim(JwtClaimTypes.GivenName, "System "),
//                            new Claim(JwtClaimTypes.FamilyName, "Admin`"),
//                            new Claim(JwtClaimTypes.Email, "boards@email.com"),
//                            new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),

//                             //new Claim(JwtClaimTypes., JsonSerializer.Serialize(address),
//                             //   IdentityServerConstants.ClaimValueTypes.Json)
//                            //new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
//                            new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address),
//                                IdentityServerConstants.ClaimValueTypes.Json)
//                        }
//                    }
//                };
//            }
//        }
//    }
//}
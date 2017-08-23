using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Security.Claims;
using JWT;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.TinyIoc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using StudentSystemApiCs.Models;
using StudentSystemApiCs.Util;

namespace StudentSystemApiCs.Modules
{
    /// <summary>
    /// Used for authentication checking
    /// </summary>
    public abstract class SecureModule : NancyModule
    {
        private readonly Type userType;

        /// <summary>
        /// Constructs secure module that allows users of type userType to access
        /// </summary>
        /// <param name="uri">Base path</param>
        /// <param name="userType">Type of user to allow access</param>
        protected SecureModule(string uri, Type userType) : base(uri)
        {
            StatelessAuthentication.Enable(this, new StatelessAuthenticationConfiguration(GetUserIdentity));
            this.userType = userType;
            Before += ctx =>
            {
                var user = ctx.CurrentUser;
                return user != null
                    ? null
                    : Response.AsText("Unauthorized").WithStatusCode(HttpStatusCode.Unauthorized);
            };
        }

        /// <summary>
        /// Gets user identity from JWT
        /// </summary>
        /// <param name="ctx">Nancy request context</param>
        /// <returns>ClaimsPrincipal that contains user data on success, or null on fail</returns>
        private ClaimsPrincipal GetUserIdentity(NancyContext ctx)
        {
            try
            {
                var token = ctx.Request.Headers["X-Auth-Token"].First();
                var data = JsonWebToken.DecodeToObject<Dictionary<string, string>>(token, AppConfig.AppKey);
                var tokenExpires = DateTime.FromBinary(long.Parse(data["expires"]));
                if (tokenExpires <= DateTime.UtcNow) return null;
                var user = UserCache.Users.First(u => u.Email == data["email"]);
                var cp = new ClaimsPrincipal();
                var identity = new ClaimsIdentity();
                identity.AddClaim(new Claim("User", user.Id.ToString()));
                var userJson = JsonConvert.SerializeObject(user, Formatting.None, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                });
                identity.AddClaim(new Claim("Data", userJson));
                cp.AddIdentity(identity);
                if(userType == typeof(User)) return cp;
                return ObjectContext.GetObjectType(user.GetType()) == userType ? cp : null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                Console.WriteLine(e.InnerException?.Message);
                return null;
            }
        }
    }
}
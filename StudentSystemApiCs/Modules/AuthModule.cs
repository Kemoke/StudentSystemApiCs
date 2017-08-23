using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JWT;
using Microsoft.CSharp.RuntimeBinder;
using Nancy;
using Newtonsoft.Json;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Models;
using StudentSystemApiCs.Util;
using static BCrypt.Net.BCrypt;

namespace StudentSystemApiCs.Modules
{
    /// <summary>
    /// Used for general authentication routes.
    /// </summary>
    public sealed class AuthModule : NancyModule
    {
        public AuthModule() : base("/auth")
        {
            Post("/login", LoginAsync);
            Post("/register", RegisterAsync);
        }

        /// <summary>
        /// Admin registration.
        /// Should be disabled once an admin is registered.
        /// </summary>
        /// <param name="_">Not used</param>
        /// <param name="token">Cancellation token if request is cancelled</param>
        /// <returns>User details in JSON format</returns>
        private async Task<object> RegisterAsync(dynamic _, CancellationToken token)
        {
            var data = Request.Form;
            using (var context = new UniContext())
            {
                //Hashes password using BCrypt with work factor 8
                string pw = await Task.Run(() => HashPassword(data.password, GenerateSalt(8)), token);
                var admin = context.Admins.Add(new Admin
                {
                    Email = data.email,
                    FirstName = data.firstname,
                    LastName = data.lastname,
                    Password = pw
                });
                await context.SaveChangesAsync(token);
                UserCache.Users.Add(admin);
                return Response.AsJson(admin);
            }
        }

        /// <summary>
        /// Used for login handling.
        /// </summary>
        /// <param name="_">Not used</param>
        /// <param name="token">Cancellation token if request is cancelled</param>
        /// <returns>JWT and User type in JSON format</returns>
        private async Task<object> LoginAsync(dynamic _, CancellationToken token)
        {
            try
            {
                var data = Request.Form;
                string email = data.email;
                var user = UserCache.Users.First(u => u.Email == email);
                if (user == null || !await Task.Run(() => Verify((string) data.password, user.Password), token))
                    return Response.AsText("Invalid password").WithStatusCode(HttpStatusCode.Unauthorized);
                // sets expiration time to 1 day
                var exp = DateTime.UtcNow.AddDays(1).ToBinary();
                // converts EF proxy type to POCO type and returns the name of the type
                var type = ObjectContext.GetObjectType(user.GetType()).Name;
                // JWT encoded data
                var payload = new Dictionary<string, string>
                {
                    {"expires", exp.ToString()},
                    {"email", email},
                    {"type", type}
                };
                var jwt = JsonWebToken.Encode(payload, AppConfig.AppKey, JwtHashAlgorithm.HS256);
                return Response.AsJson(new {jwt, type});
            }
            catch (InvalidOperationException)
            {
                return Response.AsText("Email is not registered").WithStatusCode(HttpStatusCode.Unauthorized);
            }
            catch (ArgumentNullException)
            {
                return Response.AsText("Missing password").WithStatusCode(HttpStatusCode.Unauthorized);
            }
        }
    }
}
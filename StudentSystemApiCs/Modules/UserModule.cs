using System;
using System.Linq;
using Nancy;
using Newtonsoft.Json;
using StudentSystemApiCs.Models;
using StudentSystemApiCs.Util;

namespace StudentSystemApiCs.Modules
{
    public sealed class UserModule : SecureModule
    {
        /// <summary>
        /// Used to return information about logged in user
        /// </summary>
        public UserModule() : base("/user", typeof(User))
        {
            Get("/self", _ => Response.AsText(Context.CurrentUser.Identities.First().Claims.ToList()[1].Value).WithContentType("application/json"));
        }
    }
}

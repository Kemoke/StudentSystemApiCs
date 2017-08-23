using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;
using System.Threading.Tasks;
using Nancy.Security;
using Newtonsoft.Json;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Util;

namespace StudentSystemApiCs.Models
{
    /// <summary>
    /// Class that contains basic identity info for every user
    /// </summary>
    public abstract class User : BaseModel
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        protected void EditInstance(User model)
        {
            FirstName = model.FirstName;
            LastName = model.LastName;
            Email = model.Email;
        }
    }
}

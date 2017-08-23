using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Models;

namespace StudentSystemApiCs.Util
{
    /// <summary>
    /// Used for quick loading of basic user data.
    /// These models dont contain EF proxy models so they cant be used for writing operations.
    /// </summary>
    public static class UserCache
    {
        public static List<User> Users { get; private set; }

        /// <summary>
        /// Reloads the list of all users.
        /// </summary>
        public static void Reload()
        {
            if (Users == null)
                Users = new List<User>();
            else
                Users.Clear();
            using (var context = new UniContext())
            {
                Users.AddRange(context.Admins.ToList());
                Users.AddRange(context.Instructors.ToList());
                Users.AddRange(context.Students.ToList());
            }
        }
    }
}
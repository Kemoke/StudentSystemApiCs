using System.Data.Entity;
using StudentSystemApiCs.Models;
using StudentSystemApiCs.Util;

namespace StudentSystemApiCs.DAO
{
    /// <summary>
    /// This class handles all operations with database and is instantiated on every request.
    /// </summary>
    public class UniContext : DbContext
    {
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Program> Programs { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Section> Sections { get; set; }

        public UniContext() : base(AppConfig.GetDbConnection(), true)
        {
            Configuration.LazyLoadingEnabled = false;
        }

    }
}
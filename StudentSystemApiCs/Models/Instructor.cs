using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Util;

namespace StudentSystemApiCs.Models
{
    public class Instructor : User, ICrudModel<Instructor>
    {
        public string InstructorId { get; set; }
        [JsonIgnore]
        public virtual Department Department { get; set; }

        public virtual List<Section> Sections { get; set; }


        [JsonProperty("department"), NotMapped]
        public Department DepartmentJson
        {
            get
            {
                if (Department != null)
                    Department.Instructors = null;
                return Department;
            }
            set { Department = value; }
        }

        public async Task EditInstanceAsync(Instructor model, UniContext ctx, CancellationToken token)
        {
            EditInstance(model);
            InstructorId = model.InstructorId;
            Department = await ctx.Departments.FindAsync(token, model.Department.Id);
        }

        public async Task BindInstanceAsync(UniContext ctx, CancellationToken token)
        {
            Department = await ctx.Departments.FindAsync(token, Department.Id);
        }
    }
}
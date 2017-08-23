using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using Nancy.Json;
using Newtonsoft.Json;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Util;

namespace StudentSystemApiCs.Models
{
    public class Program : BaseModel, ICrudModel<Program>
    {
        [JsonProperty("curriculum"), NotMapped]
        public List<CurriculumCourse> CurriculumJson
        {
            get
            {
                if (Curriculum != null)
                {
                    Curriculum.ForEach(i => i.Program = null);
                    Courses = null;
                }
                return Curriculum;
            }
            set { Curriculum = value; }
        }
        public string Name { get; set; }
        [JsonIgnore]
        public virtual Department Department { get; set; }
        public virtual List<Student> Students { get; set; }
        public virtual List<Course> Courses { get; set; }
        [JsonIgnore]
        public virtual List<CurriculumCourse> Curriculum { get; set; }

        [JsonProperty("department"), NotMapped]
        public Department DepartmentJson
        {
            get
            {
                if(Department != null)
                    Department.Programs = null;
                return Department;
            }
            set { Department = value; }
        }

        public async Task EditInstanceAsync(Program model, UniContext ctx, CancellationToken token)
        {
            Name = model.Name;
            Department = await ctx.Departments.FindAsync(token, model.Department.Id);
        }

        public async Task BindInstanceAsync(UniContext ctx, CancellationToken token)
        {
            Department = await ctx.Departments.FindAsync(token, Department.Id);
        }
    }
}
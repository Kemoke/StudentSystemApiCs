using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Util;

namespace StudentSystemApiCs.Models
{
    public class Student : User, ICrudModel<Student>
    {
        public string StudentId { get; set; }
        public int Semester { get; set; }
        public int Year { get; set; }
        public double Cgpa { get; set; }
        [JsonIgnore]
        public virtual Program Program { get; set; }
        public virtual List<StudentGrade> Grades { get; set; }
        [JsonIgnore]
        public virtual List<Section> Sections { get; set; }


        [JsonProperty("program"), NotMapped]
        public Program JsonProgram
        {
            get
            {
                if (Program != null)
                    Program.Students = null;
                return Program;
            }
            set { Program = value; }
        }

        public async Task EditInstanceAsync(Student model, UniContext ctx, CancellationToken token)
        {
            EditInstance(model);
            StudentId = model.StudentId;
            Semester = model.Semester;
            Year = model.Year;
            Cgpa = model.Cgpa;
            Program = await ctx.Programs.FindAsync(token, model.Program.Id);
        }

        public async Task BindInstanceAsync(UniContext ctx, CancellationToken token)
        {
            Program = await ctx.Programs.FindAsync(token, Program.Id);
        }
    }
}
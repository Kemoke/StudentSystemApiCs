using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Util;

namespace StudentSystemApiCs.Models
{
    public class Course : BaseModel, ICrudModel<Course>
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public double Ects { get; set; }
        [JsonIgnore]
        public virtual Program Program { get; set; }
        public virtual List<Section> Sections { get; set; }

        [JsonProperty("program"), NotMapped]
        public Program JsonProgram
        {
            get
            {
                if (Program != null)
                    Program.Courses = null;
                return Program;
            }
            set { Program = value; }
        }

        public async Task EditInstanceAsync(Course model, UniContext ctx, CancellationToken token)
        {
            Name = model.Name;
            Code = model.Code;
            Ects = model.Ects;
            Program = await ctx.Programs.FindAsync(token, model.Program.Id);
        }

        public async Task BindInstanceAsync(UniContext ctx, CancellationToken token)
        {
            Program = await ctx.Programs.FindAsync(token, Program.Id);
        }
    }
}
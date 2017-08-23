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
    public class Section : BaseModel, ICrudModel<Section>
    {
        public int Number { get; set; }
        public int Capacity { get; set; }
        [JsonIgnore]
        public virtual Course Course { get; set; }
        [JsonIgnore]
        public virtual Instructor Instructor { get; set; }
        public virtual List<GradeType> GradeTypes { get; set; }
        [JsonIgnore]
        public virtual List<Student> Students { get; set; }
        public virtual List<TimeIndex> TimeTable { get; set; }



        [JsonProperty("students"), NotMapped]
        public List<Student> StudentsJson
        {
            get
            {
                Students?.ForEach(s => s.Sections = null);
                return Students;
            }
        }

        [JsonProperty("course"), NotMapped]
        public Course CourseJson
        {
            get
            {
                if (Course != null)
                    Course.Sections = null;
                return Course;
            }
            set { Course = value; }
        }

        [JsonProperty("instructor"), NotMapped]
        public Instructor InstructorJson
        {
            get
            {
                if (Instructor != null)
                    Instructor.Sections = null;
                return Instructor;
            }
            set { Instructor = value; }
        }

        protected bool Equals(Section other)
        {
            return Id == other.Id;
        }

        public async Task EditInstanceAsync(Section model, UniContext ctx, CancellationToken token)
        {
            Number = model.Number;
            Capacity = model.Capacity;
            Course = await ctx.Courses.FindAsync(token, model.Course.Id);
            Instructor = await ctx.Instructors.FindAsync(token, model.Instructor.Id);
            TimeTable.Clear();
            TimeTable.AddRange(model.TimeTable);
        }

        public async Task BindInstanceAsync(UniContext ctx, CancellationToken token)
        {
            Course = await ctx.Courses.FindAsync(token, Course.Id);
            Instructor = await ctx.Instructors.FindAsync(token, Instructor.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Section) obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }
}
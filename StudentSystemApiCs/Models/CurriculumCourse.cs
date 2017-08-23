using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace StudentSystemApiCs.Models
{
    public class CurriculumCourse : BaseModel
    {
        public int Year { get; set; }
        public int Semester { get; set; }
        public Elective Elective { get; set; }
        public virtual Program Program { get; set; }
        [JsonIgnore]
        public virtual Course Course { get; set; }

        [JsonProperty("course"), NotMapped]
        public Course JsonCourse
        {
            get
            {
                if (Course != null)
                    Course.Program = null;
                return Course;
            }
            set { Course = value; }
        }
    }

    public enum Elective
    {
        No,
        University,
        Faculty,
        Program
    }
}
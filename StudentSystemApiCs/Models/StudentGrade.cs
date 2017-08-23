using System.ComponentModel.DataAnnotations.Schema;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StudentSystemApiCs.DAO;

namespace StudentSystemApiCs.Models
{
    public class StudentGrade : BaseModel
    {
        public int Score { get; set; }
        public virtual GradeType GradeType { get; set; }
        [JsonIgnore]
        public virtual Student Student { get; set; }



        [JsonProperty("student"), NotMapped]
        public Student StudentJson
        {
            get
            {
                if (Student != null)
                    Student.Grades = null;
                return Student;
            }
            set { Student = value; }
        }

        public async Task BindInstanceAsync(UniContext ctx, CancellationToken token)
        {
            GradeType = await ctx.Set<GradeType>().FindAsync(token, GradeType.Id);
        }
    }
}
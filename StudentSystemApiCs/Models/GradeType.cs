using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace StudentSystemApiCs.Models
{
    public class GradeType : BaseModel
    {
        public string Name { get; set; }
        public int Value { get; set; }
        [JsonIgnore]
        public virtual Section Section { get; set; }


        [JsonProperty("section"), NotMapped]
        public Section JsonSection
        {
            get
            {
                if (Section != null)
                    Section.GradeTypes = null;
                return Section;
            }
            set { Section = value; }
        }
    }
}
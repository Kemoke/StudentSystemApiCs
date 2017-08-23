using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace StudentSystemApiCs.Models
{
    public class TimeIndex : BaseModel
    {
        public int Day { get; set; }
        public int StartTime { get; set; }
        public int EndTime { get; set; }
        [JsonIgnore]
        public virtual Section Section { get; set; }



        [JsonProperty("section"), NotMapped]
        public Section SectionJson
        {
            get
            {
                if (Section != null)
                    Section.TimeTable = null;
                return Section;
            }
            set { Section = value; }
        }
    }
}
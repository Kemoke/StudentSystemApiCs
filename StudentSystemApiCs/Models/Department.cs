using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Util;

namespace StudentSystemApiCs.Models
{
    public class Department : BaseModel, ICrudModel<Department>
    {
        public string Name { get; set; }

        public virtual List<Program> Programs { get; set; }
        public virtual List<Instructor> Instructors { get; set; }
#pragma warning disable 1998
        public async Task EditInstanceAsync(Department model, UniContext ctx, CancellationToken token)
#pragma warning restore 1998
        {
            Name = model.Name;
        }

#pragma warning disable 1998
        public async Task BindInstanceAsync(UniContext ctx, CancellationToken token)
#pragma warning restore 1998
        {
            
        }
    }
}
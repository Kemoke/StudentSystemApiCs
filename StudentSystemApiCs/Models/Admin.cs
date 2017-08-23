using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Util;

#pragma warning disable 1998
namespace StudentSystemApiCs.Models
{
    public class Admin : User, ICrudModel<Admin>
    {

        public async Task EditInstanceAsync(Admin model, UniContext ctx, CancellationToken token)
        {
            EditInstance(model);
        }

        public async Task BindInstanceAsync(UniContext ctx, CancellationToken token)
        {

        }
    }
}
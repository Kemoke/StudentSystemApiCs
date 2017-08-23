using System.Threading;
using System.Threading.Tasks;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Util;

namespace StudentSystemApiCs.Models
{
    /// <summary>
    /// Class that contains basic identity info for every model.
    /// </summary>
    public abstract class BaseModel
    {
        public int Id { get; set; }

        public override bool Equals(object obj)
        {
            var user = obj as BaseModel;
            return Equals(user);
        }

        protected bool Equals(BaseModel other)
        {
            return Id == other?.Id;
        }

        public override int GetHashCode()
        {
            return Id;
        }
    }

    /// <summary>
    /// Interface that exposes generic methods for binding models to database proxy classes.
    /// </summary>
    /// <typeparam name="T">Database model</typeparam>
    public interface ICrudModel<in T>
    {
        /// <summary>
        /// Edits the current database model with data from request
        /// </summary>
        /// <param name="model">Model received from request</param>
        /// <param name="ctx">Database context from module</param>
        /// <param name="token">Cancellation token for cancelling operations if request is cancelled</param>
        /// <returns>Awaitable task</returns>
        Task EditInstanceAsync(T model, UniContext ctx, CancellationToken token);
        /// <summary>
        /// Binds navigation properties to EF proxy models.
        /// </summary>
        /// <param name="ctx">Database context from module</param>
        /// <param name="token">Cancellation token for cancelling operations if request is cancelled</param>
        /// <returns></returns>
        Task BindInstanceAsync(UniContext ctx, CancellationToken token);
    }
}
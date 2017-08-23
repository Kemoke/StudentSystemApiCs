using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Models;
using StudentSystemApiCs.Util;
// ReSharper disable VirtualMemberCallInConstructor

namespace StudentSystemApiCs.Modules
{
    /// <summary>
    /// Generic implementation for crud routes
    /// </summary>
    /// <typeparam name="T">Database model</typeparam>
    public abstract class CrudModule<T> : SecureModule where T : BaseModel, ICrudModel<T>
    {
        protected T ModifiedItem { get; private set; }
        private string propName;
        private readonly string[] propVal;
        protected readonly UniContext context;

        protected CrudModule(string uri, UniContext context) : base(uri, typeof(Admin))
        {
            propVal = new string[2];
            this.context = context;
            Get("/", GetItemsAsync);
            Get("/with/{props}", GetItemsWithPropsAsync);
            Get("/where/{prop}={value}", GetItemsWherePropAsync);
            Get("/where/{prop}={value}/with/{props}", GetItemsWherePropWithPropsAsync);
            Get("/range/{value1}-{value2}", GetItemsRangeAsync);
            Get("/range/{value1}-{value2}/with/{props}", GetItemsRangeWithPropsAsync);
            Get("/range/{prop}={value1}-{value2}", GetItemsRangePropAsync);
            Get("/range/{prop}={value1}-{value2}/with/{props}", GetItemsRangePropWithPropsAsync);
            Get("/{id}", GetItemByIdAsync);
            Get("/{id}/with/{props}", GetItemByIdWithPropsAsync);
            Post("/", AddItemAsync);
            Put("/{id}", EditItemAsync);
            Delete("/{id}", DeleteItemAsync);
        }

        ~CrudModule()
        {
            context.Dispose();
        }

        /// <summary>
        /// Returns items in {value1}-{value2} range with included nagivation properties.
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>List of items in JSON format</returns>
        private async Task<dynamic> GetItemsRangeWithPropsAsync(dynamic param, CancellationToken cancellationToken)
        {
            int start = param.value1;
            int end = param.value2;
            DbQuery<T> depquery = null;
            var deps = context.Set<T>();
            var props = ((string)param.props).Split(',');
            for (var i = 0; i < props.Length; i++)
                depquery = i == 0 ? deps.Include(props[i]) : depquery.Include(props[i]);
            if (depquery == null)
                return Response.AsText("no props specified").WithStatusCode(HttpStatusCode.BadRequest);
            var items = depquery.OrderBy(i => i.Id).Skip(start).Take(end-start);
            await items.LoadAsync(cancellationToken);
            return Response.AsJson(items);
        }

        /// <summary>
        /// Returns items in {value1}-{value2} range
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>List of items in JSON format</returns>
        private async Task<dynamic> GetItemsRangeAsync(dynamic param, CancellationToken cancellationToken)
        {
            int start = param.value1;
            int end = param.value2;
            var items = context.Set<T>().OrderBy(i => i.Id).Skip(start).Take(end-start);
            await items.LoadAsync(cancellationToken);
            return Response.AsJson(items);
        }

        /// <summary>
        /// Deletes item with where id is equal to path {id} from database
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>'ok' on success</returns>
        private async Task<dynamic> DeleteItemAsync(dynamic param, CancellationToken cancellationToken)
        {
            int id = param.id;
            ModifiedItem = await context.Set<T>().FindAsync(cancellationToken, id);
            context.Set<T>().Remove(ModifiedItem);
            await context.SaveChangesAsync(cancellationToken);
            return Response.AsText("ok");
        }

        /// <summary>
        /// Modifies the item with specified id using json body from request
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>Modified item in JSON format</returns>
        private async Task<dynamic> EditItemAsync(dynamic param, CancellationToken cancellationToken)
        {
            int id = param.id;
            ModifiedItem = await context.Set<T>().FindAsync(cancellationToken, id);
            var item = this.Bind<T>();
            await ModifiedItem.EditInstanceAsync(item, context, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return Response.AsJson(ModifiedItem);
        }

        /// <summary>
        /// Adds a new item
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>Item in JSON format</returns>
        private async Task<dynamic> AddItemAsync(dynamic param, CancellationToken cancellationToken)
        {
            ModifiedItem = this.Bind<T>();
            await ModifiedItem.BindInstanceAsync(context, cancellationToken);
            ModifiedItem = context.Set<T>().Add(ModifiedItem);
            await context.SaveChangesAsync(cancellationToken);
            return Response.AsJson(ModifiedItem);
        }

        /// <summary>
        /// Gets item with specified id with requested navigation properties
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>Item in JSON format</returns>
        private async Task<dynamic> GetItemByIdWithPropsAsync(dynamic param, CancellationToken cancellationToken)
        {
            int id = param.id;
            var deps = context.Set<T>();
            DbQuery<T> depquery = null;
            var props = ((string) param.props).Split(',');
            for (var i = 0; i < props.Length; i++)
            {
                var prop = props[i];
                depquery = i == 0 ? deps.Include(prop) : depquery.Include(prop);
            }
            var item = await depquery.FirstAsync(t => t.Id == id, cancellationToken);
            return Response.AsJson(item);
        }

        /// <summary>
        /// Gets item with specified id
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>Item in JSON format</returns>
        private async Task<dynamic> GetItemByIdAsync(dynamic param, CancellationToken cancellationToken)
        {
            int id = param.id;
            var item = await context.Set<T>().FindAsync(cancellationToken, id);
            return Response.AsJson(item);
        }

        /// <summary>
        /// Gets all items from database where prop is in {val1}-{val2} range including navigation properties
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>List of items in JSON format</returns>
        private async Task<dynamic> GetItemsRangePropWithPropsAsync(dynamic param, CancellationToken cancellationToken)
        {
            string proop = param.prop;
            propName = char.ToUpper(proop[0]) + proop.Substring(1);
            propVal[0] = param.value1;
            propVal[1] = param.value2;
            var deps = context.Set<T>();
            DbQuery<T> depquery = null;
            var props = ((string) param.props).Split(',');
            for (var i = 0; i < props.Length; i++)
            {
                var prop = char.ToUpper(props[i][0]) + props[i].Substring(1);
                depquery = i == 0 ? deps.Include(prop) : depquery.Include(prop);
            }
            var query = depquery?.Where(i => Range(i));
            await query.LoadAsync(cancellationToken);
            return Response.AsJson(query);
        }

        /// <summary>
        /// Gets all items from database where prop is in {val1}-{val2} range
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>List of items in JSON format</returns>
        private async Task<dynamic> GetItemsRangePropAsync(dynamic param, CancellationToken cancellationToken)
        {
            string prop = param.prop;
            propName = char.ToUpper(prop[0]) + prop.Substring(1);
            propVal[0] = param.value1;
            propVal[1] = param.value2;
            var query = context.Set<T>().Where(i => Range(i));
            await query.LoadAsync(cancellationToken);
            return Response.AsJson(query);
        }

        /// <summary>
        /// Gets all items from database where prop is equal to specified value including navigation properties
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>List of items in JSON format</returns>
        private async Task<dynamic> GetItemsWherePropWithPropsAsync(dynamic param, CancellationToken cancellationToken)
        {
            string proop = param.prop;
            propName = char.ToUpper(proop[0]) + proop.Substring(1);
            propVal[0] = param.value;
            var deps = context.Set<T>();
            DbQuery<T> depquery = null;
            var props = ((string) param.props).Split(',');
            for (var i = 0; i < props.Length; i++)
            {
                var prop = char.ToUpper(props[i][0]) + props[i].Substring(1);
                depquery = i == 0 ? deps.Include(prop) : depquery.Include(prop);
            }
            var list = depquery?.Where(i => Contains(i));
            await list.LoadAsync(cancellationToken);
            return Response.AsJson(list);
        }

        /// <summary>
        /// Gets all items from database where prop is equal to specified value
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>List of items in JSON format</returns>
        private async Task<dynamic> GetItemsWherePropAsync(dynamic param, CancellationToken cancellationToken)
        {
            string prop = param.prop;
            propName = char.ToUpper(prop[0]) + prop.Substring(1);
            propVal[0] = param.value;
            var query = context.Set<T>().Where(i => Contains(i));
            await query.LoadAsync(cancellationToken);
            return Response.AsJson(query);
        }

        /// <summary>
        /// Gets all items from database including navigation properties
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>List of items in JSON format</returns>
        private async Task<dynamic> GetItemsWithPropsAsync(dynamic param, CancellationToken cancellationToken)
        {
            var deps = context.Set<T>();
            DbQuery<T> depquery = null;
            var props = ((string) param.props).Split(',');
            for (var i = 0; i < props.Length; i++)
            {
                var prop = char.ToUpper(props[i][0]) + props[i].Substring(1);
                depquery = i == 0 ? deps.Include(prop) : depquery.Include(prop);
            }
            await depquery.LoadAsync(cancellationToken);
            return Response.AsJson(depquery);
        }

        /// <summary>
        /// Gets all items from database
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="cancellationToken">Cancellation token if request is cancelled</param>
        /// <returns>List of items in JSON format</returns>
        private async Task<dynamic> GetItemsAsync(dynamic param, CancellationToken cancellationToken)
        {
            var deps = await context.Set<T>().ToListAsync(cancellationToken);
            return Response.AsJson(deps);
        }

        /// <summary>
        /// Comparator function to check if propery is within limits
        /// </summary>
        /// <param name="item">Item to be checked</param>
        /// <returns>True if property is within range</returns>
        private bool Range(T item)
        {
            var val1 = double.Parse(propVal[0]);
            var val2 = double.Parse(propVal[1]);
            var value = double.Parse(typeof(T).GetProperty(propName).GetValue(item).ToString());
            return value >= val1 && value <= val2;
        }

        /// <summary>
        /// Comparator function to check if property value is equal to specified
        /// </summary>
        /// <param name="item">Item to be checked</param>
        /// <returns>True if propery is equal</returns>
        private bool Contains(T item)
        {
            return typeof(T).GetProperty(propName).GetValue(item).ToString() == propVal[0];
        }

        /// <summary>
        /// Updates user cache when a new student or instructor is added/modified/deleted
        /// </summary>
        /// <param name="method">Request method</param>
        /// <param name="user">User to be modified</param>
        protected void UpdateUserCache(string method, User user)
        {
            switch (method)
            {
                case "PUT":
                    UserCache.Users[UserCache.Users.IndexOf(user)] = user;
                    break;
                case "POST":
                    UserCache.Users.Add(user);
                    break;
                case "DELETE":
                    UserCache.Users.Remove(user);
                    break;
            }
        }
    }

    public class DepartmentModule : CrudModule<Department>
    {
        public DepartmentModule(UniContext context) : base("/department", context)
        {
        }
    }

    public class ProgramModule : CrudModule<Program>
    {
        public ProgramModule(UniContext context) : base("/program", context)
        {
            Post("/{id}/curriculum/", SetCurriculumAsync);
        }

        /// <summary>
        /// Sets program with specified id's curriculum based on body data
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="token">Cancellation token if request is cancelled</param>
        /// <returns>Curriculum in JSON format</returns>
        private async Task<object> SetCurriculumAsync(dynamic param, CancellationToken token)
        {
            var curriculum = this.Bind<List<CurriculumCourse>>();
            int id = param.id;
            var program = await context.Programs.Include("curriculum").FirstAsync(p => p.Id == id, token);
            foreach (var curriculumCourse in curriculum)
            {
                curriculumCourse.Program = program;
                curriculumCourse.Course = await context.Courses.FindAsync(token, curriculumCourse.Course.Id);
            }
            program.Curriculum.Clear();
            program.Curriculum.AddRange(curriculum);
            await context.SaveChangesAsync(token);
            return Response.AsJson(program.Curriculum);
        }
    }

    public class InstructorModule : CrudModule<Instructor>
    {
        public InstructorModule(UniContext context) : base("/instructor", context)
        {
            After += ctx =>
            {
                UpdateUserCache(ctx.Request.Method, ModifiedItem);
            };
        }
    }

    public class StudentModule : CrudModule<Student>
    {
        public StudentModule(UniContext context) : base("/student", context)
        {
            After += ctx =>
            {
                UpdateUserCache(ctx.Request.Method, ModifiedItem);
            };
        }
    }

    public class CourseModule : CrudModule<Course>
    {
        public CourseModule(UniContext context) : base("/course", context)
        {
        }
    }

    public class SectionModule : CrudModule<Section>
    {
        public SectionModule(UniContext context) : base("/section", context)
        {
        }
    }
}
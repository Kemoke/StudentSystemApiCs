using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Models;

namespace StudentSystemApiCs.Modules
{
    public sealed class StudentActionModule : SecureModule
    {
        private readonly UniContext context;
        public StudentActionModule(UniContext context) : base("/student", typeof(Student))
        {
            this.context = context;
            Post("/register", RegisterSectionAsync);
            Post("/unregister", UnregisterSectionAsync);
            Get("/sections/registered", GetRegisteredSectionsAsync);
            Get("/courses", GetAvailableCoursesAsync);
            Get("/grades", GetGradesAsync);
        }

        /// <summary>
        /// Gets all registered sections for logged in student
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>List of sections in JSON format</returns>
        private async Task<object> GetRegisteredSectionsAsync(dynamic param, CancellationToken token)
        {
            var student = await GetStudentAsync(token);
            return Response.AsJson(student.Sections);
        }

        /// <summary>
        /// Gets all grades for logged in student
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>List of StudentGrades in JSON format</returns>
        private async Task<object> GetGradesAsync(dynamic param, CancellationToken token)
        {
            var student = await GetStudentAsync(token);
            return student == null ? Response.AsText("User is null !?!?!").WithStatusCode(HttpStatusCode.InternalServerError) : Response.AsJson(student.Grades);
        }

        /// <summary>
        /// Gets logged in student and binds navigation properties
        /// </summary>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>Logged in student</returns>
        private async Task<Student> GetStudentAsync(CancellationToken token)
        {
            var id = Convert.ToInt32(Context.CurrentUser.Identities.First().Claims.First().Value);
            return await context.Students
                .Include("Sections.Course")
                .Include("Sections.Instructor")
                .Include("Sections.TimeTable")
                .Include("Grades.GradeType.Section.Course")
                .Include("Program")
                .FirstAsync(u => u.Id == id, token);
        }

        /// <summary>
        /// Gets all available courses for registration for logged in student
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>List of courses in JSON format</returns>
        private async Task<object> GetAvailableCoursesAsync(dynamic param, CancellationToken token)
        {
            var student = await GetStudentAsync(token);
            if (student == null)
                return Response.AsText("User is null !?!?!").WithStatusCode(HttpStatusCode.InternalServerError);
            var courses = (await context.Programs.Include("Curriculum.Course.Sections.Instructor").Include("Curriculum.Course.Sections.TimeTable").FirstAsync(p => p.Id == student.Program.Id, token)).Curriculum
                .Where(cc => cc.Year <= student.Year
                             && cc.Semester%2 == student.Semester%2
                             && !student.Sections.Select(s => s.Course)
                                 .Contains(cc.Course)).ToList();
            var output = new List<CurriculumCourse>();
            courses.ForEach(c =>
            {
                output.Add(new CurriculumCourse
                {
                    Course = new Course
                    {
                        Code = c.Course.Code,
                        Name = c.Course.Name,
                        Sections = c.Course.Sections,
                    },
                    Id = c.Id,
                    Semester = c.Semester,
                    Year = c.Year,
                    Elective = c.Elective
                });
            });
            return Response.AsJson(output);
        }

        /// <summary>
        /// Unregisters a specified section
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>Unregistered section in JSON format</returns>
        private async Task<object> UnregisterSectionAsync(dynamic param, CancellationToken token)
        {
            var student = await GetStudentAsync(token);
            if (student == null)
                return Response.AsText("User is null !?!?!").WithStatusCode(HttpStatusCode.InternalServerError);
            var id = this.Bind<Section>().Id;
            var section = await context.Sections
                .Include("Students")
                .Include("TimeTable")
                .Include("Course")
                .Include("Instructor").FirstAsync(s => s.Id == id, token);
            if (!student.Sections.Contains(section))
                return
                    Response.AsText("You are not registered with this section")
                        .WithStatusCode(HttpStatusCode.BadRequest);
            student.Sections.Remove(section);
            await context.SaveChangesAsync(token);
            return Response.AsJson(section);
        }

        /// <summary>
        /// Registers a specified section
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>Section in JSON format</returns>
        private async Task<object> RegisterSectionAsync(dynamic param, CancellationToken token)
        {
            var student = await GetStudentAsync(token);
            if (student == null)
                return Response.AsText("User is null !?!?!").WithStatusCode(HttpStatusCode.InternalServerError);
            var id = this.Bind<Section>().Id;
            var section = await context.Sections
                .Include("Students")
                .Include("TimeTable")
                .Include("Course")
                .Include("Instructor")
                .FirstAsync(s => s.Id == id, token);
            if (section.Capacity <= section.Students.Count)
                return Response.AsText("Section is full").WithStatusCode(HttpStatusCode.BadRequest);
            student.Sections.Add(section);
            await context.SaveChangesAsync(token);
            return Response.AsJson(section);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Newtonsoft.Json;
using StudentSystemApiCs.DAO;
using StudentSystemApiCs.Models;

namespace StudentSystemApiCs.Modules
{
    public sealed class InstructorActionModule : SecureModule
    {
        private readonly UniContext context;

        public InstructorActionModule(UniContext context) : base("/instructor", typeof(Instructor))
        {
            this.context = context;
            Get("/sections", GetSectionsAsync);
            Get("/section/{id}/students", GetSectionStudentsAsync);
            Get("/gradetypes/{id}", GetGradeTypesAsync);
            Post("/gradetypes/{id}", SetGradeTypesAsync);
            Get("/grades/{id}", GetStudentGradesAsync);
            Post("/grade/{id}", SetStudentGradeAsync);
        }

        /// <summary>
        /// Gets list of students in specified section
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>List of students in JSON format</returns>
        private async Task<object> GetSectionStudentsAsync(dynamic param, CancellationToken token)
        {
            int id = param.id;
            var section = await context.Sections
                .Include("Students.Program")
                .Include("Students.Grades.GradeType")
                .FirstAsync(i => i.Id == id, token);
            return Response.AsJson(section.Students);
        }

        /// <summary>
        /// Gets list of grade types for specified section
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>List of GradeTypes in JSON format</returns>
        private async Task<object> GetGradeTypesAsync(dynamic param, CancellationToken token)
        {
            int id = param.id;
            var section = await context.Sections.Include("GradeTypes").FirstAsync(i => i.Id == id, token);
            return Response.AsJson(section.GradeTypes);
        }

        /// <summary>
        /// Gets list of sections for logged in instructor
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>List of sections in JSON format</returns>
        private async Task<object> GetSectionsAsync(dynamic param, CancellationToken token)
        {
            return Response.AsJson((await GetInstructorAsync(token)).Sections);
        }

        /// <summary>
        /// Gets instructor from database and binds navigation properties.
        /// </summary>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>Logged in instructor</returns>
        private async Task<Instructor> GetInstructorAsync(CancellationToken token)
        {
            var id = Convert.ToInt32(Context.CurrentUser.Identities.First().Claims.First().Value);
            return await context.Instructors
                .Include("Sections.Course")
                .Include("Sections.TimeTable")
                .FirstAsync(i => i.Id == id, token);
        }

        /// <summary>
        /// Sets student grade
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>Grade in JSON format</returns>
        private async Task<object> SetStudentGradeAsync(dynamic param, CancellationToken token)
        {
            var grade = this.Bind<StudentGrade>();
            await grade.BindInstanceAsync(context, token);
            int id = param.id;
            var student = await context.Students.Include("Grades.GradeType").FirstAsync(s => s.Id == id, token);
            if (grade.Id != 0)
            {
                student.Grades.First(g => g.Id == grade.Id).Score = grade.Score;
            }
            else
            {
                student.Grades.Add(grade);
            }
            await context.SaveChangesAsync(token);
            return Response.AsJson(grade);
        }

        /// <summary>
        /// Gets list of student grades
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>List of grades in JSON format</returns>
        private async Task<object> GetStudentGradesAsync(dynamic param, CancellationToken token)
        {
            int id = param.id;
            var student = await context.Students.Include("Grades.GradeType").FirstAsync(s => s.Id == id, token);
            return Response.AsJson(student.Grades);
        }

        /// <summary>
        /// Sets grade types for section
        /// </summary>
        /// <param name="param">Uri parameters</param>
        /// <param name="token">Token for cancelling operations</param>
        /// <returns>Section in JSON format</returns>
        private async Task<object> SetGradeTypesAsync(dynamic param, CancellationToken token)
        {
            var gradeTypes = this.Bind<List<GradeType>>();
            int id = param.id;
            var section = await context.Sections.Include("GradeTypes").FirstAsync(s => s.Id == id, token);
            foreach (var gradeType in gradeTypes)
            {
                if (gradeType.Id != 0)
                {
                    var grt = section.GradeTypes.First(gt => gt.Id == gradeType.Id);
                    grt.Name = gradeType.Name;
                    grt.Value = gradeType.Value;
                }
                else
                {
                    gradeType.Section = null;
                    section.GradeTypes.Add(gradeType);
                }
            }
            await context.SaveChangesAsync(token);
            return Response.AsJson(section);
        }
    }
}
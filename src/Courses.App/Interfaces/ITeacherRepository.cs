using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Courses.App.Models;

namespace Courses.App.Interfaces
{
    public interface ITeacherRepository
    {
        Task<List<Teacher>> GetAllTeachersAsync();
        Task<List<Teacher>> SearchTeachersAsync(string query);
        void AddTeacher(Teacher teacher);
        void UpdateTeacher(Teacher teacher);
        void DeleteTeacher(Teacher teacher);
        Task NullifyGroupTeacherAsync(Guid teacherId);
    }
}
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Courses.App.Models;

namespace Courses.App.Interfaces
{
    public interface ITeacherRepository
    {
        Task<List<Teacher>> GetAllTeachersAsync();
        Task AddTeacherAsync(Teacher teacher);
        Task UpdateTeacherAsync(Teacher teacher);
        Task DeleteTeacherAsync(Teacher teacher);
        Task NullifyGroupTeacherAsync(Guid teacherId);
    }
}
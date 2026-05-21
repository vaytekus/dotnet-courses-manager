using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Courses.App.Models;

namespace Courses.App.Interfaces
{
    public interface IStudentRepository
    {
        Task<List<Student>> GetAllStudentsAsync();
        Task<Student?> GetStudentByIdAsync(Guid id);
        Task DeleteAllStudentsByGroupAsync(Guid groupId);
        Task AddStudentsRangeAsync(List<Student> students);
        Task RemoveStudentAsync(Student student);
        Task AddStudentAsync(Student student);
        Task UpdateStudentAsync(Student student);
    }
}
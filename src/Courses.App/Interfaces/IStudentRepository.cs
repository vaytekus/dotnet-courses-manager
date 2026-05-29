using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Courses.App.Models;

namespace Courses.App.Interfaces
{
    public interface IStudentRepository
    {
        Task<List<Student>> GetAllStudentsAsync();
        Task<List<Student>> SearchStudentsAsync(string query);
        Task<List<Student>> GetFilteredStudentsAsync(string searchQuery, Guid? groupId);
        Task<Student?> GetStudentByIdAsync(Guid id);
        Task DeleteAllStudentsByGroupAsync(Guid groupId);
        void AddStudentsRange(List<Student> students);
        void RemoveStudent(Student student);
        void AddStudent(Student student);
        void UpdateStudent(Student student);
    }
}
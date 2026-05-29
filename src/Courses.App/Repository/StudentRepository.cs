using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Courses.App.Data;
using Courses.App.Interfaces;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly AppDbContext _context;

        public StudentRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            return await _context.Students
                .Include(g => g.Group)
                .ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(Guid id)
        {
            return await _context.Students.FindAsync(id);
        }

        public async Task DeleteAllStudentsByGroupAsync(Guid groupId)
        {
            var students = await _context.Students
                .Where(s => s.GroupId == groupId)
                .ToListAsync();
            _context.Students.RemoveRange(students);
        }

        public void AddStudentsRange(List<Student> students)
        {
            _context.Students.AddRange(students);
        }

        public void RemoveStudent(Student student)
        {
            _context.Students.Remove(student);
        }

        public void AddStudent(Student student)
        {
            _context.Students.Add(student);
        }

        public void UpdateStudent(Student student)
        {
            _context.Students.Update(student);
        }

        public async Task<List<Student>> SearchStudentsAsync(string query)
        {
            return await _context.Students
                .Include(g => g.Group)
                .Where(s => s.FirstName.Contains(query) || s.LastName.Contains(query) || s.Group.Name.Contains(query))
                .ToListAsync();
        }

        public async Task<List<Student>> GetFilteredStudentsAsync(string searchQuery, Guid? groupId)
        {
            var query = _context.Students
                .Include(s => s.Group)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(s =>
                    s.FirstName.Contains(searchQuery) ||
                    s.LastName.Contains(searchQuery));
            }

            if (groupId.HasValue)
            {
                query = query.Where(s => s.GroupId == groupId);
            }

            return await query.ToListAsync();
        }
    }
}

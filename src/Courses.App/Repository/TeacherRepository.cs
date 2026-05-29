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
    public class TeacherRepository : ITeacherRepository
    {
        private readonly AppDbContext _context;

        public TeacherRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Teacher>> GetAllTeachersAsync()
        {
            return await _context.Teachers.ToListAsync();
        }

        public void AddTeacher(Teacher teacher)
        {
            _context.Teachers.Add(teacher);
        }

        public void UpdateTeacher(Teacher teacher)
        {
            _context.Teachers.Update(teacher);
        }

        public void DeleteTeacher(Teacher teacher)
        {
            _context.Teachers.Remove(teacher);
        }

        public async Task NullifyGroupTeacherAsync(Guid teacherId)
        {
            var groups = await _context.Groups
                .Where(g => g.TeacherId == teacherId)
                .ToListAsync();
            foreach (var g in groups)
            {
                g.TeacherId = null;
            }
        }
        
        public async Task<List<Teacher>> SearchTeachersAsync(string query)
        {
            return await _context.Teachers
                .Where(t => t.FirstName.Contains(query) || t.LastName.Contains(query))
                .ToListAsync();
        }
    }
}

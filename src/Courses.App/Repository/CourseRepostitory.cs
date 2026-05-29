using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Courses.App.Data;
using Courses.App.Interfaces;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.Repository
{
    public class CourseRepository : ICourseRepository
    {
        private readonly AppDbContext _context;

        public CourseRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            return await _context.Courses.ToListAsync();
        }

        public async Task<List<Course>> GetAllCoursesWithDetailsAsync()
        {
            return await _context.Courses
                .Include(c => c.Groups)
                .ThenInclude(g => g.Students)
                .AsSplitQuery()
                .ToListAsync();
        }

        public async Task<List<Course>> SearchCoursesAsync(string query)
        {
            return await _context.Courses
                .Include(c => c.Groups)
                .ThenInclude(g => g.Students)
                .Where(c => c.Name.Contains(query))
                .AsSplitQuery()
                .ToListAsync();
        }
    }
}

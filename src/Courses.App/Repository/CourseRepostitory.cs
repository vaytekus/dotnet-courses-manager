using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Courses.App.Data;
using Courses.App.Interfaces;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.Repository
{
    public class CourseRepository : ICourseRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;
        
        public CourseRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        
        public async Task<List<Course>> GetAllCoursesAsync()
        {
            if (_dbContextFactory is null)
            {
                throw new InvalidOperationException("DbContextFactory is null"); 
            };
            
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Courses.ToListAsync();
        }

        public async Task<List<Course>> GetAllCoursesWithDetailsAsync()
        {
            if (_dbContextFactory is null)
            {
                throw new InvalidOperationException("DbContextFactory is null"); 
            };
            
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Courses
                .Include(c => c.Groups)
                .ThenInclude(g => g.Students)
                .AsSplitQuery()
                .ToListAsync();
        }
    }
}
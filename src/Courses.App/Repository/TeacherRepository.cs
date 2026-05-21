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
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public TeacherRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Teacher>> GetAllTeachersAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Teachers.ToListAsync();
        }

        public async Task AddTeacherAsync(Teacher teacher)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Teachers.Add(teacher);
            await db.SaveChangesAsync();
        }

        public async Task UpdateTeacherAsync(Teacher teacher)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Teachers.Update(teacher);
            await db.SaveChangesAsync();
        }

        public async Task DeleteTeacherAsync(Teacher teacher)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Teachers.Remove(teacher);
            await db.SaveChangesAsync();
        }

        public async Task NullifyGroupTeacherAsync(Guid teacherId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            await db.Groups
                .Where(g => g.TeacherId == teacherId)
                .ExecuteUpdateAsync(s => s.SetProperty(g => g.TeacherId, (Guid?)null));
        }
    }
}
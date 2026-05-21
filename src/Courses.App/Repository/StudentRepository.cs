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
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public StudentRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<List<Student>> GetAllStudentsAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Students
                .Include(g => g.Group)
                .ToListAsync();
        }

        public async Task<Student?> GetStudentByIdAsync(Guid id)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Students.FindAsync(id);
        }

        public async Task DeleteAllStudentsByGroupAsync(Guid groupId)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            await db.Students.Where(s => s.GroupId == groupId).ExecuteDeleteAsync();
        }

        public async Task AddStudentsRangeAsync(List<Student> students)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Students.AddRange(students);
            await db.SaveChangesAsync();
        }

        public async Task RemoveStudentAsync(Student student)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Students.Remove(student);
            await db.SaveChangesAsync();
        }

        public async Task AddStudentAsync(Student student)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Students.Add(student);
            await db.SaveChangesAsync();
        }

        public async Task UpdateStudentAsync(Student student)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Students.Update(student);
            await db.SaveChangesAsync();
        }
    }
}
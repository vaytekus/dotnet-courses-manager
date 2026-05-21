using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Courses.App.Data;
using Courses.App.Interfaces;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.Repository
{
    public class GroupRepository : IGroupRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public GroupRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }
        
        public async Task<List<Group>> GetAllGroupsWidthDetailsAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Groups
                .Include(g => g.Teacher)
                .Include(g => g.Students)
                .Include(g => g.Course)
                .AsSplitQuery()
                .ToListAsync();
        }
        
        public async Task<List<Group>> GetAllGroupsAsync()
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Groups.ToListAsync();
        }

        public async Task AddGroupAsync(Group group)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Groups.Add(group);
            await db.SaveChangesAsync();
        }

        public async Task<Group?> GetByIdAsync(Guid id)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            return await db.Groups.FindAsync(id);
        }

        public async Task UpdateGroupAsync(Group group)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Groups.Update(group);
            await db.SaveChangesAsync();
        }
        
        public async Task DeleteGroupAsync(Group group)
        {
            await using var db = await _dbContextFactory.CreateDbContextAsync();
            db.Groups.Remove(group);
            await db.SaveChangesAsync();
        }
    }
}
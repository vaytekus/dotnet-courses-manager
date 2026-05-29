using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Courses.App.Data;
using Courses.App.Enums;
using Courses.App.Interfaces;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.Repository
{
    public class GroupRepository : IGroupRepository
    {
        private readonly AppDbContext _context;

        public GroupRepository(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<List<Group>> GetAllGroupsWithDetailsAsync()
        {
            return await _context.Groups
                .Include(g => g.Teacher)
                .Include(g => g.Students)
                .Include(g => g.Course)
                .AsSplitQuery()
                .ToListAsync();
        }
        
        public async Task<List<Group>> GetAllGroupsAsync()
        {
            return await _context.Groups.ToListAsync();
        }
        
        public async Task<Group?> GetByIdAsync(Guid id)
        {
            return await _context.Groups.FindAsync(id);
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void UpdateGroup(Group group)
        {
            _context.Groups.Update(group);
        }

        public void DeleteGroup(Group group)
        {
            _context.Groups.Remove(group);
        }

        public async Task<List<Group>> SearchGroupsAsync(string query)
        {
            return await _context.Groups
                .Include(g => g.Teacher)
                .Include(g => g.Course)
                .Include(g => g.Students)
                .Where(g => g.Name.Contains(query))
                .ToListAsync();
        }

        public async Task<List<Group>> GetFilteredGroupsAsync(
            string searchQuery, Guid? courseId, GroupStudentFilter studentFilter)
        {
            var query = _context.Groups
                .Include(g => g.Teacher)
                .Include(g => g.Course)
                .Include(g => g.Students)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(g => g.Name.Contains(searchQuery));
            }

            if (courseId.HasValue)
            {
                query = query.Where(g => g.CourseId == courseId);
            }

            if (studentFilter == GroupStudentFilter.WithStudents)
            {
                query = query.Where(g => g.Students.Any());
            }
            else if (studentFilter == GroupStudentFilter.WithoutStudents)
            {
                query = query.Where(g => !g.Students.Any());
            }

            return await query.ToListAsync();
        }
    }
}
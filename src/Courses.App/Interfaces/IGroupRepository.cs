using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Courses.App.Enums;
using Courses.App.Models;

namespace Courses.App.Interfaces
{
    public interface IGroupRepository
    {
        Task<List<Group>> GetAllGroupsWithDetailsAsync();
        Task<List<Group>> GetAllGroupsAsync();
        Task<List<Group>> SearchGroupsAsync(string query);
        Task<Group?> GetByIdAsync(Guid id);
        Task<List<Group>> GetFilteredGroupsAsync(string searchQuery, Guid? courseId, GroupStudentFilter studentFilter);
        void AddGroup(Group group);
        void UpdateGroup(Group group);
        void DeleteGroup(Group group);
    }
}
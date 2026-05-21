using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Courses.App.Models;

namespace Courses.App.Interfaces
{
    public interface IGroupRepository
    {
        Task<List<Group>> GetAllGroupsWidthDetailsAsync();
        Task<List<Group>> GetAllGroupsAsync();
        Task<Group?> GetByIdAsync(Guid id);
        Task AddGroupAsync(Group group);
        Task UpdateGroupAsync(Group group);
        Task DeleteGroupAsync(Group group);
    }
}
using System;
using System.Threading.Tasks;

namespace Courses.App.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        ICourseRepository Courses { get; }
        IGroupRepository Groups { get; }
        IStudentRepository Students { get; }
        ITeacherRepository Teachers { get; }
        Task SaveAsync();
    }
}
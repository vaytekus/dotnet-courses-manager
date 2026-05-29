using System.Threading.Tasks;
using Courses.App.Interfaces;
using Courses.App.Repository;

namespace Courses.App.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public ICourseRepository Courses { get; }
        public IGroupRepository Groups { get; }
        public IStudentRepository Students { get; }
        public ITeacherRepository Teachers { get; }

        public UnitOfWork(
            AppDbContext context,
            ICourseRepository courses,
            IGroupRepository groups,
            IStudentRepository students,
            ITeacherRepository teachers)
        {
            _context = context;
            Courses = courses;
            Groups = groups;
            Students = students;
            Teachers = teachers;
        }
        
        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using Courses.App.Models;

namespace Courses.App.Interfaces
{
    public interface ICourseRepository
    {
        Task<List<Course>> GetAllCoursesAsync();
        Task<List<Course>> GetAllCoursesWithDetailsAsync();
    }
}
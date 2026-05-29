using System;
using System.Collections.Generic;

namespace Courses.App.Models
{
    public class Group
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public Guid CourseId { get; set; }
        public Course Course { get; set; } = null!;
        public Guid? TeacherId { get; set; }
        public Teacher? Teacher { get; set; }
        public ICollection<Student> Students { get; set; } = new List<Student>();
    }
}
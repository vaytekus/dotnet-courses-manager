using System;

namespace Courses.App.Models
{
    public class Student
    {
        public Guid Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        public Guid GroupId { get; set; }
        public Group Group { get; set; } = null!;
    }
}
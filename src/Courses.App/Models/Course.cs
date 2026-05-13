using System;
using System.Collections.Generic;

namespace Courses.App.Models
{
    public class Course
    {
        public Guid Id { get; set; }
        
        public required string Name { get; set; }
        
        public required string Description { get; set; }
        
        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
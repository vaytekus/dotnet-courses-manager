using Courses.App.Enums;

namespace Courses.App.Models
{
    public class StudentFilterOption
    {
        public GroupStudentFilter Value { get; init; }
        public string Label { get; init; } = "";

        public override string ToString() => Label;
    }
}
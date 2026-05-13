using Courses.App.Models;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<Group> Groups => Set<Group>();
        public DbSet<Teacher> Teachers => Set<Teacher>();
        public DbSet<Student> Students => Set<Student>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Course>()
                .Property(c => c.Name)
                .IsRequired().HasMaxLength(100);
            
            modelBuilder.Entity<Course>()
                .Property(c => c.Description)
                .IsRequired().HasMaxLength(500);

            modelBuilder.Entity<Group>()
                .Property(g => g.Name)
                .IsRequired().HasMaxLength(100);
            
            modelBuilder.Entity<Group>()
                .HasOne(g => g.Course)
                .WithMany(c => c.Groups)
                .HasForeignKey(g => g.CourseId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Teacher>()
                .Property(t => t.FirstName).IsRequired().HasMaxLength(50);
            
            modelBuilder.Entity<Teacher>()
                .Property(t => t.LastName).IsRequired().HasMaxLength(50);
            
            modelBuilder.Entity<Group>()
                .HasOne(g => g.Teacher)
                .WithMany(t => t.Groups)
                .HasForeignKey(g => g.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Student>()
                .Property(s => s.FirstName)
                .IsRequired().HasMaxLength(50);

            modelBuilder.Entity<Student>()
                .Property(s => s.LastName)
                .IsRequired().HasMaxLength(50);

            modelBuilder.Entity<Student>()
                .HasOne(s => s.Group)
                .WithMany(g => g.Students)
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Courses.App.Models;
using Microsoft.EntityFrameworkCore;

namespace Courses.App.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            await db.Database.MigrateAsync();

            if (!await db.Teachers.AnyAsync())
            {                                                                                            
                db.AddRange(
                    new Teacher { Id = Guid.NewGuid(), FirstName = "Oleg", LastName = "Shevchenko" },
                    new Teacher { Id = Guid.NewGuid(), FirstName = "Iryna", LastName = "Bondarenko" }
                );
                await db.SaveChangesAsync(); 
            }
            
            if(await db.Courses.AnyAsync()) return;

            var teacher1 = await db.Teachers.FirstAsync();
            var teacher2 = await db.Teachers.Skip(1).FirstAsync();

            var csharp = new Course
            {
                Id = Guid.NewGuid(), Name = "C# Basics", Description = "Intro to C#"
            };

            var sql = new Course
            {
                Id = Guid.NewGuid(), Name = "SQL", Description = "Relational databases"
            };
            
            var groupA = new Group { Id = Guid.NewGuid(), Name = "C#-Spring-2026", Course = csharp, Teacher = teacher1 };
            var groupB = new Group { Id = Guid.NewGuid(), Name = "SQL-Summer-2026", Course = sql, Teacher = teacher2 };
            
            groupA.Students.Add(new Student()
            {
                Id = Guid.NewGuid(), FirstName = "Ivan", LastName = "Petrov"
            });
            groupA.Students.Add(new Student()
            {
                Id = Guid.NewGuid(), FirstName = "Olena", LastName = "Koval"
            });
            
            groupB.Students.Add(new Student()
            {
                Id = Guid.NewGuid(), FirstName = "Andrii", LastName = "Sydorenko"
            });
            
            db.AddRange(csharp, sql, groupA, groupB);
            
            await db.SaveChangesAsync();
        }
    }
}
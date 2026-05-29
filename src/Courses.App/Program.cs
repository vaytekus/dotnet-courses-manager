using Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using Courses.App.Data;
using Courses.App.Helper;
using Courses.App.Interfaces;
using Courses.App.Models;
using Courses.App.Repository;
using Courses.App.Services;
using Courses.App.ViewModels;
using Courses.App.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Courses.App;

internal sealed class Program
{
    private const string _logsFolder = "Logs";
    private const string _logFileName = "log-.txt";
    private const int _retainedLogFileCount = 10;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var config = ConfigurationHelper.Build();

        var logPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", _logsFolder, _logFileName);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: _retainedLogFileCount,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();
        
        var connectionString = config.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json");
        var services = new ServiceCollection();
        services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString));
        services.AddSingleton<MainWindowViewModel>();
        services.AddTransient<GroupsManagementViewModel>();
        services.AddTransient<StudentsManagementViewModel>();
        services.AddTransient<TeachersManagementViewModel>();
        
        services.AddTransient<MainWindowView>();
        services.AddTransient<GroupsManagementView>();
        services.AddTransient<StudentsManagementView>();
        services.AddTransient<TeachersManagementView>();
        
        services.AddTransient<Func<GroupsManagementView>>(
            sp => () => sp.GetRequiredService<GroupsManagementView>());
        services.AddTransient<Func<StudentsManagementView>>(
            sp => () => sp.GetRequiredService<StudentsManagementView>());
        services.AddTransient<Func<TeachersManagementView>>(
            sp => () => sp.GetRequiredService<TeachersManagementView>());

        services.AddTransient<Func<Student, IReadOnlyList<Group>, StudentItemViewModel>>(sp =>
            (student, groups) => new StudentItemViewModel(
                student,
                groups,
                sp.GetRequiredService<IUnitOfWork>(),
                sp.GetRequiredService<ILogger<StudentItemViewModel>>()));

        services.AddTransient<Func<Teacher, TeacherItemViewModel>>(sp =>
            teacher => new TeacherItemViewModel(
                teacher,
                sp.GetRequiredService<IUnitOfWork>(),
                sp.GetRequiredService<ILogger<TeacherItemViewModel>>()));

        services.AddTransient<Func<Group, IReadOnlyList<Teacher>, GroupItemViewModel>>(sp =>
            (group, teachers) => new GroupItemViewModel(
                group,
                teachers,
                sp.GetRequiredService<IUnitOfWork>(),
                sp.GetRequiredService<ILogger<GroupItemViewModel>>(),
                sp.GetRequiredService<IExportService>()));
        
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

        services.AddScoped<AppDbContext>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IGroupRepository, GroupRepository>();
        services.AddScoped<IStudentRepository, StudentRepository>();
        services.AddScoped<ITeacherRepository, TeacherRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IExportService, ExportService>();
        
        var serviceProvider = services.BuildServiceProvider();
        using (var db = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext())
        {
            DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        }

        App.Services = serviceProvider;

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
            Log.Fatal(e.ExceptionObject as Exception, "Unhandled exception");

        try
        {
            Log.Information("Launching app...");
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            Log.Fatal(e, "Critical startup error");
        }
        finally {
            Log.CloseAndFlush();
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
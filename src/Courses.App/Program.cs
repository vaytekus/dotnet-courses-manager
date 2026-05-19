using Avalonia;
using System;
using System.IO;
using Courses.App.Data;
using Courses.App.Helper;
using Courses.App.ViewModels;
using Courses.App.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Courses.App;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var config = ConfigurationHelper.Build();

        var logPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Logs", "log-.txt");

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .WriteTo.File(
                path: logPath,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 10,
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
        
        services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
        
        var serviceProvider = services.BuildServiceProvider();
        using (var db = serviceProvider.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext())
        {
            DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        }

        App.Services = serviceProvider;

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
        finally{
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
using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Courses.App.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Courses.App;

public partial class App : Application
{
    public static IServiceProvider Services { get; set; } = null!;
    
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = Services.GetRequiredService<MainWindowView>();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
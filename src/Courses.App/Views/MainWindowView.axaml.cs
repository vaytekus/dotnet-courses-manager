using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Courses.App.ViewModels;

namespace Courses.App.Views;

public partial class MainWindowView : Window
{
    private readonly Func<GroupsManagementView>? _groupsViewFactory;
    private readonly Func<StudentsManagementView>? _studentsViewFactory;
    private readonly Func<TeachersManagementView>? _teachersViewFactory;
    
    public MainWindowView() { }

    public MainWindowView(
        MainWindowViewModel viewModel,
        Func<GroupsManagementView> groupsViewFactory,
        Func<StudentsManagementView> studentsViewFactory,
        Func<TeachersManagementView> teachersViewFactory)
    {
        InitializeComponent();
        DataContext = viewModel;
        _groupsViewFactory = groupsViewFactory;
        _studentsViewFactory = studentsViewFactory;
        _teachersViewFactory = teachersViewFactory;
    }

    private async void OnManageGroupsClick(object sender, RoutedEventArgs e)
    {
        var dialog = _groupsViewFactory!();
        dialog.Opened += (_, _) => dialog.Activate();
        await dialog.ShowDialog(this);

        if (DataContext is MainWindowViewModel vm) await vm.ReloadSync();
    }

    private async void OnManageStudentsClick(object sender, RoutedEventArgs e)
    {
        var dialog = _studentsViewFactory!();
        dialog.Opened += (_, _) => dialog.Activate();
        await dialog.ShowDialog(this);
        
        if (DataContext is MainWindowViewModel vm) await vm.ReloadSync();
    }

    private async void OnManageTeachersClick(object sender, RoutedEventArgs e)
    {
        var dialog = _teachersViewFactory!();
        dialog.Opened += (_, _) => dialog.Activate();
        await dialog.ShowDialog(this);
        
        if (DataContext is MainWindowViewModel vm) await vm.ReloadSync();
    }
}
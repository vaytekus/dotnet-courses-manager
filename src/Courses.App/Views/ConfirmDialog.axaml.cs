using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Courses.App.Views;

public partial class ConfirmDialog : Window
{
    public ConfirmDialog() {}
    
    private ConfirmDialog(string message)
    {
        InitializeComponent();
        MessageText.Text = message;
    }

    public static Task<bool> ShowAsync(Window owner, string message) =>
        new ConfirmDialog(message).ShowDialog<bool>(owner);

    private void OnYesClick(object? sender, RoutedEventArgs e) => Close(true);
    private void OnNoClick(object? sender, RoutedEventArgs e) => Close(false);
}
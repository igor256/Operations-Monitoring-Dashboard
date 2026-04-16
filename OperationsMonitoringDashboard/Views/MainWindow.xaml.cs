using System.Windows;
using OperationsMonitoringDashboard.ViewModels;

namespace OperationsMonitoringDashboard.Views;

/// <summary>
/// Main application shell for the operations monitoring dashboard.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>
    /// Initializes UI components and assigns the main view model.
    /// </summary>
    public MainWindow()
    {
        DataContext = new MainViewModel();
        InitializeComponent();
    }
}

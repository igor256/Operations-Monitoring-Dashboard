using System.Windows.Input;

namespace OperationsMonitoringDashboard.Commands;

/// <summary>
/// Provides a reusable command wrapper for synchronous actions.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?>? _canExecute;

    /// <summary>
    /// Initializes a new command with execution and availability delegates.
    /// </summary>
    /// <param name="execute">Action to execute when command is invoked.</param>
    /// <param name="canExecute">Optional predicate indicating whether command is available.</param>
    public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public bool CanExecute(object? parameter)
    {
        return _canExecute == null || _canExecute(parameter);
    }

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        _execute(parameter);
    }

    /// <summary>
    /// Requests WPF to reevaluate command availability.
    /// </summary>
    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

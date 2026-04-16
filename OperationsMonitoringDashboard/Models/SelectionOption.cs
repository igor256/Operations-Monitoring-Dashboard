namespace OperationsMonitoringDashboard.Models;

/// <summary>
/// Represents a reusable label/value option for filter and sort selectors.
/// </summary>
/// <typeparam name="TValue">Type of the option value.</typeparam>
public class SelectionOption<TValue>
{
    /// <summary>
    /// Gets or sets the user-facing option label.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the option value passed into service queries.
    /// </summary>
    public TValue? Value { get; set; }

    /// <summary>
    /// Returns the user-facing label so selection controls always display readable text.
    /// </summary>
    /// <returns>Current <see cref="Label"/> value.</returns>
    public override string ToString()
    {
        return Label;
    }
}

# C# WPF MVVM Coding Rules

## Application Context
You are working on a C# WPF MVVM application.

## General Coding Rules
- Keep code readable and maintainable.
- Avoid overly complex LINQ chains.
- Do not return tuples with multiple mixed types.
- Prefer clear classes and models.
- Keep methods reasonably short.
- Use meaningful names for variables and methods.
- Follow MVVM strictly.
- Avoid code-behind logic except minimal UI initialization.
- Use `ObservableCollection`, `INotifyPropertyChanged`, and `ICommand` properly.
- Separate concerns into Models, ViewModels, Services, and Commands.
- Do not put everything into one file.
- Comment methods and classes.
- Comments should be detailed and clear.
- Hardcoded UI/user-facing text must be stored in `ViewerTextResources`.
- Classes must always be defined in separate files.

## UI Rules
- Keep XAML structured and readable.
- Avoid deeply nested layouts.
- Use reusable styles where appropriate.

## Guiding Principle
Always prefer clarity over cleverness.

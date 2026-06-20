using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace PsDotNet.Kevlar.Demo;

/// <summary>Formats a <see cref="Color"/> as an uppercase #RRGGBB hex string for display.</summary>
public sealed class ColorToHexConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Color c ? $"#{c.R:X2}{c.G:X2}{c.B:X2}" : "—";

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

/// <summary>Returns <c>Visible</c> when the bound value is null, else <c>Collapsed</c> (for empty-state overlays).</summary>
public sealed class NullToVisibleConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is null ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => Binding.DoNothing;
}

using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace Wallsh.Converters;

public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value != null && parameter != null)
            return value.Equals(parameter);

        return false;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is true && parameter != null)
            return parameter;

        if (value is false)
            return BindingOperations.DoNothing;

        if (targetType.IsEnum && parameter == null)
            return Enum.GetValues(targetType).GetValue(0);

        return BindingOperations.DoNothing;
    }
}

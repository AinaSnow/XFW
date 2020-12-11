using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using XIVChatCommon.Message.Server;

namespace XIVChat_Desktop {
    public class DoubleConverter : IValueConverter {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.ToString();
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (double.TryParse(value.ToString(), out var res)) {
                return res;
            }

            return null;
        }
    }

    public class UShortConverter : IValueConverter {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.ToString();
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (ushort.TryParse(value.ToString(), out var res)) {
                return res;
            }

            return null;
        }
    }

    public class UIntConverter : IValueConverter {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return value.ToString();
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            if (uint.TryParse(value.ToString(), out var res)) {
                return res;
            }

            return null;
        }
    }

    public class SenderPlayerConverter : IValueConverter {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (!(value is ServerMessage.SenderPlayer sender)) {
                return null;
            }

            var s = new StringBuilder();

            s.Append(sender.Name);

            var worldName = Util.WorldName(sender.Server);
            if (worldName != null) {
                s.Append(" (");
                s.Append(worldName);
                s.Append(")");
            }

            return s.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class TitleCaseConverter : IValueConverter {
        public object? Convert(object? value, Type targetType, object parameter, CultureInfo culture) {
            var s = value?.ToString();
            if (s == null) {
                return null;
            }

            var lastWasSpace = true;
            var newString = new StringBuilder();

            foreach (var c in s.ToCharArray()) {
                newString.Append(lastWasSpace ? char.ToUpperInvariant(c) : c);

                lastWasSpace = c.IsWhitespace();
            }

            return newString.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }
    }

    public class NotConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return !((bool)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return !((bool)value);
        }
    }

    public abstract class BoolMapper<T> : IValueConverter {
        public T TrueValue { get; set; } = default!;
        public T FalseValue { get; set; } = default!;

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (bool)value ? this.TrueValue : this.FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            var val = (T)value;
            return EqualityComparer<T>.Default.Equals(val, this.TrueValue);
        }
    }

    public class BoolToVisibility : BoolMapper<Visibility> {
    }
}

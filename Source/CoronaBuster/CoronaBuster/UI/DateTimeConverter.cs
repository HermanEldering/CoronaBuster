using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CoronaBuster.UI {
    class DateTimeConverter: IValueConverter, IMarkupExtension {
        public string Format { get; set; } = "{0}";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value is DateTime date) return date.ToString(Format);
            else if (value is TimeSpan span) return span.ToString(Format);

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException("Only one way bindings are supported with this converter");
        }

        public object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace CoronaBuster.UI {
    public class BoolToColorConverter: IValueConverter, IMarkupExtension {
        public Color ColorTrue { get; set; }
        public Color ColorFalse { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return (value is bool b && b) ? ColorTrue : ColorFalse;
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotSupportedException("Only one way bindings are supported with this converter");
        }

        public object ProvideValue(IServiceProvider serviceProvider) {
            return this;
        }
    }
}

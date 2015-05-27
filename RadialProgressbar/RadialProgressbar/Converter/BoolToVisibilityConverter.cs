using System.Windows;


namespace RadialProgressbar.Converter
{
    class BoolToVisibilityConverter : BoolToValueConverter<Visibility>
    {
        public BoolToVisibilityConverter() : base(Visibility.Visible, Visibility.Collapsed) { }
    }
}

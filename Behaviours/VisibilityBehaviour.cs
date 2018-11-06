using System.Windows;
using System.Windows.Controls;

namespace io.borgle.Controls.Behaviours
{
    public class VisibilityBehaviour
    {
        public static readonly DependencyProperty IsVisibledProperty =
        DependencyProperty.RegisterAttached(
            "IsVisibled",
            typeof(bool),
            typeof(VisibilityBehaviour),
            new PropertyMetadata(false, OnIsVisibledPropertyChanged)
        );

        public static bool GetIsVisibled(DependencyObject obj)
        {
            var val = obj.GetValue(IsVisibledProperty);
            return (bool)val;
        }

        public static void SetIsVisibled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsVisibledProperty, value);
        }

        static void OnIsVisibledPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = obj as Control;
            if (ctl == null)
                return;

            var newValue = (bool)args.NewValue;

            if (newValue)
            {
                ctl.Visibility = Visibility.Visible;
            }
            else
            {
                ctl.Visibility = Visibility.Hidden;
            }
        }
    }
}

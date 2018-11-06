using System.Windows;
using System.Windows.Controls;

namespace io.borgle.Controls.Behaviours
{
    public class WindowCloseBehaviour
    {
        #region 使Window支持触发关闭
        public static readonly DependencyProperty IsTriggeredProperty =
        DependencyProperty.RegisterAttached(
            "IsTriggered",
            typeof(bool),
            typeof(WindowCloseBehaviour),
            new PropertyMetadata(false, OnIsTriggeredPropertyChanged)
        );

        public static bool GetIsTriggered(DependencyObject obj)
        {
            var val = obj.GetValue(IsTriggeredProperty);
            return (bool)val;
        }

        public static void SetIsTriggered(DependencyObject obj, bool value)
        {
            obj.SetValue(IsTriggeredProperty, value);
        }

        static void OnIsTriggeredPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var ctl = obj as Control;
            if (ctl == null)
                return;

            var newValue = (bool)args.NewValue;

            if (newValue)
            {
                var window = Window.GetWindow(ctl);
                if (window == null)
                    return;

                window.Close();
            }
        }
        #endregion

        #region 使Button支持关闭窗口操作
        public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached(
            "IsEnabled",
            typeof(bool),
            typeof(WindowCloseBehaviour),
            new PropertyMetadata(false, OnIsEnabledPropertyChanged)
        );

        public static bool GetIsEnabled(DependencyObject obj)
        {
            var val = obj.GetValue(IsEnabledProperty);
            return (bool)val;
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        static void OnIsEnabledPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var button = obj as Button;
            if (button == null)
                return;

            var oldValue = (bool)args.OldValue;
            var newValue = (bool)args.NewValue;

            if (!oldValue && newValue)
            {
                button.Click += OnClick;
            }
            else if (oldValue && !newValue)
            {
                button.PreviewMouseLeftButtonDown -= OnClick;
            }
        }

        static void OnClick(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            var window = Window.GetWindow(button);
            if (window == null)
                return;

            window.Close();
        }
        #endregion
    }
}

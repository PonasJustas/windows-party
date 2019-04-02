using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace TestTesonet.Infrastructure.Helpers
{
    /// <summary>
    /// Helper class to allow caliburn micro PasswordBox binding.
    /// </summary>
    public static class PasswordBoxHelper
    {
        public static readonly DependencyProperty BoundPasswordProperty = DependencyProperty.RegisterAttached("BoundPassword", typeof(string), typeof(PasswordBoxHelper), new FrameworkPropertyMetadata(string.Empty, OnBoundPasswordChanged));

        public static string GetBoundPassword(DependencyObject dependencyObject)
        {
            if (dependencyObject is PasswordBox box)
            {
                // this funny little dance here ensures that we've hooked the
                // PasswordChanged event once, and only once.
                box.PasswordChanged -= PasswordChanged;
                box.PasswordChanged += PasswordChanged;
            }

            return (string)dependencyObject.GetValue(BoundPasswordProperty);
        }

        public static void SetBoundPassword(DependencyObject dependencyObject, string value)
        {
            if (string.Equals(value, GetBoundPassword(dependencyObject)))
            {
                return; // and this is how we prevent infinite recursion
            }

            dependencyObject.SetValue(BoundPasswordProperty, value);
        }

        private static void OnBoundPasswordChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            if (dependencyObject is PasswordBox box)
            {
                box.Password = GetBoundPassword(dependencyObject);
            }
        }

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox password)
            {
                SetBoundPassword(password, password.Password);

                // set cursor past the last character in the password box
                password.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic)?.Invoke(password, new object[] { password.Password.Length, 0 }); 
            }
        }

    }
}

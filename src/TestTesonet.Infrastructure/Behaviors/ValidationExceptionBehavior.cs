using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace TestTesonet.Infrastructure.Behaviors
{
    /// <summary>
    /// A simple behavior that can transfer the number of validation error with exceptions
    /// to a ViewModel which supports the INotifyValidationException interface
    /// </summary>
    public class ValidationExceptionBehavior : Behavior<FrameworkElement>
    {
        private int validationExceptionCount;

        protected override void OnAttached()
        {
            AssociatedObject.AddHandler(Validation.ErrorEvent, new EventHandler<ValidationErrorEventArgs>(this.OnValidationError));
        }

        private void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            // we want to count only the validation error with an exception
            // other error are handled by using the attribute on the properties
            if (e.Error.Exception == null)
            {
                return;
            }

            if (e.Action == ValidationErrorEventAction.Added)
            {
                validationExceptionCount++;
            }
            else
            {
                validationExceptionCount--;
            }

            if (AssociatedObject.DataContext is IValidationExceptionHandler handler)
            {
                // transfer the information back to the viewmodel
                var viewModel = handler;
                viewModel.ValidationExceptionsChanged(validationExceptionCount);
            }
        }
    }
}

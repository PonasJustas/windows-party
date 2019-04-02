using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using TestTesonet.Infrastructure.Behaviors;

namespace TestTesonet.Infrastructure.ViewModels
{
    /// <summary>
    /// A base class for ViewModel classes which supports validation 
    /// using IDataErrorInfo interface. Properties must defines
    /// validation rules by using validation attributes defined in 
    /// System.ComponentModel.DataAnnotations.
    /// </summary>
    public abstract class ValidationPropertyChangedViewModelBase : PropertyChangedBase, 
		IDataErrorInfo, IValidationExceptionHandler
    {
        private readonly Dictionary<string, Func<ValidationPropertyChangedViewModelBase, object>> _propertyGetters;
        private readonly Dictionary<string, ValidationAttribute[]> _validators;

        private int _validationExceptionCount;

        protected ValidationPropertyChangedViewModelBase()
        {
            _validators = GetType().GetProperties().Where(p => GetValidations(p).Length != 0).ToDictionary(p => p.Name, GetValidations);

            _propertyGetters = GetType().GetProperties().Where(p => GetValidations(p).Length != 0).ToDictionary(p => p.Name, GetValueGetter);
        }

        /// <summary>
        /// Gets the error message for the property with the given name.
        /// </summary>
        /// <param name="propertyName">Name of the property</param>
        public string this[string propertyName]
        {
            get
            {
                if (_propertyGetters.ContainsKey(propertyName))
                {
                    var propertyValue = _propertyGetters[propertyName](this);
                    var errorMessages = _validators[propertyName].Where(v => !v.IsValid(propertyValue)).Select(v => v.ErrorMessage).ToArray();

                    return string.Join(Environment.NewLine, errorMessages);
                }

                return string.Empty;
            }
        }

        /// <summary>
        /// Gets an error message indicating what is wrong with this object.
        /// </summary>
        public string Error
        {
            get
            {
                var errors = from validator in _validators
                    from attribute in validator.Value
                    where !attribute.IsValid(_propertyGetters[validator.Key](this))
                    select attribute.ErrorMessage;

                return string.Join(Environment.NewLine, errors.ToArray());
            }
        }

        /// <summary>
        /// Gets the number of properties which have a 
        /// validation attribute and are currently valid
        /// </summary>
        public int ValidPropertiesCount
        {
            get
            {
                var query = from validator in _validators
                    where validator.Value.All(attribute => attribute.IsValid(_propertyGetters[validator.Key](this)))
                    select validator;

                var count = query.Count() - _validationExceptionCount;
                return count;
            }
        }
        
        /// <summary>
        /// Gets the number of properties which have a validation attribute
        /// </summary>
        public int TotalPropertiesWithValidationCount => _validators.Count;

        public void ValidationExceptionsChanged(int count)
        {
            _validationExceptionCount = count;
            NotifyOfPropertyChange(nameof(ValidPropertiesCount));
        }
        
        private ValidationAttribute[] GetValidations(PropertyInfo property)
        {
            return (ValidationAttribute[]) property.GetCustomAttributes(typeof(ValidationAttribute), true);
        }

        private Func<ValidationPropertyChangedViewModelBase, object> GetValueGetter(PropertyInfo property)
        {
            return viewmodel => property.GetValue(viewmodel, null);
        }
    }
}

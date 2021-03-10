using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ModelExtensions
{
    /// <summary>
    /// minimalistic model class
    /// </summary>
    [Serializable]
    public class BindableModel :
        INotifyPropertyChanged,
        IDataErrorInfo,
        INotifyDataErrorInfo
    {
        #region interface INotifyPropertyChanged

        /// <summary>
        /// indicates wether or not properties changes notification are enabled or not
        /// </summary>
        [NonSerialized]
        public bool NotifyPropertyChangedEnabled = true;

        /// <summary>
        /// indicates wetger or not errors changes notification are enabled or not
        /// </summary>
        [NonSerialized]
        public bool NotifyErrorsChangedEnabled = true;

        /// <summary>
        /// properties names in this set won't update IsModified when changed
        /// </summary>
        public List<string> IgnoreIsModifiedUpdate =
            new List<string> { nameof(HasNotifiedPropertyChanged) , nameof(IsValid) };

        /// <summary>
        /// true if one of the model property has been modified
        /// </summary>
        bool _IsModified = false;
        public bool IsModified
        {
            get
            {
                return _IsModified;
            }
            set
            {
                _IsModified = value;
                NotifyPropertyChanged();
            }
        }

        bool _HasNotifiedPropertyChanged = false;
        public bool HasNotifiedPropertyChanged
        {
            get
            {
                return _HasNotifiedPropertyChanged;
            }
            set
            {
                _HasNotifiedPropertyChanged = value;
                NotifyPropertyChanged();
            }
        }

        bool _IsValid = true;
        public bool IsValid
        {
            get
            {
                return !HasErrors;
            }
            set
            {
                
            }
        }

        /// <summary>
        /// property changed event handler
        /// </summary>
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// notify a property has changed
        /// </summary>
        /// <param name="propName"></param>
        public void NotifyPropertyChanged(
            [CallerMemberName] string propName = "")
        {
            if (propName == nameof(IsModified))
                return; // anti loop

            if (!IgnoreIsModifiedUpdate.Contains(propName))
                IsModified = true;

            if (!NotifyPropertyChangedEnabled)
            {
                return;
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
            if (!HasNotifiedPropertyChanged)
                HasNotifiedPropertyChanged = true;
        }

        #endregion

        #region interface IDataErrorInfo

        /// <summary>
        /// properties errors
        /// </summary>
        protected Dictionary<string, List<string>> _Errors = new Dictionary<string, List<string>>();

        /// <summary>
        /// store (remember,never forget) ruleError type for each error message/property
        /// </summary>
        protected Dictionary<string, Dictionary<string, Type>> _ErrorsTypes = new Dictionary<string, Dictionary<string, Type>>();

        /// <summary>
        /// returns error text for given column if exists any one
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <returns>error text</returns>
        public string this[string propertyName]
        {
            get
            {
                if (_Errors.TryGetValue(propertyName, out var errList))
                    return string.Join(",",errList);
                return null;
            }
        }

        /// <summary>
        /// last error
        /// </summary>
        string _Error = null;

        /// <summary>
        /// text of last notified error by InfoDataErrorsChanged if any one, else null
        /// </summary>
        public string Error
        {
            get
            {
                return _Error;
            }
        }

        #endregion

        #region interface INotifyDataErrorInfo
       
        /// <summary>
        /// data errors changed event handler
        /// </summary>
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public string ErrorResume
        {
            get
            {
                return HasErrors ? _Errors.First().Value[0] : "";
            }
        }
        public bool HasErrors => _Errors.Count > 0;

        #region error management

#if OBSOLETE
        public void SetErrors(List<string> propertyErrors, [CallerMemberName] string propertyName = "")
        {
            // Clear any errors that already exist for this property.
            errors.Remove(propertyName);
            // Add the list collection for the specified property.
            errors.Add(propertyName, propertyErrors);
            // Raise the error-notification event.
            if (NotifyErrorsChangedEnabled)
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            NotifyPropertyChanged(nameof(IsValid));
        }

        public void SetError(string propertyError, [CallerMemberName] string propertyName = "")
        {
            // Clear any errors that already exist for this property.
            errors.Remove(propertyName);
            // Add the list collection for the specified property.
            errors.Add(propertyName, new List<string> { propertyError });
            // Raise the error-notification event.
            if (NotifyErrorsChangedEnabled)
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            NotifyPropertyChanged(nameof(IsValid));
        }
#endif

        public void AddError(
            string propertyError, 
            Type ruleErrorType,
            [CallerMemberName] string propertyName = "", 
            bool addUnique=false)
        {
            if (propertyError == null)
                return;
            if (!_ErrorsTypes.ContainsKey(propertyName))
                _ErrorsTypes.Add(propertyName, new Dictionary<string, Type> { { propertyError, ruleErrorType } });
            else
                if (!_ErrorsTypes[propertyName].ContainsKey(propertyError))
                    _ErrorsTypes[propertyName].Add(propertyError, ruleErrorType);
            if (!_Errors.ContainsKey(propertyName))
                _Errors.Add(propertyName, new List<string> { propertyError });
            else
            {
                if (!addUnique || !_Errors[propertyName].Contains(propertyError))
                    _Errors[propertyName].Add(propertyError);
            }
            if (NotifyErrorsChangedEnabled)
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            NotifyPropertyChanged(nameof(IsValid));
        }

        public void RemoveError(string propertyError, [CallerMemberName] string propertyName = "")
        {
            if (_Errors.TryGetValue(propertyName,out var errorList))
            {
                errorList.Remove(propertyError);
                if (errorList.Count == 0)
                    _Errors.Remove(propertyName);
                if (NotifyErrorsChangedEnabled)
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
            NotifyPropertyChanged(nameof(IsValid));
        }

        public void ClearErrors([CallerMemberName] string propertyName = "")
        {
            // Remove the error list for this property.
            _Errors.Remove(propertyName);
            // Raise the error-notification event.
            if (NotifyErrorsChangedEnabled)
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            NotifyPropertyChanged(nameof(IsValid));
        }

        public void ClearPropertyValidationErrors([CallerMemberName] string propertyName = "")
        {
            if (!_Errors.ContainsKey(propertyName))
                return;
            var lst = _Errors[propertyName].ToList();
            foreach (var e in lst)
            {
                if (_ErrorsTypes.TryGetValue(propertyName, out var propErrTypes)
                    && propErrTypes.TryGetValue(e, out var errType)
                    && errType.FullName == "System.Windows.Controls.NotifyDataErrorValidationRule")
                    _Errors[propertyName]
                        .Remove(e);
                if (NotifyErrorsChangedEnabled)
                    ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
                NotifyPropertyChanged(nameof(IsValid));
            }
        }

        public bool HasPropertyValivationError([CallerMemberName] string propertyName = "")
        {
            if (!_Errors.ContainsKey(propertyName))
                return false;
            var lst = _Errors[propertyName].ToList();
            foreach (var e in lst)
                if (_ErrorsTypes.TryGetValue(propertyName, out var propErrTypes)
                    && propErrTypes.TryGetValue(e, out var errType)
                    && errType.FullName == "System.Windows.Controls.NotifyDataErrorValidationRule")
                    return true;
            return false;
        }

        public IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                // Provide all the error collections.
                return (_Errors.Values);
            }
            else
            {
                // Provide the error collection for the requested property if it has errors
                if (_Errors.ContainsKey(propertyName))
                {
                    return (_Errors[propertyName]);
                }
                else
                {
                    return null;
                }
            }
        }

        public void ValidateModel()
        {
            foreach ( var property in GetType().GetProperties() )
            {
                var validationAttributes = property
                    .GetCustomAttributes(true)
                    .OfType<ValidationAttribute>();
                if (validationAttributes.Count() > 0)
                    ValidateProperty(property.GetValue(this), property.Name);
            }
            NotifyPropertyChanged(null);
        }

#endregion

#endregion

#region support of DataModelAnnotations

        /// <summary>
        /// to be called from property setter.
        /// such validated controls should not have binding validation property NotifyOnValidationError=True (False needed, is default value)
        /// </summary>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        /// <returns>true if property value is validaed. should enabled change of property value in property setter</returns>
        public bool ValidateProperty(object value,[CallerMemberName] string propertyName="")
        {
            _Errors.Remove(propertyName);
            ICollection<ValidationResult> validationResults = new List<ValidationResult>();
            ValidationContext validationContext =
                new ValidationContext(this, null, null) { MemberName = propertyName };
            bool r;
            if (!(r=Validator.TryValidateProperty(value, validationContext, validationResults)))
            {
                _Errors.Add(propertyName, new List<string>());
                foreach (ValidationResult validationResult in validationResults)
                {
                    _Errors[propertyName].Add(validationResult.ErrorMessage);
                }
            }
            /*if (NotifyErrorsChangedEnabled)
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));*/
            NotifyPropertyChanged(nameof(IsValid));
            return r;
        }

#endregion
    }
}

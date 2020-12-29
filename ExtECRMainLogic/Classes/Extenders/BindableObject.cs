using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace ExtECRMainLogic.Classes.Extenders
{
    /// <summary>
    /// Implements the INotifyPropertyChanged interface and
    /// exposes a RaisePropertyChanged method for derived
    /// classes to raise the PropertyChange event.  The event
    /// arguments created by this class are cached to prevent
    /// managed heap fragmentation.
    /// </summary>
    [Serializable]
    public abstract class BindableObject : INotifyPropertyChanged, IDataErrorInfo
    {
        #region Data
        private static readonly Dictionary<string, PropertyChangedEventArgs> eventArgCache;
        private readonly Dictionary<String, List<String>> errors = new Dictionary<string, List<string>>();
        private const string ERROR_MSG = "{0} is not a public property of {1}";
        #endregion

        #region Constructors
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        static BindableObject()
        {
            eventArgCache = new Dictionary<string, PropertyChangedEventArgs>();
        }
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        protected BindableObject()
        {
        }
        #endregion

        #region Public Members
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Raised when a public property of this object is set.
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Returns an instance of PropertyChangedEventArgs for
        /// the specified property name.
        /// </summary>
        /// <param name="propertyName">
        /// The name of the property to create event args for.
        /// </param>
        /// <returns></returns>
        public static PropertyChangedEventArgs GetPropertyChangedEventArgs(string propertyName)
        {
            if (String.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("propertyName cannot be null or empty.");
            }
            PropertyChangedEventArgs args;
            // Get the event args from the cache, creating them and adding to the cache if necessary.
            lock (typeof(BindableObject))
            {
                bool isCached = eventArgCache.ContainsKey(propertyName);
                if (!isCached)
                {
                    eventArgCache.Add(propertyName, new PropertyChangedEventArgs(propertyName));
                }
                args = eventArgCache[propertyName];
            }
            return args;
        }
        #endregion

        #region Protected Members
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Derived classes can override this method to
        /// execute logic after a property is set. The
        /// base implementation does nothing.
        /// </summary>
        /// <param name="propertyName">
        /// The property which was changed.
        /// </param>
        protected virtual void AfterPropertyChanged(string propertyName)
        {
        }
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Attempts to raise the PropertyChanged event, and
        /// invokes the virtual AfterPropertyChanged method,
        /// regardless of whether the event was raised or not.
        /// </summary>
        /// <param name="propertyName">
        /// The property which was changed.
        /// </param>
        protected void RaisePropertyChanged(string propertyName)
        {
            this.VerifyProperty(propertyName);
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null)
            {
                // Get the cached event args.
                PropertyChangedEventArgs args = GetPropertyChangedEventArgs(propertyName);
                // Raise the PropertyChanged event.
                handler(this, args);
            }
            this.AfterPropertyChanged(propertyName);
        }
        #endregion

        #region Private Helpers
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        [Conditional("DEBUG")]
        private void VerifyProperty(string propertyName)
        {
            Type type = this.GetType();
            // Look for a public property with the specified name.
            PropertyInfo propInfo = type.GetProperty(propertyName);
            if (propInfo == null)
            {
                // The property could not be found, so alert the developer of the problem.
                string msg = string.Format(ERROR_MSG, propertyName, type.FullName);
                Debug.Fail(msg);
            }
        }
        #endregion

        #region ErrorImplementation
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        string IDataErrorInfo.Error
        {
            get { throw new NotImplementedException(); }
        }
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        string IDataErrorInfo.this[string columnName]
        {
            get { return GetErrors(columnName); }
        }
        #region Protected Members
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Derived classes can override this method to
        /// execute logic after a property is set. The
        /// base implementation does nothing.
        /// </summary>
        /// <param name="propertyName">
        /// The property which was changed.
        /// </param>
        protected virtual void AfterDataErrorChanged(string propertyName)
        {
        }
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Attempts to raise the PropertyChanged event, and
        /// invokes the virtual AfterPropertyChanged method,
        /// regardless of whether the event was raised or not.
        /// </summary>
        /// <param name="propertyName">
        /// The property which was changed.
        /// </param>
        protected void RaiseDataErrorChanged(string propertyName)
        {
            this.VerifyProperty(propertyName);
            var cur_property = this.GetType().GetProperty(propertyName);
            var validationMap = cur_property.GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>();
            foreach (var v in validationMap)
            {
                try
                {
                    v.Validate(cur_property.GetValue(this, null), cur_property.Name);
                    if (v.IsValid(cur_property.GetValue(this, null)))
                    {
                        RemoveError(propertyName, v.ErrorMessage);
                    }
                    else
                    {
                        AddError(propertyName, v.ErrorMessage, false);
                    }
                }
                catch (Exception exception)
                {
                    AddError(propertyName, exception.Message, false);
                }
            }
            this.AfterDataErrorChanged(propertyName);
        }
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public string GetErrors(string propertyName)
        {
            string strerr = string.Empty;
            if (errors.ContainsKey(propertyName))
            {
                int i = 0;
                foreach (var e in errors[propertyName])
                {
                    if (i > 0)
                    {
                        strerr += "," + e;
                    }
                    else
                    {
                        strerr += e;
                    }
                }
            }
            return strerr;
        }
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Validates current instance properties using Data Annotations.
        /// </summary>
        /// <param name="propertyName">This instance property to validate.</param>
        /// <returns>Relevant error string on validation failure or <see cref="System.String.Empty"/> on validation success.</returns>
        protected virtual string OnValidateItem(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException("Invalid property name", propertyName);
            }
            string err = string.Empty;
            var cur_property = this.GetType().GetProperty(propertyName);
            var validationMap = cur_property.GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>();
            foreach (var v in validationMap)
            {
                try
                {
                    v.Validate(cur_property.GetValue(this, null), cur_property.Name);
                    if (v.IsValid(cur_property.GetValue(this, null)))
                    {
                        RemoveError(propertyName, v.ErrorMessage);
                    }
                    else
                    {
                        AddError(propertyName, v.ErrorMessage, false);
                    }
                }
                catch (Exception exception)
                {
                    AddError(propertyName, exception.Message, false);
                }
            }
            if (errors.ContainsKey(propertyName))
            {
                int i = 0;
                foreach (var e in errors[propertyName])
                {
                    if (i == 0)
                    {
                        err += e + ", ";
                    }
                }
            }
            return err;
        }
        #endregion
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public void InitializeErrors()
        {
            foreach (var pname in GetType().GetProperties())
            {
                var validationMap = pname.GetCustomAttributes(typeof(ValidationAttribute), true).Cast<ValidationAttribute>();
                foreach (var v in validationMap)
                {
                    try
                    {
                        v.Validate(pname.GetValue(this, null), pname.Name);
                        if (v.IsValid(pname.GetValue(this, null)))
                        {
                            RemoveError(pname.Name, v.ErrorMessage);
                        }
                        else
                        {
                            AddError(pname.Name, v.ErrorMessage, false);
                        }
                    }
                    catch (Exception exception)
                    {
                        AddError(pname.Name, exception.Message, false);
                    }
                }
            }
        }
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 
        /// </summary>
        public bool HasErrors
        {
            get { return errors.Count > 0; }
        }
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Adds the specified error to the errors collection if it is not
        /// already present, inserting it in the first position if isWarning is
        /// false. Raises the ErrorsChanged event if the collection changes.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="error"></param>
        /// <param name="isWarning"></param>
        public void AddError(string propertyName, string error, bool isWarning)
        {
            if (!errors.ContainsKey(propertyName))
            {
                errors[propertyName] = new List<string>();
            }
            if (!errors[propertyName].Contains(error))
            {
                if (isWarning)
                {
                    errors[propertyName].Add(error);
                }
                else
                {
                    errors[propertyName].Insert(0, error);
                }
            }
        }
        /////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// Removes the specified error from the errors collection if it is
        /// present. Raises the ErrorsChanged event if the collection changes.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="error"></param>
        public void RemoveError(string propertyName, string error)
        {
            if (errors.ContainsKey(propertyName) && errors[propertyName].Contains(error))
            {
                errors[propertyName].Remove(error);
                if (errors[propertyName].Count == 0)
                {
                    errors.Remove(propertyName);
                }
            }
        }
        #endregion
    }
}
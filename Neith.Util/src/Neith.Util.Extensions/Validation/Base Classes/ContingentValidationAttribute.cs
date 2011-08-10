﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Neith.Util.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ContingentValidationAttribute : ModelAwareValidationAttribute
    {
        public string DependentProperty { get; private set; }

        public ContingentValidationAttribute(string dependentProperty)
        {
            DependentProperty = dependentProperty;
        }

        public override string FormatErrorMessage(string name)
        {
            if (string.IsNullOrEmpty(ErrorMessageResourceName) && string.IsNullOrEmpty(ErrorMessage))
                ErrorMessage = DefaultErrorMessage;

            return string.Format(ErrorMessageString, name, DependentProperty);
        }

        public override string DefaultErrorMessage
        {
            get { return "{0} is invalide due to {1}."; }
        }

        private object GetDependentPropertyValue(object container)
        {
            return container.GetType()
                .GetProperty(DependentProperty)
                .GetValue(container, null);
        }

        protected override IEnumerable<KeyValuePair<string, object>> GetClientValidationParameters()
        {
            return base.GetClientValidationParameters()
                .Union(new[] { new KeyValuePair<string, object>("DependentProperty", DependentProperty) });
        }

        public override bool IsValid(object value, object container)
        {
            return IsValid(value, GetDependentPropertyValue(container), container);
        }

        public abstract bool IsValid(object value, object dependentValue, object container);
    }
}
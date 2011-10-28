using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace System.Windows
{
    public static class ResourceExtensions
    {
        /// <summary>
        /// プロパティにダイナミックリソースを割り当てます。
        /// </summary>
        /// <param name="target"></param>
        /// <param name="dp"></param>
        /// <param name="key"></param>
        public static void SetDynamicResource(this DependencyObject target, DependencyProperty dp, object key)
        {
            var src = new ValueTarget(target, dp);
            var dre = new DynamicResourceExtension(key);
            target.SetValue(dp, dre.ProvideValue(src));
        }
        internal class ValueTarget : IServiceProvider, IProvideValueTarget
        {
            internal ValueTarget(DependencyObject target, DependencyProperty dp)
            {
                TargetObject = target;
                TargetProperty = dp;
            }

            public object TargetObject { get; private set; }
            public object TargetProperty { get; private set; }
            public object GetService(Type serviceType)
            {
                return (serviceType == typeof(IProvideValueTarget)) ? this : null;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Util
{
    /// <summary>
    /// Enum���샆�[�e�B���e�B�B
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class EnumUtil<T>
    {
        /// <summary>
        /// �������Enum�ɕϊ����܂��B
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T Parse(string value)
        {
            return (T)Enum.Parse(typeof(T), value);
        }
    }
}

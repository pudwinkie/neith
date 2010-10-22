﻿/*
Copyright (c) 2010 <a href="http://www.gutgames.com">James Craig</a>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.*/

#region Usings
using System;
using System.Collections.Generic;
using System.Text;
using Utilities.FileFormats.FixedLength.Interfaces;
#endregion

namespace Utilities.FileFormats.FixedLength.BaseClasses
{
    /// <summary>
    /// Record base class
    /// </summary>
    public class RecordBase<T> : IRecord<T>
    {
        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public RecordBase()
        {
            Fields = new List<IField<T>>();
        }

        #endregion

        #region IRecord Members

        public virtual void Parse(string Value)
        {
            throw new NotImplementedException();
        }

        public virtual void Parse(string Value,int Length)
        {
            throw new NotImplementedException();
        }

        public int Length { get; set; }

        public List<IField<T>> Fields { get; set; }

        #endregion

        #region Public Overridden Functions

        public override string ToString()
        {
            StringBuilder Builder = new StringBuilder();
            foreach (IField<T> Field in Fields)
            {
                Builder.Append(Field.ToString());
            }
            return Builder.ToString();
        }

        #endregion
    }
}
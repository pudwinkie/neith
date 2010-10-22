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
using System.Linq;
using System.Text;
#endregion

namespace Utilities.Environment.DataTypes
{
    /// <summary>
    /// Represents a computer
    /// </summary>
    public class Computer
    {
        #region Constructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Name">Computer Name</param>
        /// <param name="UserName">User name</param>
        /// <param name="Password">Password</param>
        public Computer(string Name,string UserName="",string Password="")
        {
            this.Name = Name;
            this.UserName = UserName;
            this.Password = Password;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Computer Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// User Name
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// BIOS info
        /// </summary>
        public BIOS BIOS
        {
            get
            {
                try
                {
                    if (_BIOS == null)
                    {
                        _BIOS = new BIOS(Name, UserName, Password);
                    }
                    return _BIOS;
                }
                catch { throw; }
            }
        }

        private BIOS _BIOS = null;

        /// <summary>
        /// Application info
        /// </summary>
        public Applications Applications
        {
            get
            {
                try
                {
                    if (_Applications == null)
                    {
                        _Applications = new Applications(Name, UserName, Password);
                    }
                    return _Applications;
                }
                catch { throw; }
            }
        }

        private Applications _Applications = null;

        public Network Network
        {
            get
            {
                try
                {
                    if (_Network == null)
                    {
                        _Network = new Network(Name, UserName, Password);
                    }
                    return _Network;
                }
                catch { throw; }
            }
        }

        private Network _Network = null;


        #endregion
    }
}

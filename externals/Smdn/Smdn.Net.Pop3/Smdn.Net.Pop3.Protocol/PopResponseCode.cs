// 
// Author:
//       smdn <smdn@mail.invisiblefulmoon.net>
// 
// Copyright (c) 2010 smdn
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;

namespace Smdn.Net.Pop3.Protocol {
  public sealed class PopResponseCode : PopStringEnum {
    public static readonly PopStringEnumList<PopResponseCode> AllCodes;

    /*
     * POP3 Extension Mechanism
     * http://www.iana.org/assignments/pop3-extension-mechanism
     */

#region "RFC 2449 - POP3 Extension Mechanism"
    /*
     * RFC 2449 - POP3 Extension Mechanism
     * http://tools.ietf.org/html/rfc2449#section-8
     * 
     * 8. Extended POP3 Response Codes
     */

    /*
     * 8.1.1. The LOGIN-DELAY response code
     * 
     *    This occurs on an -ERR response to an AUTH, USER (see note), PASS or
     *    APOP command and indicates that the user has logged in recently and
     *    will not be allowed to login again until the login delay period has
     *    expired.
     * 
     */
    public static readonly PopResponseCode LoginDelay = new PopResponseCode("LOGIN-DELAY");

    /*
     * 8.1.2. The IN-USE response code
     * 
     *    This occurs on an -ERR response to an AUTH, APOP, or PASS command.
     *    It indicates the authentication was successful, but the user's
     *    maildrop is currently in use (probably by another POP3 client).
     */
    public static readonly PopResponseCode InUse = new PopResponseCode("IN-USE");
#endregion

#region "RFC 3206 - The SYS and AUTH POP Response Codes"
    /*
     * RFC 3206 - The SYS and AUTH POP Response Codes
     * http://tools.ietf.org/html/rfc3206
     */

    /*
     * 4. The SYS Response Code
     * 
     *    The SYS response code announces that a failure is due to a system
     *    error, as opposed to the user's credentials or an external condition.
     *    It is hierarchical, with two possible second-level codes: TEMP and
     *    PERM.  (Case is not significant at any level of the hierarchy.)
     * 
     *    SYS/TEMP indicates a problem which is likely to be temporary in
     *    nature, and therefore there is no need to alarm the user, unless the
     *    failure persists.  Examples might include a central resource which is
     *    currently locked or otherwise temporarily unavailable, insufficient
     *    free disk or memory, etc.
     * 
     *    SYS/PERM is used for problems which are unlikely to be resolved
     *    without intervention.  It is appropriate to alert the user and
     *    suggest that the organization's support or assistance personnel be
     *    contacted.  Examples include corrupted mailboxes, system
     *    configuration errors, etc.
     */
    public static readonly PopResponseCode SysTemp = new PopResponseCode("SYS/TEMP"); // XXX: resp-level
    public static readonly PopResponseCode SysPerm = new PopResponseCode("SYS/PERM"); // XXX: resp-level

    /* 
     * 5. The AUTH Response Code
     * 
     *    The AUTH response code informs the client that there is a problem
     *    with the user's credentials.  This might be an incorrect password, an
     *    unknown user name, an expired account, an attempt to authenticate in
     *    violation of policy (such as from an invalid location or during an
     *    unauthorized time), or some other problem.
     */
    public static readonly PopResponseCode Auth = new PopResponseCode("AUTH");
#endregion

    static PopResponseCode()
    {
      AllCodes = CreateDefinedConstantsList<PopResponseCode>();
    }

    public static PopResponseCode GetKnownOrCreate(string code)
    {
      if (AllCodes.Has(code))
        return AllCodes[code];

      //Smdn.Net.Pop3.Client.Trace.Verbose("unknown response code: {0}", code);

      return new PopResponseCode(code);
    }

    internal PopResponseCode(string code)
      : base(code)
    {
      if (code == null)
        throw new ArgumentNullException("code");
    }
  }
}

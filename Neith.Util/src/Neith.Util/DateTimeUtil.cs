using System;
using System.Collections.Generic;
using System.Text;

namespace Neith.Util
{
    /// <summary>
    /// �������[�e�B���e�B�B
    /// </summary>
    public static class DateTimeUtil
    {
        /// <summary>
        /// DateTime.UtcNow��Ԃ��܂��B�A���O��Ɠ������ߋ��̎����������ꍇ�ɁA
        /// �O��Ԃ���������1tick���Z��������Ԃ��A���j�[�N�ł��邱�Ƃ�ۏႵ�܂��B
        /// </summary>
        /// <returns></returns>
        public static DateTime GetUniqueTimeStamp()
        {
            return GetUniqueTimeStampImpl.Create();
        }
        #region ����
        /// <summary>
        /// �K���O����傫��������Ԃ����Ƃ�ۏႷ��^�C���X�^���v���s�@�̎����B
        /// </summary>
        private static class GetUniqueTimeStampImpl
        {
            private static DateTime last = DateTime.MinValue;

            public static DateTime Create()
            {
                DateTime rc = DateTime.UtcNow;
                lock (typeof(GetUniqueTimeStampImpl)) {
                    if (last >= rc) {
                        rc = last.AddTicks(1);
                    }
                    last = rc;
                }
                return rc;
            }
        }
        #endregion

    }

}

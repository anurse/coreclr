// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Diagnostics.Contracts;
using System.Text;

namespace System.Globalization
{
    public partial class TextInfo
    {
        private readonly bool m_needsTurkishCasing;

        //////////////////////////////////////////////////////////////////////////
        ////
        ////  TextInfo Constructors
        ////
        ////  Implements CultureInfo.TextInfo.
        ////
        //////////////////////////////////////////////////////////////////////////
        internal unsafe TextInfo(CultureData cultureData)
        {
            m_cultureData = cultureData;
            m_cultureName = m_cultureData.CultureName;
            m_textInfoName = m_cultureData.STEXTINFO;
            m_needsTurkishCasing = NeedsTurkishCasing(m_textInfoName);
        }

        [System.Security.SecuritySafeCritical]
        private unsafe string ChangeCase(string s, bool toUpper)
        {
            Contract.Assert(s != null);

            if (s.Length == 0)
            {
                return string.Empty;
            }

            string result = string.FastAllocateString(s.Length);

            fixed (char* pResult = result)
            {
                if (IsAsciiCasingSameAsInvariant && s.IsAscii())
                {
                    fixed (char* pSource = s)
                    {
                        int length = s.Length;
                        char* a = pSource, b = pResult;
                        if (toUpper)
                        {
                            while (length-- != 0)
                            {
                                *b++ = ToUpperAsciiInvariant(*a++);
                            }
                        }
                        else
                        {
                            while (length-- != 0)
                            {
                                *b++ = ToLowerAsciiInvariant(*a++);
                            }
                        }
                    }
                }
                else
                {
                    Interop.GlobalizationInterop.ChangeCase(s, s.Length, pResult, result.Length, toUpper, m_needsTurkishCasing);
                }
            }

            return result;
        }

        [System.Security.SecuritySafeCritical]
        private unsafe char ChangeCase(char c, bool toUpper)
        {
            char dst = default(char);

            Interop.GlobalizationInterop.ChangeCase(&c, 1, &dst, 1, toUpper, m_needsTurkishCasing);

            return dst;
        }

        // -----------------------------
        // ---- PAL layer ends here ----
        // -----------------------------

        private bool NeedsTurkishCasing(string localeName)
        {
            Contract.Assert(localeName != null);
            return CultureInfo.GetCultureInfo(localeName).CompareInfo.Compare("i", "I", CompareOptions.IgnoreCase) != 0;
        }
    }
}

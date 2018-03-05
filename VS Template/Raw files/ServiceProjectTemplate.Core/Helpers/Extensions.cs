using System;

namespace $safeprojectname$
{
    public static class Extensions
    {
        /// <summary>
        /// Output a timestamp string
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToTimeStamp(this DateTime value)
        {
            return value.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public static string ToTitleCase(this String str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                System.Globalization.TextInfo txtInfo = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
                return txtInfo.ToTitleCase(str.ToLower());
            }
            return str;
        }

        public static string ToUpperCase(this String str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                return str.ToUpper();
            }
            return str;
        }


        public static string ToLowerCase(this String str)
        {
            if (!String.IsNullOrEmpty(str))
            {
                return str.ToLower();
            }
            return str;
        }


        public static string Left(this String str, int length)
        {
            if (length > str.Length)
            {
                length = str.Length;
            }
            return str.Substring(0, length);
        }

        public static string Right(this String str, int length)
        {
            if (length <= str.Length)
            {
                return str.Substring(str.Length - length, length);
            }
            return str;
        }


        public static string RemoveFirst(this String source, string remove, int inFirstXChars = 0)
        {
            if (!String.IsNullOrEmpty(source))
            {
                int index = source.IndexOf(remove);
                if ((index >= 0) && (index < inFirstXChars || inFirstXChars == 0))
                {
                    return source.Remove(index, remove.Length);
                }
            }
            return source;
        }
    }
}

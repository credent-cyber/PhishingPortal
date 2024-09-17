using System.Text.RegularExpressions;

namespace PhishingPortal.Common
{

    public static class StringExtensions
    {
        public static byte[] ToByteArray(this string value)
        {
            return System.Text.Encoding.UTF8.GetBytes(value);  
        }

        public static string ToBase64String(this string value)
        {
            return Convert.ToBase64String(value.Replace("\r", "").Replace("\n", "").ToByteArray());
        }

        public static string FromBase64String(this string value)
        {
            return System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        /// <summary>
        /// Computes MD5 hash for the input string
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ComputeMd5Hash(this string input)
        {
            using (System.Security.Cryptography.MD5 md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                return Convert.ToHexString(hashBytes);
            }
        }

        /// <summary>
        /// Compare Hash
        /// </summary>
        /// <param name="value"></param>
        /// <param name="hash"></param>
        /// <returns></returns>
        public static bool CompareHash(this string value, string hash)
        {
            var tempHash = ComputeMd5Hash(value);
            return tempHash == hash;
        }

        public static bool IsValidDomain(this string str)
        {
            string strRegex = @"^((?!-)[A-Za-z0-9-]{1,63}(?<!-)\.)+[A-Za-z]{2,6}$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(str))
                return (true);
            else
                return (false);
        }

    }
}
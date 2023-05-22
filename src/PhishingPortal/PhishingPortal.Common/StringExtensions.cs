namespace PhishingPortal.Common
{

    public static class StringExtensions
    {

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

    }
}
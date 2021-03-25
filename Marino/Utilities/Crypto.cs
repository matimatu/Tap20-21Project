using System;
using System.Security.Cryptography;

namespace Marino.Utilities
{
    class Crypto
    {
        /// <summary>
        /// Generate a string for sessionId 32 chars long
        /// </summary>
        /// <returns></returns>
        public static string GenerateSessionId()
        {
            const int size = 16;
            var data = new byte[size];
            var crypto = new RNGCryptoServiceProvider();
            crypto.GetBytes(data);
            var id = BitConverter.ToString(data).Replace("-", string.Empty);

            return id;
        }
        /// <summary>
        /// Hash the string adding some salt too
        /// </summary>
        /// <param name="password"></param>
        /// <param name="hashSize"></param>
        /// <param name="saltSize"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static string HashPw(string password, int hashSize, int saltSize, int iterations)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));
            byte[] salt;
            byte[] bytes;
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, saltSize, iterations))
            {
                salt = rfc2898DeriveBytes.Salt;
                bytes = rfc2898DeriveBytes.GetBytes(hashSize);
            }
            var inArray = new byte[saltSize + hashSize];
            Buffer.BlockCopy((Array)salt, 0, (Array)inArray, 0, saltSize);
            Buffer.BlockCopy((Array)bytes, 0, (Array)inArray, saltSize, hashSize);
            return Convert.ToBase64String(inArray);
        }
        /// <summary>
        /// Verifies if the password is the correct hash of the hashedpw
        /// </summary>
        /// <param name="hashedPasswordAndSalt"></param>
        /// <param name="password"></param>
        /// <param name="hashSize"></param>
        /// <param name="saltSize"></param>
        /// <param name="iterations"></param>
        /// <returns></returns>
        public static bool VerifyHashedPw(
            string hashedPasswordAndSalt,
            string password,
            int hashSize,
            int saltSize,
            int iterations)
        {
            if (hashedPasswordAndSalt == null)
                return false;
            var numArray1 = Convert.FromBase64String(hashedPasswordAndSalt);
            if (numArray1.Length != saltSize + hashSize)
                return false;
            var salt = new byte[saltSize];
            Buffer.BlockCopy((Array)numArray1, 0, (Array)salt, 0, saltSize);
            var numArray2 = new byte[hashSize];
            Buffer.BlockCopy((Array)numArray1, saltSize, (Array)numArray2, 0, hashSize);
            byte[] bytes;
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, iterations))
                bytes = rfc2898DeriveBytes.GetBytes(hashSize);
            var flag = true;
            for (var i = 0; i < hashSize; ++i)
                flag &= (int)bytes[i] == (int)numArray2[i];
            return flag;
        }
    }
}

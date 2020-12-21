using System.Security.Cryptography;
using System.Text;

namespace Cmb.SkyApm.Common
{
    public static class HashHelpers
    {
        public static string GetHashString(string inputString)
        {
            var hashBytes = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(inputString));
            var sb = new StringBuilder();

            foreach (byte b in hashBytes)
                sb.Append(b.ToString("X2"));

            return sb.ToString().Substring(8, 16).ToLower();
        }
    }
}

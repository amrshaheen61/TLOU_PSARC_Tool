using System.Security.Cryptography;

namespace TLOU_PSARC_Tool.Core
{
    internal class Md5
    {
        public static byte[] Calc(string input)
        {
#pragma warning disable SCS0006
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                return hashBytes;
            }
#pragma warning restore SCS0006
        }
    }
}

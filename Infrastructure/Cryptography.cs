using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Trader.Infrastructure
{
    public static class Cryptography
    {

        public static string HashHMAC512Hex(string data, string key)
        {
            string result = "";
            try
            {
                var byteKey = Encoding.UTF8.GetBytes(key);

                 var hash = new HMACSHA512(byteKey);
                    var mac_data =  hash.ComputeHash(Encoding.UTF8.GetBytes(data));
                
                result = bytesToHex(mac_data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }

        public static string HashHMAC256Hex(string data, string key)
        {
            string result = "";
            try
            {
                var byteKey = Encoding.UTF8.GetBytes(key);

                 var hash = new HMACSHA256(byteKey);
                    var mac_data =  hash.ComputeHash(Encoding.UTF8.GetBytes(data));
                
                result = bytesToHex(mac_data);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return result;
        }

        private static string bytesToHex(byte[] hashInBytes)
        {

            StringBuilder sb = new StringBuilder();

            foreach (byte b in hashInBytes)
            {
                string bufstring = String.Format("{0:X2}", b);
                sb.Append(bufstring);
            }

            return sb.ToString();

        }


        public static string HashHMACHex(string keyHex, string message)
        {
            var inp = Encoding.ASCII.GetBytes(keyHex);
            byte[] hash = HashHMAC(inp, StringEncode(message));
            return HashEncode(hash);
        }

        private static string HashSHAHex(string innerKeyHex, string outerKeyHex, string message)
        {
            byte[] hash = HashSHA(HexDecode(innerKeyHex), HexDecode(outerKeyHex), StringEncode(message));
            return HashEncode(hash);
        }

        private static byte[] HashHMAC(byte[] key, byte[] message)
        {
            var hash = new HMACSHA256(key);
            return hash.ComputeHash(message);
        }

        private static byte[] HashSHA(byte[] innerKey, byte[] outerKey, byte[] message)
        {
            var hash = new SHA256Managed();

            // Compute the hash for the inner data first
            byte[] innerData = new byte[innerKey.Length + message.Length];
            Buffer.BlockCopy(innerKey, 0, innerData, 0, innerKey.Length);
            Buffer.BlockCopy(message, 0, innerData, innerKey.Length, message.Length);
            byte[] innerHash = hash.ComputeHash(innerData);

            // Compute the entire hash
            byte[] data = new byte[outerKey.Length + innerHash.Length];
            Buffer.BlockCopy(outerKey, 0, data, 0, outerKey.Length);
            Buffer.BlockCopy(innerHash, 0, data, outerKey.Length, innerHash.Length);
            byte[] result = hash.ComputeHash(data);

            return result;
        }

        private static byte[] StringEncode(string text)
        {
            var encoding = new UTF8Encoding();
            return encoding.GetBytes(text);
        }

        private static string HashEncode(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToUpper();
        }

        private static byte[] HexDecode(string hex)
        {
            var bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = byte.Parse(hex.Substring(i * 2, 2), NumberStyles.HexNumber);
            }
            return bytes;
        }
    }
}
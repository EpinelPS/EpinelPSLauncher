using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using SharpCompress.Compressors.LZMA;
using ZstdSharp;

namespace EpinelPSLauncher.Utils
{
    public static class DataUtils
    {
        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }
        public static byte[] Decrypt(byte[] key, byte[] data, bool cbc = true, bool padding = true)
        {
            Aes aes = Aes.Create();
            aes.Padding = padding ? PaddingMode.PKCS7 : PaddingMode.None;
            // CBC: Encryption type 6
            aes.Mode = cbc ? CipherMode.CBC : CipherMode.ECB;
            aes.KeySize = 128;
            aes.Key = key;
            aes.IV = new byte[16];

            var _crypto = aes.CreateDecryptor(key, aes.IV);
            byte[] decrypted = _crypto.TransformFinalBlock(data, 0, data.Length);
            _crypto.Dispose();
            return decrypted;
        }

        public static Stream DecompressLZMA2(Stream data, int size, int streamSize)
        {
            var propertyByte = (byte)data.ReadByte();

            return new LzmaStream([propertyByte], data, streamSize - 1, size, null, true);
        }

        public static async Task<byte[]> DecompressZSTD(Stream data)
        {
            using var decompressor = new Decompressor();

            using var ms = new MemoryStream();
            await data.CopyToAsync(ms);

            var decompressed = decompressor.Unwrap(ms.ToArray()).ToArray();
            return decompressed;
        }

        public static async Task<byte[]> DecompressLZ4(Stream data, int decompressedSize, int compressedSize)
        {
            byte[] buffer = new byte[compressedSize];
            int read = await data.ReadAtLeastAsync(buffer.AsMemory(0, buffer.Length), compressedSize);
            if (read != compressedSize)
            {
                throw new Exception($"expected {compressedSize} bytes to be read");
            }

            byte[] target = new byte[decompressedSize];
            read = LZ4Codec.Decode(buffer, target);

            if (read == -1)
            {
                throw new Exception("insufficient buffer");
            }

            if (read != decompressedSize) throw new Exception();

            return target;
        }
    }
}

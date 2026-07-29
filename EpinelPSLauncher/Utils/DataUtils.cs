using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using SharpCompress.Compressors.LZMA;
using ZstdNet;

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
        public static byte[] Decrypt(byte[] key, byte[] data, int mode)
        {
            Aes aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;
            if (mode == 1 || mode == 10)
            {
                byte[] counter = new byte[16];
                return DecryptCTR(data, key, counter);
            }
            else if (mode == 3 || mode == 9)
                aes.Mode = CipherMode.ECB;
            else if (mode == 4 || mode == 7)
                aes.Mode = CipherMode.CFB;
            else if (mode == 5 || mode == 8)
                aes.Mode = CipherMode.OFB;
            else if (mode == 2 || mode == 6)
                aes.Mode = CipherMode.CBC;
            else throw new NotImplementedException();
            aes.KeySize = 128;
            aes.Key = key;
            aes.FeedbackSize = 128;
            aes.IV = new byte[16];

            if (aes.Mode == CipherMode.CFB)
            {
                // note that c# does not support cfb with any length it needs to be padded see https://github.com/dotnet/runtime/issues/85205
                int size = aes.GetCiphertextLengthCfb(data.Length, PaddingMode.Zeros, feedbackSizeInBits: 128);
                byte[] tempBuffer = new byte[size];
                data.AsSpan().CopyTo(tempBuffer);
                byte[] decrypted = aes.DecryptCfb(tempBuffer, new byte[16], PaddingMode.Zeros, feedbackSizeInBits: 128);
                return decrypted[0..data.Length];
            }
            else
            {
                using var _crypto = aes.CreateDecryptor(key, aes.IV);
                return _crypto.TransformFinalBlock(data, 0, data.Length);
            }
        }

        private static byte[] DecryptCTR(byte[] input, byte[] key, byte[] counter)
        {
            if (key.Length != 16)
                throw new ArgumentException("AES-128 key must be 16 bytes.");
            if (counter.Length != 16)
                throw new ArgumentException("Counter must be 16 bytes.");

            byte[] output = new byte[input.Length];
            byte[] ctr = (byte[])counter.Clone();
            byte[] keystream = new byte[16];

            using var aes = Aes.Create();
            aes.Key = key;
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;

            for (int offset = 0; offset < input.Length;)
            {
                aes.EncryptEcb(ctr, keystream, PaddingMode.None);

                int len = Math.Min(16, input.Length - offset);

                for (int i = 0; i < len; i++)
                    output[offset + i] = (byte)(input[offset + i] ^ keystream[i]);

                // CFB uses IV as a counter
                for (int i = 15; i >= 0; i--)
                {
                    if (++counter[i] != 0)
                        break;
                }
                offset += len;
            }

            return output;
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

            var decompressed = decompressor.Unwrap(ms.ToArray());
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

using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpinelPSLauncher.Utils
{
    public class TeaEncryption
    {
        private const uint Delta = 0x61C88647; // Modified TEA delta
        private const int NumRounds = 16;      // Number of rounds (half standard TEA)

        private static void Tea16DecryptBlock(Span<byte> block, uint[] key)
        {
            if (block.Length != 8) throw new ArgumentException("Block must be exactly 8 bytes.");
            if (key.Length != 4) throw new ArgumentException("Key must be 4 uints.");

            uint left = BinaryPrimitives.ReadUInt32BigEndian(block[..4]);
            uint right = BinaryPrimitives.ReadUInt32BigEndian(block.Slice(4, 4));
            uint sum = 0xE3779B90; // -Delta * 16 mod 2^32

            for (int i = 0; i < NumRounds; i++)
            {
                right -= ((left >> 5) + key[3]) ^ (sum + left) ^ ((left << 4) + key[2]);
                left -= ((right >> 5) + key[1]) ^ (sum + right) ^ ((right << 4) + key[0]);
                sum += Delta;
            }

            BinaryPrimitives.WriteUInt32BigEndian(block[..4], left);
            BinaryPrimitives.WriteUInt32BigEndian(block.Slice(4, 4), right);
        }

        public static int Decrypt(byte[] ciphertext, byte[] key, out byte[] plaintext)
        {
            plaintext = [];

            if (ciphertext == null || ciphertext.Length < 16 || (ciphertext.Length % 8) != 0)
                return -1;
            if (key == null || key.Length != 16)
                return -1;

            int totalLength = ciphertext.Length;

            // Convert key to 4-element uint array (big-endian)
            uint[] keyParts = new uint[4];
            for (int i = 0; i < 4; i++)
                keyParts[i] = BinaryPrimitives.ReadUInt32BigEndian(key.AsSpan(i * 4, 4));

            byte[] decryptedBlock = new byte[8];
            byte[] xorBuffer = new byte[8];
            byte[] resultBuffer = new byte[totalLength]; // Will be trimmed later

            // Step 1: Decrypt the first 8 bytes (header block)
            Array.Copy(ciphertext, 0, decryptedBlock, 0, 8);
            Tea16DecryptBlock(decryptedBlock, keyParts);

            int paddingLength = decryptedBlock[0] & 7;
            int plaintextLength = totalLength - paddingLength - 10;

            if (plaintextLength < 0 || plaintextLength > resultBuffer.Length)
                return -1;

            int resultIndex = 0;
            int cipherIndex = 8;
            int blockByteIndex = paddingLength + 1;

            byte[] previousCipherBlock = new byte[8];
            Array.Copy(ciphertext, 0, previousCipherBlock, 0, 8);

            byte[] nextCipherBlock = new byte[8];
            byte[] currentXorKey = new byte[8];
            Array.Clear(xorBuffer, 0, 8);

            int decryptionStep = 1;

            // Stage 1: Skip padding bytes in decryptedBlock
            while (decryptionStep <= 2)
            {
                if (blockByteIndex >= 8)
                {
                    if (blockByteIndex == 8)
                    {
                        currentXorKey = ciphertext.AsSpan(cipherIndex - 8, 8).ToArray();
                        ciphertext.AsSpan(cipherIndex, 8).CopyTo(nextCipherBlock);
                        cipherIndex += 8;

                        for (int i = 0; i < 8; i++)
                            decryptedBlock[i] ^= nextCipherBlock[i];

                        Tea16DecryptBlock(decryptedBlock, keyParts);
                        blockByteIndex = 0;
                        continue;
                    }
                }
                else
                {
                    blockByteIndex++;
                    decryptionStep++;
                }
            }

            // Stage 2: Extract plaintext
            int remainingBytes = plaintextLength;
            while (remainingBytes > 0)
            {
                if (blockByteIndex >= 8)
                {
                    if (blockByteIndex == 8)
                    {
                        currentXorKey = ciphertext.AsSpan(cipherIndex - 8, 8).ToArray();
                        ciphertext.AsSpan(cipherIndex, 8).CopyTo(nextCipherBlock);
                        cipherIndex += 8;

                        for (int i = 0; i < 8; i++)
                            decryptedBlock[i] ^= nextCipherBlock[i];

                        Tea16DecryptBlock(decryptedBlock, keyParts);
                        blockByteIndex = 0;
                    }
                }
                else
                {
                    resultBuffer[resultIndex++] = (byte)(currentXorKey[blockByteIndex] ^ decryptedBlock[blockByteIndex]);
                    blockByteIndex++;
                    remainingBytes--;
                }
            }

            // Stage 3: Trailer integrity check
            for (int i = 1; i <= 7; i++)
            {
                if (blockByteIndex >= 8)
                {
                    if (blockByteIndex == 8)
                    {
                        currentXorKey = ciphertext.AsSpan(cipherIndex - 8, 8).ToArray();
                        ciphertext.AsSpan(cipherIndex, 8).CopyTo(nextCipherBlock);
                        cipherIndex += 8;

                        for (int j = 0; j < 8; j++)
                            decryptedBlock[j] ^= nextCipherBlock[j];

                        Tea16DecryptBlock(decryptedBlock, keyParts);
                        blockByteIndex = 0;
                    }
                }

                if (currentXorKey[blockByteIndex] != decryptedBlock[blockByteIndex])
                    return -1;

                blockByteIndex++;
            }

            plaintext = new byte[plaintextLength];
            Array.Copy(resultBuffer, 0, plaintext, 0, plaintextLength);
            return plaintextLength;
        }
    }
}

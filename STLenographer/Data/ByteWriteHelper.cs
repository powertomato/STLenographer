using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace STLenographer.Data
{
    public class ByteWriteHelper
    {

        private List<byte> data;
        private bool[] currentByte;
        private int currentPtr;
        private int currentDataPtr;
        private ICryptoTransform encryptor;
        private List<byte> dataUnencrypted;
        private bool dataFinalized;

        public static byte[] HexStringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                      .Where(x => x % 2 == 0)
                      .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                      .ToArray();
        }
        public ByteWriteHelper(bool sholdEncrypt, string keyOrPassword)
        {

            this.data = new List<byte>();

            if (sholdEncrypt)
            {
                AesManaged aes = new AesManaged();
                aes.Padding = PaddingMode.Zeros;
                aes.KeySize = 256;
                aes.GenerateIV();
                this.data.AddRange(aes.IV);
                if (keyOrPassword.Length == 64 && System.Text.RegularExpressions.Regex.IsMatch(keyOrPassword, @"\A\b[0-9a-fA-F]+\b\Z"))
                {
                    aes.Key = HexStringToByteArray(keyOrPassword);
                }
                else
                {
                    PasswordDeriveBytes password = new PasswordDeriveBytes(keyOrPassword, new byte[] { 0x22, 0xf0, 0x2d, 0x47, 0x2f, 0x97, 0xee, 0xb1 }, "SHA1", 2);
                    aes.Key = password.GetBytes(256 / 8);
                }
                encryptor = aes.CreateEncryptor();
                dataUnencrypted = new List<byte>();
            }

            currentByte = new bool[8];
            currentDataPtr = 0;
            currentPtr = 0;
            dataFinalized = false;
        }

        public void AppendData(IEnumerable<byte> _data)
        {
            if (encryptor != null)
            {
                if (dataFinalized)
                {
                    throw new InvalidOperationException("Can't add data! Encryption has been finalized!");
                }
                dataUnencrypted.AddRange(_data);
            }
            else
            {
                data.AddRange(_data);
            }
        }

        public void FinalizeData()
        {
            if (encryptor != null)
            {
                byte[] result = new byte[16 * getNumberOfChunks(dataUnencrypted.Count)];
                byte[] dataChunk = dataUnencrypted.Take(16).ToArray();
                CryptoStream encr_str = new CryptoStream(new MemoryStream(result), encryptor, CryptoStreamMode.Write);
                encr_str.Write(dataUnencrypted.ToArray(), 0, dataUnencrypted.Count);
                encr_str.Close();
                data.AddRange(result);

                dataUnencrypted.Clear();
            }
            dataFinalized = true;
        }

        private static int getNumberOfChunks(int num)
        {
            return ((num + 15) / 16);
        }


        public bool HasUnencodedData()
        {
            return (currentDataPtr < data.Count);
        }

        public bool GetCurrentBit()
        {
            checkCapacity();
            return (data[currentDataPtr] & (1 << currentPtr)) != 0;
        }

        public bool GetAndMoveCurrentBit()
        {
            bool ret = GetCurrentBit();
            MoveNext();
            return ret;
        }

        public bool MoveNext()
        {
            checkCapacity();
            currentPtr++;
            if (currentPtr >= 8)
            {
                currentPtr = 0;
                currentDataPtr++;
            }
            return HasUnencodedData();
        }

        private void checkCapacity()
        {
            if (!HasUnencodedData())
            {
                throw new InvalidOperationException("No data left!");
            }
        }
    }
}

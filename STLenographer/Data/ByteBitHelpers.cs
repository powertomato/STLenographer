using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace STLenographer.Data {
    public class ByteWriteHelper {

        private List<byte> data;
        private bool[] currentByte;
        private int currentPtr;
        private int currentDataPtr;
        private ICryptoTransform encryptor;
        private List<byte> dataUnencrypted;
        private bool dataFinalized;

        public static byte[] HexStringToByteArray(string hex) {
            return Enumerable.Range(0, hex.Length)
                      .Where(x => x % 2 == 0)
                      .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                      .ToArray();
        }
        public ByteWriteHelper(bool sholdEncrypt, string keyOrPassword) {
            
            this.data = new List<byte>();

            if (sholdEncrypt) {
                AesManaged aes = new AesManaged();
                aes.Padding = PaddingMode.Zeros;
                aes.KeySize = 256;
                aes.GenerateIV();
                this.data.AddRange(aes.IV);
                if (keyOrPassword.Length == 64 && System.Text.RegularExpressions.Regex.IsMatch(keyOrPassword, @"\A\b[0-9a-fA-F]+\b\Z")) {
                    aes.Key = HexStringToByteArray(keyOrPassword);
                } else {
                    PasswordDeriveBytes password = new PasswordDeriveBytes(keyOrPassword, new byte[] { 0x22, 0xf0, 0x2d, 0x47, 0x2f, 0x97,0xee, 0xb1 }, "SHA1", 2);
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

        public void AppendData(IEnumerable<byte> _data) {
            if(encryptor != null) {
                if(dataFinalized) {
                    throw new InvalidOperationException("Can't add data! Encryption has been finalized!");
                }
                dataUnencrypted.AddRange(_data);
            } else {
                data.AddRange(_data);
            }
        }

        public void FinalizeData() {
            if (encryptor != null) {
                byte[] result = new byte[16*getNumberOfChunks(dataUnencrypted.Count)];
                byte[] dataChunk = dataUnencrypted.Take(16).ToArray();
                CryptoStream encr_str = new CryptoStream(new MemoryStream(result), encryptor, CryptoStreamMode.Write);
                encr_str.Write(dataUnencrypted.ToArray(), 0, dataUnencrypted.Count);
                encr_str.Close();
                data.AddRange(result);

                dataUnencrypted.Clear();
            }
            dataFinalized = true;
        }

        private static int getNumberOfChunks(int num) {
            return ((num + 15) / 16);
        }


        public bool HasData() {
            return (currentDataPtr < data.Count);
        }

        public bool GetCurrentBit() {
            checkCapacity();
            return (data[currentDataPtr] & (1<<currentPtr)) != 0;
        }

        public bool GetAndMoveCurrentBit() {
            bool ret = GetCurrentBit();
            MoveNext();
            return ret;
        }

        public bool MoveNext() {
            checkCapacity();
            currentPtr++;
            if(currentPtr >= 8) {
                currentPtr = 0;
                currentDataPtr++;
            }
            return HasData();
        }

        private void checkCapacity() {
            if (!HasData()) {
                throw new InvalidOperationException("No data left!");
            }
        }
    }

    public class ByteReadHelper {
        
        private byte[] dataLenBytes;
        private int dataLen;
        private int dataRead;

        private bool magicByteRead;
        private bool magicByteInvalid;

        private List<byte> dataUnprocessed;
        private List<byte> data;
        private byte currentByte;
        private int currentPtr;

        private bool shouldDecrypt;
        private byte[] key;
        private ICryptoTransform decryptor;


        public int Version { get; private set; }
        private bool versionRead;

        public ByteReadHelper(bool shouldDecrypt, string keyOrPassword) {
            if (shouldDecrypt) {
                if (keyOrPassword.Length == 64 && System.Text.RegularExpressions.Regex.IsMatch(keyOrPassword, @"\A\b[0-9a-fA-F]+\b\Z")) {
                    key = ByteWriteHelper.HexStringToByteArray(keyOrPassword);
                } else {
                    PasswordDeriveBytes password = new PasswordDeriveBytes(keyOrPassword, new byte[] { 0x22, 0xf0, 0x2d, 0x47, 0x2f, 0x97,0xee, 0xb1 }, "SHA1", 2);
                    key = password.GetBytes(256 / 8);
                }
            }

            this.dataUnprocessed = new List<byte>();
            this.data = new List<byte>();
            currentByte = 0;
            currentPtr = 0;

            //First 4 bytes read represent the data length
            dataLenBytes = new byte[4];
            dataLen = -1;
            dataRead = 0;

            magicByteInvalid = false;
            magicByteRead = false;
            versionRead = false;
            this.shouldDecrypt = shouldDecrypt;
        }

        public List<byte> Data { get { return data; } }

        private bool headerRead() {
            return dataLen != -1;
        }

        public bool ReadEverything() {
            return headerRead() && dataRead == dataLen;
        }

        public bool MoveNext() {
            if (ReadEverything()) {
                return true;
            }
            currentPtr++;
            if (currentPtr >= 8) {
                currentPtr = 0;
                if (shouldDecrypt) {
                    dataUnprocessed.Add(currentByte);
                    checkChunk();
                } else {
                    processByte(currentByte);
                }
                currentByte = 0;
                
            }
            return ReadEverything();
        }

        private void checkChunk() {
            if (dataUnprocessed.Count >= 16) {
                if (decryptor == null) {
                    AesManaged aes = new AesManaged();
                    aes.Padding = PaddingMode.Zeros;
                    aes.KeySize = 256;
                    aes.Key = key;
                    aes.IV = dataUnprocessed.Take(16).ToArray();
                    decryptor = aes.CreateDecryptor();
                } else {
                    byte[] result = new byte[16];
                    decryptor.TransformBlock(dataUnprocessed.Take(16).ToArray(), 0, 16, result, 0);
                    foreach(byte curByte in result) {
                        processByte(curByte);
                    }
                }
                dataUnprocessed.RemoveRange(0, 16);
            }
        }

        private void processByte(byte curByte) {
            if (ReadEverything()) {
                return;
            }
            if (headerRead()) {
                data.Add(curByte);
            } else {

                if (magicByteRead) {
                    if (versionRead) {
                        dataLenBytes[dataRead] = curByte;
                        if (dataRead == 3) {
                            dataRead = 0;
                            dataLen = BitConverter.ToInt32(dataLenBytes, 0);
                            return;
                        }
                    } else {
                        Version = curByte;
                        versionRead = true;
                        return;
                    }
                } else {
                    if (curByte == 0x77 && !magicByteInvalid) {
                        magicByteRead = true;
                    } else {
                        magicByteInvalid = true;
                    }
                    return;
                }
            }
            dataRead++;
        }

        public void SetCurrentBit(bool bit) {
            byte tmp = (byte)(1 << currentPtr);
            if (bit) {
                currentByte |= tmp;
            } else {
                currentByte &= (byte) ~tmp;
            }
        }

        public void SetCurrentBitAndMove(bool bit) {
            SetCurrentBit(bit);
            MoveNext();
        }

        private void checkCapacity() {
            if(ReadEverything()) {
                throw new InvalidOperationException("Can't read more than expected data length!");
            }
        }
    }
}

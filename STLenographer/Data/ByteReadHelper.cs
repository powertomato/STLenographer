using System.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;

namespace STLenographer.Data {
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

        private bool hasReadHeader() {
            return dataLen != -1;
        }

        public bool HasReadEverything() {
            return hasReadHeader() && dataRead == dataLen;
        }

        public bool MoveNext() {
            if (HasReadEverything()) {
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
            return HasReadEverything();
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
            if (HasReadEverything()) {
                return;
            }
            if (hasReadHeader()) {
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
            if(HasReadEverything()) {
                throw new InvalidOperationException("Can't read more than expected data length!");
            }
        }
    }
}

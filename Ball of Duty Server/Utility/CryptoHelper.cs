using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ball_of_Duty_Server.Utility
{
    public class CryptoHelper : IDisposable
    {
        public const int SALT_SIZE = 32;
        public const int HASH_SIZE = 32;
        public const int IV_LENGTH = 16;
        private const int HASH_ITERATIONS = 1500;

        private bool _disposed = false;
        private RijndaelManaged _rm = new RijndaelManaged();
        private byte[] _hash;

        public CryptoHelper(byte[] hash)
        {
            _hash = hash;
        }

        public CryptoHelper(string hash, byte[] salt)
        {
            _hash = GenerateHash(hash, salt);
        }

        public byte[] GenerateIV()
        {
            _rm.GenerateIV();
            return _rm.IV;
        }

        public byte[] Encrypt(byte[] buffer)
        {
            return Transform(buffer, _rm.CreateEncryptor(_hash, _rm.IV));
        }

        public byte[] Decrypt(byte[] buffer, byte[] iv)
        {
            if (buffer.Length % (_rm.BlockSize / 8) != 0)
                throw new CryptographicException("Invalid block size");

            return Transform(buffer, _rm.CreateDecryptor(_hash, iv));
        }

        private static byte[] Transform(byte[] buffer, ICryptoTransform transform)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (CryptoStream cs = new CryptoStream(ms, transform, CryptoStreamMode.Write))
                {
                    cs.Write(buffer, 0, buffer.Length);
                    cs.FlushFinalBlock();
                    return ms.ToArray();
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool safeToFreeManaged)
        {
            if (!_disposed)
            {
                if (safeToFreeManaged)
                {
                    _rm?.Dispose();
                }
                // free unmanaged resources here.

                _disposed = true;
            }
        }

        ~CryptoHelper()
        {
            Dispose(false);
        }

        public static byte[] GenerateSalt()
        {
            return FillRandomly(new byte[SALT_SIZE]);
        }

        public static byte[] GenerateHash(string password, byte[] salt)
        {
            using (Rfc2898DeriveBytes pbkdf2 = new Rfc2898DeriveBytes(password, salt, HASH_ITERATIONS))
                return pbkdf2.GetBytes(HASH_SIZE);
        }

        public static byte[] FillRandomly(byte[] buffer)
        {
            using (RNGCryptoServiceProvider csprng = new RNGCryptoServiceProvider())
            {
                csprng.GetBytes(buffer);
            }

            return buffer;
        }
    }
}
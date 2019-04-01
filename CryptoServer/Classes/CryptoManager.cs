using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SyslogLogging;

namespace Kvpbase.Classes
{
    /// <summary>
    /// Crypto manager responsible for encryption and decryption operations.
    /// </summary>
    public class CryptoManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private Settings _Settings;
        private LoggingModule _Logging;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="settings">Server settings.</param>
        /// <param name="logging">Logging instance.</param>
        public CryptoManager(Settings settings, LoggingModule logging)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (logging == null) throw new ArgumentNullException(nameof(logging));

            _Settings = settings;
            _Logging = logging;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Encrypt data.
        /// </summary>
        /// <param name="data">Data to encrypt.</param>
        /// <param name="ret">Encrypted object and metadata.</param>
        /// <param name="failureReason">Failure reason, if any.</param>
        /// <returns>True if successful.</returns>
        public bool Encrypt(byte[] data, out Obj ret, out string failureReason)
        {
            ret = new Obj();
            failureReason = "";
             
            ret.StartTime = DateTime.Now;
            ret.Clear = data;

            string sessionKey = "";
            string ksn = "";
             
            if (!CreateSessionKey(out sessionKey, out ksn))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Encrypt unable to generate session key and ksn");
                failureReason = "Unable to generate session key and KSN.";
                return false;
            }

            ret.SessionKey = sessionKey;
            ret.Ksn = ksn;
              
            ret.Cipher = EncryptInternal(ret.Clear, ret.SessionKey);
            ret.EndTime = DateTime.Now;

            TimeSpan ts = Convert.ToDateTime(ret.EndTime) - Convert.ToDateTime(ret.StartTime);
            ret.TotalTimeMs = Convert.ToDecimal(Common.DecimalToString(Convert.ToDecimal(ts.TotalMilliseconds)));
             
            if (ret.Cipher == null || ret.Cipher.Length < 1)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Encrypt null value for cipher after encryption");
                failureReason = "Null value for cipher after encryption.";
                return false;
            }
             
            ret.Clear = null;
            ret.Passphrase = null;
            ret.Salt = null;
            ret.InitVector = null;
            ret.SessionKey= null;

            _Logging.Log(LoggingModule.Severity.Debug, "Encrypt encrypted " + data.Length + " clear bytes to " + ret.Cipher.Length + " cipher bytes");
            return true; 
        }

        /// <summary>
        /// Decrypt data.
        /// </summary>
        /// <param name="req">Encrypted object and metadata.</param>
        /// <param name="ret">Cleartext data.</param>
        /// <param name="failureReason">Failure reason, if any.</param>
        /// <returns>True if successful.</returns>
        public bool Decrypt(Obj req, out byte[] ret, out string failureReason)
        {
            failureReason = "";
            ret = null;
              
            req.Passphrase = _Settings.Crypto.Passphrase;
            req.InitVector = _Settings.Crypto.InitVector;
            req.Salt = _Settings.Crypto.Salt;

            req.StartTime = DateTime.Now;
             
            string sessionKey = "";

            if (!GetSessionKey(req.Ksn, out sessionKey))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Decrypt unable to derive session key");
                failureReason = "Unable to derive session key.";
                return false;
            }

            req.SessionKey = sessionKey;
             
            req.Clear = DecryptInternal(req.Cipher, req.SessionKey);
            req.EndTime = DateTime.Now;

            TimeSpan ts = Convert.ToDateTime(req.EndTime) - Convert.ToDateTime(req.StartTime);
            req.TotalTimeMs = Convert.ToDecimal(Common.DecimalToString(Convert.ToDecimal(ts.TotalMilliseconds)));
             
            if (req.Clear == null || req.Clear.Length < 1)
            {
                _Logging.Log(LoggingModule.Severity.Warn, "Decrypt null value for clear after decryption");
                failureReason = "Null value for cleartext after decryption.";
                return false;
            }

            _Logging.Log(LoggingModule.Severity.Debug, "Decrypt decrypted " + req.Cipher.Length + " cipher bytes to " + req.Clear.Length + " clear bytes");

            req.Cipher = null;
            req.Passphrase = null;
            req.Salt = null;
            req.InitVector = null;
            req.SessionKey = null;
            req.Ksn = null;

            ret = req.Clear;
            return true; 
        }

        #endregion

        #region Private-Methods

        private bool CreateSessionKey(out string sessionKey, out string ksn)
        {
            sessionKey = "";
            ksn = "";
             
            DateTime dt = DateTime.Now;
            sessionKey = dt.ToString("MMddyyyyhhmmssff");  // 16 bytes
            byte[] ksnBytes = EncryptInternal(Encoding.UTF8.GetBytes(sessionKey), _Settings.Crypto.Passphrase);
            ksn = Convert.ToBase64String(ksnBytes);
            return true; 
        }

        private bool GetSessionKey(string ksn, out string sessionKey)
        {
            sessionKey = "";
             
            if (String.IsNullOrEmpty(ksn))
            {
                _Logging.Log(LoggingModule.Severity.Warn, "GetSessionKey null ksn supplied");
                return false;
            }

            byte[] sessionKeyBytes = DecryptInternal(Convert.FromBase64String(ksn), _Settings.Crypto.Passphrase);
            sessionKey = Encoding.UTF8.GetString(sessionKeyBytes); 

            if (!String.IsNullOrEmpty(sessionKey))
            {
                if (sessionKey.Length == 16)
                {
                    return true;
                }
                else
                {
                    _Logging.Log(LoggingModule.Severity.Warn, "GetSessionKey session key length was not 16, returning null (result: " + sessionKey + ")");
                    sessionKey = "";
                    return false;
                }
            }
            else
            {
                _Logging.Log(LoggingModule.Severity.Warn, "GetSessionKey null session key after decryption operation");
                return false;
            } 
        }
        
        private byte[] EncryptInternal(byte[] clear, string passphrase)
        {
            // Taken from http://www.obviex.com/samples/Encryption.aspx
            // First, we must create a password, from which the key will be derived.
            // This password will be generated from the specified passphrase and 
            // salt value. The password will be created using the specified hash 
            // algorithm. Password creation can be done in several iterations.
            PasswordDeriveBytes password = new PasswordDeriveBytes(
                                                            passphrase,
                                                            Encoding.UTF8.GetBytes(_Settings.Crypto.Salt),
                                                            "SHA1",
                                                            2);

            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
            byte[] keyBytes = password.GetBytes(256 / 8);

            // Create uninitialized Rijndael encryption object.
            RijndaelManaged symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;

            // Generate encryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            ICryptoTransform encryptor = symmetricKey.CreateEncryptor(
                                                             keyBytes,
                                                             Encoding.UTF8.GetBytes(_Settings.Crypto.InitVector));

            // Define memory stream which will be used to hold encrypted data.
            MemoryStream memoryStream = new MemoryStream();

            // Define cryptographic stream (always use Write mode for encryption).
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                                                         encryptor,
                                                         CryptoStreamMode.Write);
            // Start encrypting.
            cryptoStream.Write(clear, 0, clear.Length);

            // Finish encrypting.
            cryptoStream.FlushFinalBlock();

            // Convert our encrypted data from a memory stream into a byte array.
            byte[] cipherTextBytes = memoryStream.ToArray();
            
            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();

            return cipherTextBytes;
        }
        
        private byte[] DecryptInternal(byte[] cipher, string passphrase)
        {
            // Taken from http://www.obviex.com/samples/Encryption.aspx
            // First, we must create a password, from which the key will be 
            // derived. This password will be generated from the specified 
            // passphrase and salt value. The password will be created using
            // the specified hash algorithm. Password creation can be done in
            // several iterations.
            PasswordDeriveBytes password = new PasswordDeriveBytes(
                                                            passphrase,
                                                            Encoding.UTF8.GetBytes(_Settings.Crypto.Salt),
                                                            "SHA1",
                                                            2);

            // Use the password to generate pseudo-random bytes for the encryption
            // key. Specify the size of the key in bytes (instead of bits).
            byte[] keyBytes = password.GetBytes(256 / 8);

            // Create uninitialized Rijndael encryption object.
            RijndaelManaged symmetricKey = new RijndaelManaged();

            // It is reasonable to set encryption mode to Cipher Block Chaining
            // (CBC). Use default options for other symmetric key parameters.
            symmetricKey.Mode = CipherMode.CBC;

            // Generate decryptor from the existing key bytes and initialization 
            // vector. Key size will be defined based on the number of the key 
            // bytes.
            ICryptoTransform decryptor = symmetricKey.CreateDecryptor(
                                                             keyBytes,
                                                             Encoding.UTF8.GetBytes(_Settings.Crypto.InitVector));

            // Define memory stream which will be used to hold encrypted data.
            MemoryStream memoryStream = new MemoryStream(cipher);

            // Define cryptographic stream (always use Read mode for encryption).
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                                                          decryptor,
                                                          CryptoStreamMode.Read);

            // Since at this point we don't know what the size of decrypted data
            // will be, allocate the buffer long enough to hold ciphertext;
            // plaintext is never longer than ciphertext.
            byte[] plainTextBytes = new byte[cipher.Length];

            // Start decrypting.
            int decryptedByteCount = cryptoStream.Read(plainTextBytes,
                                                       0,
                                                       plainTextBytes.Length);

            // Close both streams.
            memoryStream.Close();
            cryptoStream.Close();

            byte[] ret = new byte[decryptedByteCount];
            Buffer.BlockCopy(plainTextBytes, 0, ret, 0, decryptedByteCount);
            return ret;
        }
        
        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvpbase
{ 
    /// <summary>
    /// Metadata and data container for cleartext and ciphertext data.
    /// </summary>
    public class Obj
    {
        /// <summary>
        /// Cleartext byte data.
        /// </summary>
        public byte[] Clear { get; set; }

        /// <summary>
        /// Ciphertext byte data.
        /// </summary>
        public byte[] Cipher { get; set; }
        
        /// <summary>
        /// Encryption passphrase.
        /// </summary>
        public string Passphrase { get; set; }

        /// <summary>
        /// Encryption salt.
        /// </summary>
        public string Salt { get; set; }

        /// <summary>
        /// Encryption initialization vector.
        /// </summary>
        public string InitVector { get; set; }

        /// <summary>
        /// Encryption key sequence number.
        /// </summary>
        public string Ksn { get; set; }

        /// <summary>
        /// Encryption session key.
        /// </summary>
        public string SessionKey { get; set; }

        /// <summary>
        /// Start time of the encryption or decryption operation.
        /// </summary>
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// End time of the encryption or decryption operation.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Total time in milliseconds for the operation.
        /// </summary>
        public decimal TotalTimeMs { get; set; }
    } 
}

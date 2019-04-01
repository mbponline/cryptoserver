using System;
using System.Collections.Generic;

namespace Kvpbase.Classes
{
    /// <summary>
    /// Server settings.
    /// </summary>
    public class Settings
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable server console.
        /// </summary>
        public bool EnableConsole;

        /// <summary>
        /// Server settings.
        /// </summary>
        public SettingsServer Server;

        /// <summary>
        /// Crypto settings.
        /// </summary>
        public SettingsCrypto Crypto;

        /// <summary>
        /// Authentication settings.
        /// </summary>
        public SettingsAuth Auth;

        /// <summary>
        /// Logging settings.
        /// </summary>
        public SettingsLogging Logging;

        /// <summary>
        /// REST settings.
        /// </summary>
        public SettingsRest Rest;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        public Settings()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Load settings from file.
        /// </summary>
        /// <param name="filename">Filename and path.</param>
        /// <returns>Settings.</returns>
        public static Settings FromFile(string filename)
        {
            return Common.DeserializeJson<Settings>(Common.ReadTextFile(filename));
        }

        #endregion

        #region Private-Methods

        #endregion
    }
    
    /// <summary>
    /// Server settings.
    /// </summary>
    public class SettingsServer
    {
        #region Public-Members

        /// <summary>
        /// Hostname on which to listen.
        /// </summary>
        public string DnsHostname;

        /// <summary>
        /// TCP port on which to listen.
        /// </summary>
        public int Port;

        /// <summary>
        /// Enable or disable SSL.
        /// </summary>
        public bool Ssl;
        
        #endregion
    }
      
    /// <summary>
    /// Crypto settings (encryption, decryption).
    /// </summary>
    public class SettingsCrypto
    {
        #region Public-Members

        /// <summary>
        /// Base passphrase.
        /// </summary>
        public string Passphrase;

        /// <summary>
        /// Base salt.
        /// </summary>
        public string Salt;

        /// <summary>
        /// Base initialization vector.
        /// </summary>
        public string InitVector;

        #endregion
    }

    /// <summary>
    /// Authentication settings.
    /// </summary>
    public class SettingsAuth
    {
        #region Public-Members

        /// <summary>
        /// API key header.
        /// </summary>
        public string ApiKeyHeader;

        /// <summary>
        /// Admin API key.
        /// </summary>
        public string AdminApiKey;

        /// <summary>
        /// API key for cryptography operations.
        /// </summary>
        public string CryptoApiKey;

        #endregion
    }

    /// <summary>
    /// Logging settings.
    /// </summary>
    public class SettingsLogging
    {
        #region Public-Members

        /// <summary>
        /// IP address or hostname for the syslog server.
        /// </summary>
        public string SyslogServerIp;

        /// <summary>
        /// Syslog server port number.
        /// </summary>
        public int SyslogServerPort;

        /// <summary>
        /// Minimum severity required to send a log message.
        /// </summary>
        public int MinimumSeverityLevel;

        /// <summary>
        /// Enable or disable logging of incoming requests.
        /// </summary>
        public bool LogRequests;

        /// <summary>
        /// Enable or disable logging of outgoing responses.
        /// </summary>
        public bool LogResponses;

        /// <summary>
        /// Enable or disable logging.
        /// </summary>
        public bool ConsoleLogging;

        #endregion
    }
    
    /// <summary>
    /// REST settings.
    /// </summary>
    public class SettingsRest
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable use of a web proxy.
        /// </summary>
        public bool UseWebProxy;

        /// <summary>
        /// Web proxy URL.
        /// </summary>
        public string WebProxyUrl;

        /// <summary>
        /// Enable acceptance of SSL certificates that are unable to be validated.
        /// </summary>
        public bool AcceptInvalidCerts;

        #endregion
    }
}

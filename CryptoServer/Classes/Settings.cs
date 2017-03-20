using System;
using System.Collections.Generic;

namespace Kvpbase
{
    public class Settings
    {
        #region Public-Members

        public int EnableConsole;

        public SettingsServer Server;
        public SettingsCrypto Crypto;
        public SettingsAuth Auth;
        public SettingsLogging Syslog;
        public SettingsRest Rest;

        #endregion

        #region Constructors-and-Factories

        public Settings()
        {

        }

        #endregion

        #region Public-Methods

        public static Settings FromFile(string filename)
        {
            return Common.DeserializeJson<Settings>(Common.ReadTextFile(filename));
        }

        #endregion
    }
    
    public class SettingsServer
    {
        #region Public-Members

        public string DnsHostname;
        public int Port;
        public int Ssl;
        
        #endregion
    }
      
    public class SettingsCrypto
    {
        #region Public-Members

        public string Passphrase;
        public string Salt;
        public string InitVector;

        #endregion
    }

    public class SettingsAuth
    {
        #region Public-Members

        public string ApiKeyHeader;
        public string AdminApiKey;
        public string CryptoApiKey;

        #endregion
    }

    public class SettingsLogging
    {
        #region Public-Members

        public string SyslogServerIp;
        public int SyslogServerPort;
        public int MinimumSeverityLevel;
        public int LogRequests;
        public int LogResponses;
        public int ConsoleLogging;

        #endregion
    }
    
    public class SettingsRest
    {
        #region Public-Members

        public int UseWebProxy;
        public string WebProxyUrl;
        public int AcceptInvalidCerts;

        #endregion
    }
}

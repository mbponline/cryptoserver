using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Kvpbase
{
    public class Setup
    {
        #region Constructors-and-Factories

        public Setup()
        {
            RunSetup();
        }

        #endregion

        #region Private-Methods

        private void RunSetup()
        { 
            #region Variables

            DateTime ts = DateTime.Now;
            Settings ret = new Settings();
            
            #endregion

            #region Welcome

            Console.WriteLine("");
            Console.WriteLine(@"   _             _                    ");
            Console.WriteLine(@"  | |____ ___ __| |__  __ _ ___ ___   ");
            Console.WriteLine(@"  | / /\ V / '_ \ '_ \/ _` (_-</ -_)  ");
            Console.WriteLine(@"  |_\_\ \_/| .__/_.__/\__,_/__/\___|  ");
            Console.WriteLine(@"           |_|                        ");
            Console.WriteLine(@"                                      ");
            Console.ResetColor();

            Console.WriteLine("");
            Console.WriteLine("Kvpbase CryptoServer");
            Console.WriteLine("");
                            //          1         2         3         4         5         6         7
                            // 12345678901234567890123456789012345678901234567890123456789012345678901234567890
            Console.WriteLine("Thank you for using kvpbase!  We'll put together a basic system configuration");
            Console.WriteLine("so you can be up and running quickly.");
            Console.WriteLine("");
            Console.WriteLine("Press ENTER to get started.");
            Console.WriteLine("");
            Console.WriteLine(Common.Line(79, "-"));
            Console.ReadLine();

            #endregion

            #region Initial-Settings
            
            ret.EnableConsole = 1; 

            #endregion
              
            #region Server

            ret.Server = new SettingsServer();
            ret.Server.Ssl = 0;
            ret.Server.Port = Common.InputInteger("On which TCP port shall this node listen?", 9000, true, false);
            ret.Server.DnsHostname = Common.InputString("On which hostname shall this node listen?", "localhost", false);

            Console.WriteLine("This node is configured to use HTTP (not HTTPS) and is accessible at:");
            Console.WriteLine("");
            Console.WriteLine("  http://" + ret.Server.DnsHostname + ":" + ret.Server.Port);
             
            Console.WriteLine("");
            Console.WriteLine("Be sure to install an SSL certificate and modify your config file to");
            Console.WriteLine("use SSL to maximize security and set the correct hostname.");
            Console.WriteLine("");

            #endregion

            #region Crypto

            ret.Crypto = new SettingsCrypto();

            //                          1         2         3         4         5         6         7
            //                 12345678901234567890123456789012345678901234567890123456789012345678901234567890
            Console.WriteLine("Let's configure your encryption material, i.e. the super-secret key");
            Console.WriteLine("and other pieces that are used to generate keys that are then used");
            Console.WriteLine("to encrypt and decrypt your data.");
            Console.WriteLine("");

            if (Common.InputBoolean("Would you like us to create encryption material for you?", true))
            {
                ret.Crypto.InitVector = Common.RandomHexString(16);
                Thread.Sleep(10);

                ret.Crypto.Passphrase = Common.RandomHexString(16);
                Thread.Sleep(100);

                ret.Crypto.Salt = Common.RandomHexString(16);
            }
            else
            {
                while (true)
                {
                    Console.WriteLine("All of these values should be 16 hexadecimal characters.");
                    Console.WriteLine("                   i.e. 0123456789ABCDEF");

                    ret.Crypto.Passphrase = Common.InputString("Passphrase            : ", null, false);
                    ret.Crypto.InitVector = Common.InputString("Initialization Vector : ", null, false);
                    ret.Crypto.Salt = Common.InputString("Salt                  : ", null, false);

                    if (ret.Crypto.Passphrase.Length != 16 ||
                        ret.Crypto.InitVector.Length != 16 ||
                        ret.Crypto.Salt.Length != 16)
                    {
                        Console.WriteLine("All values must be 16 characters in length");
                        continue;
                    }

                    if (!Common.IsHexString(ret.Crypto.Passphrase) ||
                        !Common.IsHexString(ret.Crypto.InitVector) ||
                        !Common.IsHexString(ret.Crypto.Salt))
                    {
                        Console.WriteLine("All values must be hexadecimal strings, i.e. 0123456789ABCDEF");
                        continue;
                    }

                    Console.WriteLine("");
                    break;
                } 
            }
            
            #endregion

            #region Auth

            ret.Auth = new SettingsAuth();
            ret.Auth.ApiKeyHeader = "x-api-key";
            ret.Auth.AdminApiKey = "admin";
            ret.Auth.CryptoApiKey = "user";

            #endregion

            #region Syslog

            ret.Syslog = new SettingsLogging();
            ret.Syslog.ConsoleLogging = 1;
            ret.Syslog.SyslogServerIp = "127.0.0.1";
            ret.Syslog.SyslogServerPort = 514;
            ret.Syslog.LogRequests = 0;
            ret.Syslog.LogResponses = 0;
            ret.Syslog.MinimumSeverityLevel = 1;

            #endregion
            
            #region REST

            ret.Rest = new SettingsRest();
            ret.Rest.AcceptInvalidCerts = 1;
            ret.Rest.UseWebProxy = 0;
            ret.Rest.WebProxyUrl = "";

            #endregion

            #region Overwrite-Existing-Config-Files

            #region System-Config

            if (Common.FileExists("System.json"))
            {
                Console.WriteLine("System configuration file already exists.");
                if (Common.InputBoolean("Do you wish to overwrite this file?", true))
                {
                    Common.DeleteFile("System.json");
                    if (!Common.WriteFile("System.json", Common.SerializeJson(ret), false))
                    {
                        Common.ExitApplication("Setup", "Unable to write System.json", -1);
                        return;
                    }
                }
            }
            else
            {
                if (!Common.WriteFile("System.json", Common.SerializeJson(ret), false))
                {
                    Common.ExitApplication("Setup", "Unable to write System.json", -1);
                    return;
                }
            }

            #endregion

            #endregion

            #region Finish

            Console.WriteLine("");
            Console.WriteLine("All finished!");
            Console.WriteLine("");
            Console.WriteLine("If you ever want to return to this setup wizard, just re-run the application");
            Console.WriteLine("from the terminal with the 'setup' argument.");
            Console.WriteLine("");
            Console.WriteLine("Press ENTER to start.");
            Console.WriteLine("");
            Console.ReadLine();

            #endregion
        }
  
        #endregion
    }
}
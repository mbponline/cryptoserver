using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SyslogLogging;
using WatsonWebserver;
using RestWrapper;
using Kvpbase.Classes;

namespace Kvpbase
{
    public partial class CryptoServer
    {
        public static Settings _Settings;
        public static LoggingModule _Logging;
        public static CryptoManager _Crypto;
        public static Server _Server;
        public static ConnectionManager _Connections;
        public static ConsoleManager _Console;

        static void Main(string[] args)
        {
            #region Process-Arguments

            if (args != null && args.Length > 0)
            {
                foreach (string curr in args)
                {
                    if (curr.Equals("setup"))
                    {
                        new Setup();
                    }
                }
            } 

            #endregion

            #region Load-Config-and-Initialize

            if (!Common.FileExists("System.json"))
            {
                Setup s = new Setup();
            }

            _Settings = Settings.FromFile("System.json");

            Welcome();

            #endregion

            #region Start-Modules

            _Logging = new LoggingModule(
                _Settings.Logging.SyslogServerIp,
                _Settings.Logging.SyslogServerPort,
                Common.IsTrue(_Settings.Logging.ConsoleLogging),
                (LoggingModule.Severity)(_Settings.Logging.MinimumSeverityLevel),
                false,
                true,
                true,
                false,
                true,
                false);

            _Crypto = new CryptoManager(_Settings, _Logging);

            _Server = new Server(
                _Settings.Server.DnsHostname,
                _Settings.Server.Port,
                Common.IsTrue(_Settings.Server.Ssl),
                RequestHandler);

            _Connections = new ConnectionManager(_Logging);

            if (Common.IsTrue(_Settings.EnableConsole)) _Console = new ConsoleManager(_Settings, _Connections, _Crypto, ExitApplication);

            #endregion

            #region Wait-for-Server-Thread

            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, Guid.NewGuid().ToString());
            bool waitHandleSignal = false;
            do
            {
                waitHandleSignal = waitHandle.WaitOne(1000);
            } while (!waitHandleSignal);

            _Logging.Log(LoggingModule.Severity.Debug, "CryptoServer exiting");

            #endregion 
        }

        static void Welcome()
        {
            // http://patorjk.com/software/taag/#p=display&f=Small&t=kvpbase

            string msg =
                Environment.NewLine +
                @"   _             _                    " + Environment.NewLine +
                @"  | |____ ___ __| |__  __ _ ___ ___   " + Environment.NewLine +
                @"  | / /\ V / '_ \ '_ \/ _` (_-</ -_)  " + Environment.NewLine +
                @"  |_\_\ \_/| .__/_.__/\__,_/__/\___|  " + Environment.NewLine +
                @"           |_|                        " + Environment.NewLine +
                @"                                      " + Environment.NewLine;

            Console.WriteLine(msg);
        }

        static HttpResponse RequestHandler(HttpRequest req)
        {
            DateTime startTime = DateTime.Now;
            HttpResponse resp = null; 
            bool connAdded = false;

            try
            {
                #region Unauthenticated-APIs

                switch (req.Method)
                {
                    case HttpMethod.GET:
                        if (WatsonCommon.UrlEqual(req.RawUrlWithoutQuery, "/loopback", false))
                        {
                            resp = new HttpResponse(req, true, 200, null, "application/json", "Hello from CryptoServer!", false);
                            return resp;
                        }
                        break;

                    case HttpMethod.PUT:
                    case HttpMethod.POST:
                    case HttpMethod.DELETE:
                    default:
                        break;
                }

                #endregion

                #region Add-to-Connection-List

                _Connections.Add(Thread.CurrentThread.ManagedThreadId, req);
                connAdded = true;

                #endregion

                #region APIs

                if (!String.IsNullOrEmpty(req.RetrieveHeaderValue(_Settings.Auth.ApiKeyHeader)))
                {
                    if (req.RetrieveHeaderValue(_Settings.Auth.ApiKeyHeader).Equals(_Settings.Auth.AdminApiKey))
                    {
                        #region Admin-API-Key

                        _Logging.Log(LoggingModule.Severity.Info, "RequestHandler use of admin API key detected for: " + req.RawUrlWithoutQuery);

                        switch (req.Method)
                        {
                            case HttpMethod.GET:
                                if (WatsonCommon.UrlEqual(req.RawUrlWithoutQuery, "/_cryptoserver/connections", false))
                                {
                                    resp = new HttpResponse(req, true, 200, null, "application/json", _Connections.GetActiveConnections(), false);
                                    return resp;
                                }

                                if (WatsonCommon.UrlEqual(req.RawUrlWithoutQuery, "/_cryptoserver/config", false))
                                {
                                    resp = new HttpResponse(req, true, 200, null, "application/json", _Settings, false);
                                    return resp;
                                }
                                 
                                break;

                            case HttpMethod.PUT:
                            case HttpMethod.POST:
                            case HttpMethod.DELETE:
                            default:
                                break;
                        }

                        _Logging.Log(LoggingModule.Severity.Warn, "RequestHandler unknown admin API endpoint: " + req.RawUrlWithoutQuery);
                        resp = new HttpResponse(req, false, 400, null, "application/json", "Unknown API endpoint or verb", false);
                        return resp;

                        #endregion
                    }
                    else if (req.RetrieveHeaderValue(_Settings.Auth.ApiKeyHeader).Equals(_Settings.Auth.CryptoApiKey))
                    {
                        #region Crypto-API-Key

                        string failureReason;
                        byte[] responseData;
                        Obj responseObj;

                        switch (req.Method)
                        {
                            case HttpMethod.POST:
                                if (WatsonCommon.UrlEqual(req.RawUrlWithoutQuery, "/decrypt", false))
                                {
                                    Obj reqObj = Common.DeserializeJson<Obj>(req.Data);
                                    if (!_Crypto.Decrypt(reqObj, out responseData, out failureReason))
                                    {
                                        _Logging.Log(LoggingModule.Severity.Warn, "RequestHandler unable to decrypt: " + failureReason);
                                        resp = new HttpResponse(req, false, 500, null, "application/json", failureReason, false);
                                        return resp;
                                    }
                                    else
                                    {
                                        _Logging.Log(LoggingModule.Severity.Debug, "RequestHandler encrypt returning " + responseData.Length + " bytes");
                                        resp = new HttpResponse(req, true, 200, null, "application/octet-stream", responseData, true);
                                        return resp;
                                    }
                                }

                                if (WatsonCommon.UrlEqual(req.RawUrlWithoutQuery, "/encrypt", false))
                                {
                                    if (!_Crypto.Encrypt(req.Data, out responseObj, out failureReason))
                                    {
                                        _Logging.Log(LoggingModule.Severity.Warn, "RequestHandler unable to encrypt: " + failureReason);
                                        resp = new HttpResponse(req, false, 500, null, "application/json", failureReason, false);
                                        return resp;
                                    }
                                    else
                                    {
                                        resp = new HttpResponse(req, true, 200, null, "application/json", Common.SerializeJson(responseObj), true);
                                        return resp;
                                    }
                                }

                                break;

                            case HttpMethod.GET: 
                            case HttpMethod.PUT:
                            case HttpMethod.DELETE:
                            default:
                                break;
                        }

                        _Logging.Log(LoggingModule.Severity.Warn, "RequestHandler unknown crypto API endpoint: " + req.RawUrlWithoutQuery);
                        resp = new HttpResponse(req, false, 400, null, "application/json", "Unknown API endpoint or verb", false);
                        return resp;

                        #endregion
                    }
                    else
                    {
                        #region Invalid-Auth-Material

                        _Logging.Log(LoggingModule.Severity.Warn, "RequestHandler invalid API key supplied: " + req.RetrieveHeaderValue(_Settings.Auth.ApiKeyHeader));
                        resp = new HttpResponse(req, false, 401, null, "application/json", "Invalid API key", false);
                        return resp;

                        #endregion
                    }
                }

                resp = new HttpResponse(req, false, 401, null, "application/json", "No authentication material", false);
                return resp;

                #endregion 
            }
            catch (Exception e)
            {
                _Logging.LogException("CryptoServer", "RequestHandler", e);
                resp = new HttpResponse(req, false, 500, null, "application/json", "Internal server error", false);
                return resp;
            }
            finally
            {
                if (resp != null)
                {
                    string message = "RequestHandler " + req.SourceIp + ":" + req.SourcePort + " " + req.Method + " " + req.RawUrlWithoutQuery;
                    message += " " + resp.StatusCode + " " + Common.TotalMsFrom(startTime) + "ms";
                    _Logging.Log(LoggingModule.Severity.Debug, message);
                }

                if (connAdded)
                {
                    _Connections.Close(Thread.CurrentThread.ManagedThreadId);
                }
            }
        }
         
        static bool ExitApplication()
        {
            _Logging.Log(LoggingModule.Severity.Info, "CryptoServer exiting due to console request");
            Environment.Exit(0);
            return true;
        }
    }
}

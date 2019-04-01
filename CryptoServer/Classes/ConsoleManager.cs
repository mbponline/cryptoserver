using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyslogLogging;
using RestWrapper;

namespace Kvpbase.Classes
{
    /// <summary>
    /// Console manager.
    /// </summary>
    public class ConsoleManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private bool _Enabled { get; set; }
        private Settings _Settings { get; set; }
        private ConnectionManager _ConnMgr { get; set; }
        private CryptoManager _CryptoMgr { get; set; }
        private Func<bool> _ExitAppDelegate;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate the object.
        /// </summary>
        /// <param name="settings">Server settings.</param>
        /// <param name="conn">Connection manager.</param>
        /// <param name="crypto">Crypto manager.</param>
        /// <param name="exitApplication">Function to call when exiting the server.</param>
        public ConsoleManager(
            Settings settings,
            ConnectionManager conn, 
            CryptoManager crypto,
            Func<bool> exitApplication)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (conn == null) throw new ArgumentNullException(nameof(conn));
            if (crypto == null) throw new ArgumentNullException(nameof(crypto));
            if (exitApplication == null) throw new ArgumentNullException(nameof(exitApplication));

            _Enabled = true;
            _Settings = settings;
            _ConnMgr = conn;
            _CryptoMgr = crypto;
            _ExitAppDelegate = exitApplication;

            Task.Run(() => ConsoleWorker());
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Stop the server.
        /// </summary>
        public void Stop()
        {
            _Enabled = false;
            return;
        }

        #endregion

        #region Private-Methods

        private void ConsoleWorker()
        {
            string userInput = "";
            while (_Enabled)
            {
                Console.Write("Command (? for help) > ");
                userInput = Console.ReadLine();

                if (userInput == null) continue;
                switch (userInput.ToLower().Trim())
                {
                    case "?":
                        Menu();
                        break;

                    case "c":
                    case "cls":
                    case "clear":
                        Console.Clear();
                        break;

                    case "q":
                    case "quit":
                        _Enabled = false;
                        _ExitAppDelegate();
                        break;
                         
                    case "list_connections":
                        ListConnections();
                        break;
                        
                    default:
                        Console.WriteLine("Unknown command.  '?' for help.");
                        break;
                }
            }
        }

        private void Menu()
        {
            Console.WriteLine(Common.Line(79, "-"));
            Console.WriteLine("  ?                         help / this menu");
            Console.WriteLine("  cls / c                   clear the console");
            Console.WriteLine("  quit / q                  exit the application");
            Console.WriteLine("  list_connections          list active connections");
            Console.WriteLine("");
            return;
        }
          
        private void ListConnections()
        {
            List<Connection> conns = _ConnMgr.GetActiveConnections();
            
            if (conns != null && conns.Count > 0)
            {
                Console.WriteLine(conns.Count + " Connections");
                foreach (Connection currConn in conns)
                {
                    Console.WriteLine("  " + currConn.SourceIp + ":" + currConn.SourcePort +
                        " " + currConn.Method + " " + currConn.RawUrl +
                        " " + currConn.HttpHostName + " " + currConn.NodeName);
                }
            }
            else
            {
                Console.WriteLine("(null)");
            }

            Console.WriteLine("");
        }

        #endregion 
    }
}

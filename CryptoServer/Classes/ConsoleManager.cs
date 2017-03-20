using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SyslogLogging;
using RestWrapper;

namespace Kvpbase
{
    public class ConsoleManager
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private bool Enabled { get; set; }
        private Settings CurrentSettings { get; set; }
        private ConnectionManager Connections { get; set; }
        private CryptoManager Crypto { get; set; }
        private Func<bool> ExitApplicationDelegate;

        #endregion

        #region Constructors-and-Factories

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

            Enabled = true;
            CurrentSettings = settings;
            Connections = conn;
            Crypto = crypto;
            ExitApplicationDelegate = exitApplication;

            Task.Run(() => ConsoleWorker());
        }

        #endregion

        #region Public-Methods

        public void Stop()
        {
            Enabled = false;
            return;
        }

        #endregion

        #region Private-Methods

        private void ConsoleWorker()
        {
            string userInput = "";
            while (Enabled)
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
                        Enabled = false;
                        ExitApplicationDelegate();
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
            List<Connection> conns = Connections.GetActiveConnections();
            
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

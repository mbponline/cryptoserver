using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kvpbase
{ 
    public class Obj
    {
        public byte[] Clear { get; set; }
        public byte[] Cipher { get; set; }
        public string Passphrase { get; set; }
        public string Salt { get; set; }
        public string InitVector { get; set; }
        public string Ksn { get; set; }
        public string SessionKey { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal TotalTimeMs { get; set; }
    } 
}

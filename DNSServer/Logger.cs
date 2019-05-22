using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DNSServer
{
    public class Logger
    {
        private string path;
        private StringBuilder builder;

        public Logger(string logPath)
        {
            builder = new StringBuilder();
            path = logPath;
            File.Delete(path);
        }

        public Logger(string logPath, string title) : this(logPath)
        {
            WriteLog(title);
        }

        public Logger(string logPath, params Object[] startParams) : this(logPath)
        {
            var parString = "";
            foreach (var p in startParams) parString += p.ToString();
            WriteLog(parString);
        }

        public void WriteLog(string log)
        {
            builder.Append(DateTime.Now.ToString());
            builder.Append(": ");
            builder.Append(log);
            builder.Append("\r\n");
        }

        public void Flush()
        {
            File.AppendAllText(path, builder.ToString());
        }
    }
}

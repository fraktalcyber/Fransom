using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Fransom
{
    class Logger
    {
        private static bool _logtofile = false;
        private static string _filename = "";

        public static void LogToFile(bool v)
        {
            _logtofile = v;
            if (v)
            {
                _filename = System.IO.Directory.GetCurrentDirectory() + "/Fransom_Log_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
                Logger.WriteLine("Logging to file: " + _filename);
            }
            else
            {
                Logger.WriteLine("Logging to file disabled");
            }
        }

        public static void WriteLine(string msg, bool sensitive = false)
        {
            Console.WriteLine(msg);
            if (_logtofile)
            {
                if (sensitive) msg = msg.Substring(0, 20) + " [...]";
                DateTime now = DateTime.Now;
                FileStream fs = new FileStream(_filename, FileMode.Append);
                TextWriter tmp = Console.Out;
                StreamWriter sw = new StreamWriter(fs);
                Console.SetOut(sw);
                Console.WriteLine("[ " + now + " ]: " + msg);
                Console.SetOut(tmp);
                sw.Close();
            }
        }

        public static void Write(string msg)
        {
            Console.Write(msg);
        }
    }
}

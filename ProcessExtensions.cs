using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.CompilerServices
{
	[AttributeUsage(AttributeTargets.Method)]
	public sealed class ExtensionAttribute : Attribute
	{
		public ExtensionAttribute() { }
	}
}

namespace InstantBackgroundUploader
{
	public static class ProcessExtensions
	{
		[DllImport("KERNEL32.dll")] //[DllImport("toolhelp.dll")]
		public static extern int CreateToolhelp32Snapshot(uint flags, uint processid);
		[DllImport("KERNEL32.DLL")] //[DllImport("toolhelp.dll")] 
		public static extern int CloseHandle(int handle);
		[DllImport("KERNEL32.DLL")] //[DllImport("toolhelp.dll")
		public static extern int Process32Next(int handle, ref ProcessEntry32 pe);

		[StructLayout(LayoutKind.Sequential)]
		public struct ProcessEntry32
		{
			public uint dwSize;
			public uint cntUsage;
			public uint th32ProcessID;
			public IntPtr th32DefaultHeapID;
			public uint th32ModuleID;
			public uint cntThreads;
			public uint th32ParentProcessID;
			public int pcPriClassBase;
			public uint dwFlags;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string szExeFile;
		};

		public static Process Parent(this Process process)
		{
			int SnapShot = CreateToolhelp32Snapshot(0x00000002, 0); //2 = SNAPSHOT of all procs
			try
			{
				ProcessEntry32 pe32 = new ProcessEntry32();
				pe32.dwSize = 296;
				int procid = process.Id;
				while (Process32Next(SnapShot, ref pe32) != 0)
				{
					string xname = pe32.szExeFile.ToString();
					if (procid == pe32.th32ProcessID)
					{
						return System.Diagnostics.Process.GetProcessById(Convert.ToInt32(pe32.th32ParentProcessID));
					}
				}
			}
			catch (Exception ex)
			{
				throw new Exception(System.Reflection.MethodBase.GetCurrentMethod() + " failed! [Type:" + ex.GetType().ToString() + ", Msg:" + ex.Message + "]");
			}
			finally
			{
				CloseHandle(SnapShot);
			}
			return null;
		}
	}

	/*public static class ProcessExtensions
	{
		private static string FindIndexedProcessName(int pid)
		{
			var processName = Process.GetProcessById(pid).ProcessName;
			var processesByName = Process.GetProcessesByName(processName);
			string processIndexdName = null;

			for (var index = 0; index < processesByName.Length; index++)
			{
				processIndexdName = index == 0 ? processName : processName + "#" + index;
				var processId = new PerformanceCounter("Process", "ID Process", processIndexdName);
				if (((int)processId.NextValue()).Equals(pid))
				{
					return processIndexdName;
				}
			}

			return processIndexdName;
		}

		private static Process FindPidFromIndexedProcessName(string indexedProcessName)
		{
			var parentId = new PerformanceCounter("Process", "Creating Process ID", indexedProcessName);
			return Process.GetProcessById((int)parentId.NextValue());
		}

		public static Process Parent(this Process process)
		{
			return FindPidFromIndexedProcessName(FindIndexedProcessName(process.Id));
		}
	}*/
}

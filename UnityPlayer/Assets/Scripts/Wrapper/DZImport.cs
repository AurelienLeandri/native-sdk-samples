using System;
using System.IO;
using System.Runtime.InteropServices;


public static class DZImport {
#if UNITY_STANDALONE_WIN
	public const string LibPath = "..\\NativeSDK\\Bins\\Platforms\\Windows\\DLLs\\libdeezer.x64.dll";
#elif UNITY_STANDALONE_OSX
	public const string LibPath = "../NativeSDK/Bins/Platforms/MacOSX/libdeezer.framework/Versions/Current/libdeezer";
#elif UNITY_STANDALONE_LINUX
	public const string LibPath = "../NativeSDK/Bins/Platforms/Linux/x86_64/libdeezer.so";
#else
	#error "Architecture not detected"
#endif

	private enum Platform
	{
		Windows,
		Linux,
		Mac
	}

	private static Platform RunningPlatform()
	{
		switch (Environment.OSVersion.Platform)
		{
		case PlatformID.Unix:
			// Well, there are chances MacOSX is reported as Unix instead of MacOSX.
			// Instead of platform check, we'll do a feature checks (Mac specific root folders)
			if (Directory.Exists("/Applications")
				& Directory.Exists("/System")
				& Directory.Exists("/Users")
				& Directory.Exists("/Volumes"))
				return Platform.Mac;
			else
				return Platform.Linux;

		case PlatformID.MacOSX:
			return Platform.Mac;

		default:
			return Platform.Windows;
		}
	}

	[DllImport("kernel32.dll")]
	private static extern IntPtr LoadLibrary(string dllToLoad);
}

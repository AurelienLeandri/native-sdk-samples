using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;

public static class DZConnection {
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	private struct dz_connect_configuration
	{
		[MarshalAs(UnmanagedType.LPStr)]
		public string app_id;

		[MarshalAs(UnmanagedType.LPStr)]
		public string product_id;

		[MarshalAs(UnmanagedType.LPStr)]
		public string product_build_id;

		[MarshalAs(UnmanagedType.LPStr)]
		public string user_profile_path;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public dz_connect_onevent_cb connect_event_cb;

		[MarshalAs(UnmanagedType.LPStr)]
		public string anonymous_blob;

		[MarshalAs(UnmanagedType.FunctionPtr)]
		public dz_connect_crash_reporting_delegate app_has_crashed_delegate;
	};

	// COMMON

	private delegate void dz_activity_operation_callback(IntPtr d, IntPtr data, int status, IntPtr result);

	[DllImport("libdeezer")] private static extern void dz_object_release(IntPtr objectHandle);

	// CONNECT

	private const int DZ_CONNECT_EVENT_UNKNOWN = 0;
	private const int DZ_CONNECT_EVENT_USER_OFFLINE_AVAILABLE = 1;
	private const int DZ_CONNECT_EVENT_USER_ACCESS_TOKEN_OK = 2;
	private const int DZ_CONNECT_EVENT_USER_ACCESS_TOKEN_FAILED = 3;
	private const int DZ_CONNECT_EVENT_USER_LOGIN_OK = 4;
	private const int DZ_CONNECT_EVENT_USER_LOGIN_FAIL_NETWORK_ERROR = 5;
	private const int DZ_CONNECT_EVENT_USER_LOGIN_FAIL_BAD_CREDENTIALS = 6;
	private const int DZ_CONNECT_EVENT_USER_LOGIN_FAIL_USER_INFO = 7;
	private const int DZ_CONNECT_EVENT_USER_LOGIN_FAIL_OFFLINE_MODE = 8;
	private const int DZ_CONNECT_EVENT_USER_NEW_OPTIONS = 9;
	private const int DZ_CONNECT_EVENT_ADVERTISEMENT_START = 10;
	private const int DZ_CONNECT_EVENT_ADVERTISEMENT_STOP = 11;

	private delegate void dz_connect_onevent_cb(IntPtr connectHandle, IntPtr eventHandle, IntPtr data);
	private delegate bool dz_connect_crash_reporting_delegate();

	[DllImport("libdeezer")] private static extern IntPtr dz_connect_new(ref dz_connect_configuration config);
	[DllImport("libdeezer")] private static extern IntPtr dz_connect_get_device_id(IntPtr self);
	[DllImport("libdeezer")] private static extern int dz_connect_activate(IntPtr self, IntPtr userdata);
	[DllImport("libdeezer")] private static extern int dz_connect_cache_path_set(IntPtr self, dz_activity_operation_callback cb, IntPtr data, [MarshalAs(UnmanagedType.LPStr)]string local_path);
	[DllImport("libdeezer")] private static extern int dz_connect_set_access_token(IntPtr self, dz_activity_operation_callback cb, IntPtr data, [MarshalAs(UnmanagedType.LPStr)]string token);
	[DllImport("libdeezer")] private static extern int dz_connect_offline_mode(IntPtr self, dz_activity_operation_callback cb, IntPtr data, bool offline_mode_forced);
	[DllImport("libdeezer")] private static extern int dz_connect_event_get_type(IntPtr eventHandle);
	[DllImport("libdeezer")] private static extern int dz_connect_deactivate(IntPtr connectHandle, dz_activity_operation_callback cb, IntPtr data);
}

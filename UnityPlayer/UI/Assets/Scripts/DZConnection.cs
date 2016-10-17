using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
public struct dz_connect_configuration
{
	public dz_connect_configuration(string app_id, string product_id, string product_build_id,
		string user_profile_path, string connect_event_cb, string anonymous_blob, string app_has_crashed_delegate)
	{
		this.app_id = app_id;
		this.product_id = product_id;
		this.product_build_id = product_build_id;
		this.user_profile_path = user_profile_path;
		this.connect_event_cb = connect_event_cb;
		this.anonymous_blob = anonymous_blob;
		this.app_has_crashed_delegate = app_has_crashed_delegate;
	}

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

enum dz_connect_event_t {
	DZ_CONNECT_EVENT_UNKNOWN,
	DZ_CONNECT_EVENT_USER_OFFLINE_AVAILABLE,
	DZ_CONNECT_EVENT_USER_ACCESS_TOKEN_OK,
	DZ_CONNECT_EVENT_USER_ACCESS_TOKEN_FAILED,
	DZ_CONNECT_EVENT_USER_LOGIN_OK,
	DZ_CONNECT_EVENT_USER_LOGIN_FAIL_NETWORK_ERROR,
	DZ_CONNECT_EVENT_USER_LOGIN_FAIL_BAD_CREDENTIALS,
	DZ_CONNECT_EVENT_USER_LOGIN_FAIL_USER_INFO,
	DZ_CONNECT_EVENT_USER_LOGIN_FAIL_OFFLINE_MODE,
	DZ_CONNECT_EVENT_USER_NEW_OPTIONS,
	DZ_CONNECT_EVENT_ADVERTISEMENT_START,
	DZ_CONNECT_EVENT_ADVERTISEMENT_STOP
};

delegate void dz_activity_operation_callback(IntPtr d, IntPtr data, int status, IntPtr result);
delegate void dz_connect_onevent_cb(IntPtr connectHandle, IntPtr eventHandle, IntPtr data);
delegate bool dz_connect_crash_reporting_delegate();

[Serializable()]
public class ConnectionInitFailedException : System.Exception
{
	public ConnectionInitFailedException() : base() { }
	public ConnectionInitFailedException(string message) : base(message) { }
	public ConnectionInitFailedException(string message, System.Exception inner) : base(message, inner) { }
	protected ConnectionInitFailedException(System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) { }
}

[Serializable()]
public class ConnectionRequestFailedException : System.Exception
{
	public ConnectionRequestFailedException() : base() { }
	public ConnectionRequestFailedException(string message) : base(message) { }
	public ConnectionRequestFailedException(string message, System.Exception inner) : base(message, inner) { }
	protected ConnectionRequestFailedException(System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) { }
}

public class DZConnection {
	public DZConnection(dz_connect_configuration config, IntPtr context) {
		Handle = dz_connect_new (ref config);
		if (!Handle)
			throw new ConnectionInitFailedException ("Connection handle failed to initialize. Check connection info you gave");
		if (dz_connect_activate(Handle, context))
			throw new ConnectionRequestFailedException ("Connection failed to activate.");
		active = true;
	}

	public int GetDeviceId() {
		return dz_connect_get_device_id (Handle);
	}

	public void DebugLogDisable() {
		// TODO: dz_connect_debug_log_disable
	}

	public void CachePathSet(string path, dz_activity_operation_callback cb = null, IntPtr operationUserdata = IntPtr.Zero) {
		if (dz_connect_cache_path_set (path, cb, operationUserdata))
			throw new ConnectionRequestFailedException ("Cache path was not set. Check connection.");
	}

	public void SetAccessToken(string token, dz_activity_operation_callback cb = null, IntPtr operationUserData = IntPtr.Zero) {
		if (dz_connect_set_access_token(Handle, token, cb, operationUserData))
			throw new ConnectionRequestFailedException ("Could not set access token. Check connection and that the token is valid.");
	}

	public void SetOfflineMode(bool offlineModeForced, dz_activity_operation_callback cb = null, IntPtr operationUserData = IntPtr.Zero) {
		if (dz_connect_offline_mode(offlineModeForced, cb, operationUserData))
			throw new ConnectionRequestFailedException ("Failed to set offline mode.");
	}

	public void shutdown(dz_activity_operation_callback cb = null, IntPtr operationUserData = IntPtr.Zero) {
		if (Handle) {
			dz_connect_deactivate (Handle, cb, operationUserData);
			active = false;
		}
	}

	public bool Active { get; set; } = false;
	public IntPtr Handle { get; private set; } = IntPtr.Zero;

	[DllImport("libdeezer")] public static extern void dz_object_release(IntPtr objectHandle);
	[DllImport("libdeezer")] public static extern IntPtr dz_connect_new(ref dz_connect_configuration config);
	[DllImport("libdeezer")] public static extern IntPtr dz_connect_get_device_id(IntPtr self);
	[DllImport("libdeezer")] public static extern int dz_connect_activate(IntPtr self, IntPtr userdata);
	[DllImport("libdeezer")] public static extern int dz_connect_cache_path_set(IntPtr self, dz_activity_operation_callback cb, IntPtr data, [MarshalAs(UnmanagedType.LPStr)]string local_path);
	[DllImport("libdeezer")] public static extern int dz_connect_set_access_token(IntPtr self, dz_activity_operation_callback cb, IntPtr data, [MarshalAs(UnmanagedType.LPStr)]string token);
	[DllImport("libdeezer")] public static extern int dz_connect_offline_mode(IntPtr self, dz_activity_operation_callback cb, IntPtr data, bool offline_mode_forced);
	[DllImport("libdeezer")] public static extern int dz_connect_event_get_type(IntPtr eventHandle);
	[DllImport("libdeezer")] public static extern int dz_connect_deactivate(IntPtr connectHandle, dz_activity_operation_callback cb, IntPtr data);
}

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;

public enum DZPlayerEvent {
	UNKNOWN,
	LIMITATION_FORCED_PAUSE,
	QUEUELIST_LOADED,
	QUEUELIST_NO_RIGHT,
	QUEUELIST_TRACK_NOT_AVAILABLE_OFFLINE,
	QUEUELIST_TRACK_RIGHTS_AFTER_AUDIOADS,
	QUEUELIST_SKIP_NO_RIGHT,
	QUEUELIST_TRACK_SELECTED,
	QUEUELIST_NEED_NATURAL_NEXT,
	MEDIASTREAM_DATA_READY,
	MEDIASTREAM_DATA_READY_AFTER_SEEK,
	RENDER_TRACK_START_FAILURE,
	RENDER_TRACK_START,
	RENDER_TRACK_END,
	RENDER_TRACK_PAUSED,
	RENDER_TRACK_SEEKING,
	RENDER_TRACK_UNDERFLOW,
	RENDER_TRACK_RESUMED,
	RENDER_TRACK_REMOVED
};

public enum DZPlayerCommand {
	UNKNOWN,
	START_TRACKLIST,
	JUMP_IN_TRACKLIST,
	NEXT,
	PREV,
	DISLIKE,
	NATURAL_END,
	RESUMED_AFTER_ADS,
};

public enum DZPlayerRepeatMode {
	OFF,
	ON,
	ALL
};

public static class DZPlayerIndex32 {
	public static readonly Int32 INVALID = Int32.MaxValue;
	public static readonly Int32 PREVIOUS = Int32.MaxValue - 1;
	public static readonly Int32 CURRENT = Int32.MaxValue - 2;
	public static readonly Int32 NEXT = Int32.MaxValue - 3;
};

public static class DZPlayerIndex64 {
	public static readonly Int64 INVALID = Int64.MaxValue;
	public static readonly Int64 PREVIOUS = Int64.MaxValue - 1;
	public static readonly Int64 CURRENT = Int64.MaxValue - 2;
	public static readonly Int64 NEXT = Int64.MaxValue - 3;
};

public delegate void dz_player_onevent_cb(IntPtr playerHandle, IntPtr eventHandle, IntPtr data);
public delegate void dz_player_onindexprogress_cb(IntPtr playerHandle, uint progress, IntPtr data);

[Serializable()]
public class PlayerInitFailedException : System.Exception
{
	public PlayerInitFailedException() : base() { }
	public PlayerInitFailedException(string message) : base(message) { }
	public PlayerInitFailedException(string message, System.Exception inner) : base(message, inner) { }
	protected PlayerInitFailedException(System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) { }
}

[Serializable()]
public class PlayerRequestFailedException : System.Exception
{
	public PlayerRequestFailedException() : base() { }
	public PlayerRequestFailedException(string message) : base(message) { }
	public PlayerRequestFailedException(string message, System.Exception inner) : base(message, inner) { }
	protected PlayerRequestFailedException(System.Runtime.Serialization.SerializationInfo info,
		System.Runtime.Serialization.StreamingContext context) { }
}

public class DZPlayer {
	public DZPlayer(IntPtr context, IntPtr connectionHandle) {
		Debug.Log ("Init player");
		Active = false;
		Handle = dz_player_new (connectionHandle);
		if (Handle.ToInt64() == 0)
			throw new PlayerInitFailedException ("Player failed to initialize. Check connection handle initialized properly");
		if (dz_player_activate (Handle, context) != 0)
			throw new PlayerRequestFailedException ("Unable to activate player. Check connection.");
		Active = true;
		Debug.Log ("Player initialized");
	}

	public void SetEventCallback(dz_player_onevent_cb cb) {
		Debug.Log ("Set event callback for player");
		if (dz_player_set_event_cb(Handle, cb) != 0)
			throw new PlayerRequestFailedException ("Unable to set event callback for player.");
		Debug.Log ("Event callback for player set");
	}

	public void Load(string content = null, dz_activity_operation_callback cb = null, IntPtr operationUserData = default(IntPtr)) {
		Debug.Log ("Load content");
		currentContent = content;
		if (dz_player_load(Handle, cb, operationUserData, currentContent) != 0)
			throw new PlayerRequestFailedException ("Unable to load content. Check the given dzmedia entry.");
		Debug.Log ("Content loaded");
	}

	public void Play(dz_activity_operation_callback cb = null, IntPtr operationUserData = default(IntPtr),
		DZPlayerCommand command=DZPlayerCommand.START_TRACKLIST,
		Int64 index = 0) {
		Debug.Log ("Play content");
		Debug.Log (command);
		Debug.Log (index);
		if (index == 0)
			index = Marshal.SizeOf (IntPtr.Zero) == 4 ? DZPlayerIndex32.CURRENT : DZPlayerIndex64.CURRENT;
		if (dz_player_play (Handle, cb, operationUserData, (int)command, (uint)index) > 1)
			throw new PlayerRequestFailedException ("Unable to play content.");
		Debug.Log ("Content playing");
	}

	public void Shutdown(dz_activity_operation_callback cb = null, IntPtr operationuserData = default(IntPtr)) {
		Debug.Log ("Shutdown player");
		if (Handle.ToInt64() != 0)
			dz_player_deactivate (Handle, cb, operationuserData);
		Debug.Log ("Player shut");
	}

	public void Stop(dz_activity_operation_callback cb = null, IntPtr operationUserData = default(IntPtr)) {
		Debug.Log ("Stop player");
		if (dz_player_stop(Handle, cb, operationUserData) != 0)
			throw new PlayerRequestFailedException ("Unable to stop current track.");
		Debug.Log ("Player stopped");
	}

	public void Pause(dz_activity_operation_callback cb = null, IntPtr operationUserData = default(IntPtr)) {
		Debug.Log ("Pause player");
		if (dz_player_pause(Handle, cb, operationUserData) != 0)
			throw new PlayerRequestFailedException ("Unable to pause current track.");
		Debug.Log ("Player paused");
	}

	public void Resume(dz_activity_operation_callback cb = null, IntPtr operationUserData = default(IntPtr)) {
		Debug.Log ("Resume player");
		if (dz_player_resume(Handle, cb, operationUserData) != 0)
			throw new PlayerRequestFailedException ("Unable to resume current track.");
		Debug.Log ("Player resumed");
	}

	public void UpdateRepeatMode(DZPlayerRepeatMode mode, dz_activity_operation_callback cb = null, IntPtr operationUserData = default(IntPtr)) {
		Debug.Log ("Update repeat mode");
		dz_player_set_repeat_mode (Handle, cb, operationUserData, (int)mode);
		Debug.Log ("Repeat mode updated");
	}

	public void EnableShuffleMode(bool shuffleMode, dz_activity_operation_callback cb = null, IntPtr operationUserData = default(IntPtr)) {
		Debug.Log ("Enable shuffle mode");
		dz_player_enable_shuffle_mode (Handle, cb, operationUserData, shuffleMode);
		Debug.Log ("Shuffle mode updated");
	}

	public void PlayAudioAds(dz_activity_operation_callback cb = null, IntPtr operationUserData = default(IntPtr)) {
		Debug.Log ("Play audio ad");
		if (dz_player_play_audioads(Handle, cb, operationUserData) != 0)
			throw new PlayerRequestFailedException ("Unable to load audio ads.");
		Debug.Log ("Audio ad playing");
	}

	public static DZPlayerEvent GetEventFromHandle(IntPtr handle) {
		return (DZPlayerEvent)dz_player_event_get_type (handle);
	}

	// TODO: Remove attributes from wrapper ??
	public bool Active { get; set; }
	public IntPtr Handle { get; set; }
	private string currentContent = "";
	private int nbTracksPlayed;

	[DllImport("libdeezer")] private static extern IntPtr dz_player_new(IntPtr self);
	[DllImport("libdeezer")] private static extern int dz_player_activate(IntPtr player, IntPtr supervisor);
	[DllImport("libdeezer")] private static extern int dz_player_deactivate(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data);
	[DllImport("libdeezer")] private static extern int dz_player_cache_next(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data, [MarshalAs(UnmanagedType.LPStr)]string trackUrl);
	[DllImport("libdeezer")] private static extern int dz_player_load(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data, [MarshalAs(UnmanagedType.LPStr)]string tracklistData);
	[DllImport("libdeezer")] private static extern int dz_player_pause(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data);
	[DllImport("libdeezer")] private static extern int dz_player_play(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data, int command, uint idx);
	[DllImport("libdeezer")] private static extern int dz_player_play_audioads(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data);
	[DllImport("libdeezer")] private static extern int dz_player_stop(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data);
	[DllImport("libdeezer")] private static extern int dz_player_resume(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data);
	[DllImport("libdeezer")] private static extern int dz_player_seek(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data, uint pos);
	[DllImport("libdeezer")] private static extern int dz_player_set_index_progress_cb(IntPtr player, dz_player_onindexprogress_cb cb, uint refreshTime);
	[DllImport("libdeezer")] private static extern int dz_player_set_event_cb(IntPtr player, dz_player_onevent_cb cb);
	[DllImport("libdeezer")] private static extern int dz_player_set_repeat_mode(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data, int mode);
	[DllImport("libdeezer")] private static extern int dz_player_enable_shuffle_mode(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data, bool shuffle_mode);
	[DllImport("libdeezer")] private static extern int dz_player_event_get_type(IntPtr eventHandle);
	[DllImport("libdeezer")] private static extern IntPtr dz_player_event_track_selected_dzapiinfo(IntPtr eventHandle);
	[DllImport("libdeezer")] private static extern IntPtr dz_player_event_track_selected_next_track_dzapiinfo(IntPtr eventHandle);
	[DllImport("libdeezer")] private static extern bool dz_player_event_track_selected_is_preview(IntPtr eventHandle);
}

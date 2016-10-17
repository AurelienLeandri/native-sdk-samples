using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;

// TODO: update enum values for 1.1

enum DZPlayerEvent {
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

enum DZPlayerCommand {
	UNKNOWN,
	START_TRACKLIST,
	JUMP_IN_TRACKLIST,
	NEXT,
	PREV,
	DISLIKE,
	NATURAL_END,
	RESUMED_AFTER_ADS,
};

enum DZPlayerRepeatMode { // TODO: Think it has changed since then. Refactor that.
	OFF,
	ON,
	ALL
};

enum DZPlayerIndex {
	INVALID = Marshal.SizeOf(IntPtr.Zero),
	PREVIOUS = Marshal.SizeOf(IntPtr.Zero) - 1,
	CURRENT = Marshal.SizeOf(IntPtr.Zero) - 2,
	NEXT = Marshal.SizeOf(IntPtr.Zero) - 3
};

delegate void dz_player_onevent_cb(IntPtr playerHandle, IntPtr eventHandle, IntPtr data);
delegate void dz_player_onindexprogress_cb(IntPtr playerHandle, uint progress, IntPtr data);

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

public static class DZPlayer {
	public DZPlayer(IntPtr context, IntPtr connectionHandle) {
		Handle = dz_player_new (connectionHandle);
		if (!Handle)
			throw new PlayerInitFailedException ("Player failed to initialize. Check connection handle initialized properly");
		if (dz_player_activate (Handle, context))
			throw new PlayerRequestFailedException ("Unable to activate player. Check connection.");
	}

	public void SetEventCallback(dz_connect_onevent_cb cb) {
		if (dz_player_set_event_cb(Handle, cb))
			throw new PlayerRequestFailedException ("Unable to set event callback for player.");
	}

	public void Load(string content, dz_activity_operation_callback cb = null, IntPtr operationUserData = IntPtr.Zero) {
		currentContent = content;
		if (dz_player_load(Handle, cb, operationUserData, currentContent))
			throw new PlayerRequestFailedException ("Unable to load content. Check the given dzmedia entry.");
	}

	public void Play(dz_activity_operation_callback cb = null, IntPtr operationUserData = IntPtr.Zero,
		DZPlayerCommand command=DZPlayerCommand.DZ_PLAYER_PLAY_CMD_START_TRACKLIST,
		DZPlayerIndex index = DZPlayerIndex.CURRENT) {
		if (dz_player_play(Handle, cb, operationUserData, command, index) > 1)
			throw new PlayerRequestFailedException ("Unable to play content.");
		IsStopped = true;
	}

	public void Shutdown(dz_activity_operation_callback cb = null, IntPtr operationuserData = IntPtr.Zero) {
		if (Handle)
			dz_player_deactivate (Handle, cb, operationuserData);
	}

	public void Stop(dz_activity_operation_callback cb = null, IntPtr operationUserData = IntPtr.Zero) {
		if (dz_player_stop(Handle, cb, operationUserData))
			throw new PlayerRequestFailedException ("Unable to stop current track.");
	}

	public void Pause(dz_activity_operation_callback cb = null, IntPtr operationUserData = IntPtr.Zero) {
		if (dz_player_pause(Handle, cb, operationUserData))
			throw new PlayerRequestFailedException ("Unable to pause current track.");
		IsStopped = false;
	}

	public void Resume(dz_activity_operation_callback cb = null, IntPtr operationUserData = IntPtr.Zero) {
		if (dz_player_resume(Handle, cb, operationUserData))
			throw new PlayerRequestFailedException ("Unable to resume current track.");
	}

	public void UpdateRepeatMode(DZPlayerRepeatMode mode, dz_activity_operation_callback cb = null, IntPtr operationUserData = IntPtr.Zero) {
		// TODO: dz_player_set_repeat_mode
		RepeatMode = mode;
	}

	public void EnableShuffleMode(bool shuffleMode, dz_activity_operation_callback cb = null, IntPtr operationUserData = IntPtr.Zero) {
		// TODO: dz_player_enable_shuffle_mode
		isShuffleMode = shuffleMode;
	}

	public void PlayAudioAds(dz_activity_operation_callback cb = null, IntPtr operationUserData = IntPtr.Zero) {
		if (dz_player_play_audioads(cb, operationUserData))
			throw new PlayerRequestFailedException ("Unable to load audio ads.");
	}

	public bool Active { get; set; } = false;
	public bool IsStopped = false;
	public bool IsPaused = false;
	public IntPtr Handle { get; private set; } = IntPtr.Zero;
	public DZPlayerRepeatMode RepeatMode { get; private set; } = 0;
	private string currentContent = "";
	public bool isShuffleMode { get; private set; } = false;
	private int nbTracksPlayed;

	[DllImport("libdeezer")] private static extern IntPtr dz_player_new(IntPtr self);
	[DllImport("libdeezer")] private static extern int dz_player_activate(IntPtr player, IntPtr supervisor);
	[DllImport("libdeezer")] private static extern int dz_player_deactivate(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data);
	[DllImport("libdeezer")] private static extern int dz_player_cache_next(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data, [MarshalAs(UnmanagedType.LPStr)]string trackUrl);
	[DllImport("libdeezer")] private static extern int dz_player_load(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data, [MarshalAs(UnmanagedType.LPStr)]string tracklistData);
	[DllImport("libdeezer")] private static extern int dz_player_pause(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data);
	[DllImport("libdeezer")] private static extern int dz_player_play(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data, int command, int mode, uint idx);
	[DllImport("libdeezer")] private static extern int dz_player_play_audioads(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data);
	[DllImport("libdeezer")] private static extern int dz_player_stop(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data);
	[DllImport("libdeezer")] private static extern int dz_player_resume(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data);
	[DllImport("libdeezer")] private static extern int dz_player_seek(IntPtr playerHandle, dz_activity_operation_callback cb, IntPtr data, uint pos);
	[DllImport("libdeezer")] private static extern int dz_player_set_index_progress_cb(IntPtr player, dz_player_onindexprogress_cb cb, uint refreshTime);
	[DllImport("libdeezer")] private static extern int dz_player_set_event_cb(IntPtr player, dz_player_onevent_cb cb);
	[DllImport("libdeezer")] private static extern int dz_player_event_get_type(IntPtr eventHandle);
	[DllImport("libdeezer")] private static extern IntPtr dz_player_event_track_selected_dzapiinfo(IntPtr eventHandle);
	[DllImport("libdeezer")] private static extern IntPtr dz_player_event_track_selected_next_track_dzapiinfo(IntPtr eventHandle);
	[DllImport("libdeezer")] private static extern bool dz_player_event_track_selected_is_preview(IntPtr eventHandle);
}

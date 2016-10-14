using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;

enum dz_player_event_t {
	DZ_PLAYER_EVENT_UNKNOWN = 0,
	DZ_PLAYER_EVENT_LIMITATION_FORCED_PAUSE = 1,
	DZ_PLAYER_EVENT_PLAYLIST_TRACK_NOT_AVAILABLE_OFFLINE = 2,
	DZ_PLAYER_EVENT_PLAYLIST_TRACK_NO_RIGHT = 3,
	DZ_PLAYER_EVENT_PLAYLIST_TRACK_RIGHTS_AFTER_AUDIOADS = 4,
	DZ_PLAYER_EVENT_PLAYLIST_SKIP_NO_RIGHT = 5,
	DZ_PLAYER_EVENT_PLAYLIST_TRACK_SELECTED = 6,
	DZ_PLAYER_EVENT_PLAYLIST_NEED_NATURAL_NEXT = 7,
	DZ_PLAYER_EVENT_MEDIASTREAM_DATA_READY = 8,
	DZ_PLAYER_EVENT_MEDIASTREAM_DATA_READY_AFTER_SEEK = 9,
	DZ_PLAYER_EVENT_RENDER_TRACK_START_FAILURE = 10,
	DZ_PLAYER_EVENT_RENDER_TRACK_START = 11,
	DZ_PLAYER_EVENT_RENDER_TRACK_END = 12,
	DZ_PLAYER_EVENT_RENDER_TRACK_PAUSED = 13,
	DZ_PLAYER_EVENT_RENDER_TRACK_SEEKING = 14,
	DZ_PLAYER_EVENT_RENDER_TRACK_UNDERFLOW = 15,
	DZ_PLAYER_EVENT_RENDER_TRACK_RESUMED = 16,
	DZ_PLAYER_EVENT_RENDER_TRACK_REMOVED = 17
};

enum dz_player_command_t {
	DZ_PLAYER_PLAY_CMD_UNKNOWN = 0,
	DZ_PLAYER_PLAY_CMD_START_TRACKLIST = 1,
	DZ_PLAYER_PLAY_CMD_JUMP_IN_TRACKLIST = 2,
	DZ_PLAYER_PLAY_CMD_NEXT = 3,
	DZ_PLAYER_PLAY_CMD_PREV = 4,
	DZ_PLAYER_PLAY_CMD_DISLIKE = 5,
	DZ_PLAYER_PLAY_CMD_NATURAL_END = 6,
	DZ_PLAYER_PLAY_CMD_RESUMED_AFTER_ADS = 7,
};

enum dz_player_mode_t { // TODO: Think it has changed since then. Refactor that.
	DZ_TRACKLIST_AUTOPLAY_MODE_UNKNOWN = 0,
	DZ_TRACKLIST_AUTOPLAY_MANUAL = 1,
	DZ_TRACKLIST_AUTOPLAY_MODE_ONE = 2,
	DZ_TRACKLIST_AUTOPLAY_MODE_ONE_REPEAT = 3,
	DZ_TRACKLIST_AUTOPLAY_MODE_NEXT = 4,
	DZ_TRACKLIST_AUTOPLAY_MODE_NEXT_REPEAT = 5,
	DZ_TRACKLIST_AUTOPLAY_MODE_RANDOM = 6,
	DZ_TRACKLIST_AUTOPLAY_MODE_RANDOM_REPEAT = 7
};

enum dz_player_index_t {
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

	public void Load(string content, dz_activity_operation_callback cb, IntPtr operationUserData) {
		currentContent = content;
		if (dz_player_load(Handle, cb, operationUserData, currentContent))
			throw new PlayerRequestFailedException ("Unable to load content. Check the given dzmedia entry.");
	}

	public void Play(dz_activity_operation_callback cb, IntPtr operationUserData,
		dz_player_command_t command=dz_player_command_t.DZ_PLAYER_PLAY_CMD_START_TRACKLIST,
		dz_player_index_t index = dz_player_index_t.CURRENT) {
		if (dz_player_play(Handle, cb, operationUserData, command, index) > 1)
			throw new PlayerRequestFailedException ("Unable to play content.");
	}

	public void Shutdown(dz_activity_operation_callback cb, IntPtr operationuserData) {
		if (Handle)
			dz_player_deactivate (Handle, cb, operationuserData);
	}

	public void Stop(dz_activity_operation_callback cb, IntPtr operationUserData) {
		if (dz_player_stop(Handle, cb, operationUserData))
			throw new PlayerRequestFailedException ("Unable to stop current track.");
	}

	public void Pause(dz_activity_operation_callback cb, IntPtr operationUserData) {
		if (dz_player_pause(Handle, cb, operationUserData))
			throw new PlayerRequestFailedException ("Unable to pause current track.");
	}

	public void Resume(dz_activity_operation_callback cb, IntPtr operationUserData) {
		if (dz_player_resume(Handle, cb, operationUserData))
			throw new PlayerRequestFailedException ("Unable to resume current track.");
	}

	public void SetRepeatMode(dz_player_mode_t, dz_activity_operation_callback cb, IntPtr operationUserData) {
		// TODO: dz_player_set_repeat_mode
	}

	public void EnableShuffleMode(bool shuffleMode, dz_activity_operation_callback cb, IntPtr operationUserData) {
		// TODO: dz_player_enable_shuffle_mode
		isShuffleMode = shuffleMode;
	}

	public void PlayAudioAds(dz_activity_operation_callback cb, IntPtr operationUserData) {
		if (dz_player_play_audioads(cb, operationUserData))
			throw new PlayerRequestFailedException ("Unable to load audio ads.");
	}

	private bool active = false;
	private bool playing = false;
	private IntPtr Handle
	{
		get { return Handle; }
		set { Handle = value; }
	} = IntPtr.Zero;
	private string currentContent = "";
	private bool isShuffleMode = false;
	private int nbTracksPlayed;
	private dz_player_mode_t repeatMode;

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

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;

public class MyDeezerApp : MonoBehaviour {
	public void Start() 
	{
		string userAccessToken = "";
		dz_connect_configuration config = new dz_connect_configuration ();
		this.debugMode = debugMode;
		this.config = config;
		Connection = new DZConnection (config);
		if (!debugMode)
			Connection.DebugLogDisable ();
		Player = new DZPlayer (ref this, Connection.Handle);
		Player.SetEventCallback (MyDeezerApp.PlayerOnEventCallback);
		Connection.CachePathSet(config.user_profile_path);
		Connection.SetAccessToken (userAccessToken);
		Connection.SetOfflineMode (false);
	}

	public void Shutdown() {
		if (Player.Handle)
			Player.Shutdown (MyDeezerApp.PlayerOnDeactivateCallback, (IntPtr)(&this));
		else if (Connection.Handle)
			Connection.shutdown (MyDeezerApp.ConnectionOnDeactivateCallback, (IntPtr)(&this));
	}

	public void StartStop() {
		if (Player.IsStopped)
			Player.Play ();
		else
			Player.Stop ();
	}

	public void PlayPause() {
		if (Player.IsPaused)
			Player.Resume ();
		else
			Player.Pause ();
	}

	public void Next() {
		Player.Play (command: dz_player_command_t.DZ_PLAYER_PLAY_CMD_START_TRACKLIST, index: dz_player_index_t.NEXT);
	}

	public void Previous() {
		Player.Play (command: dz_player_command_t.DZ_PLAYER_PLAY_CMD_START_TRACKLIST, index: dz_player_index_t.PREVIOUS);
	}

	public void ToggleRepeat() {
		Player.RepeatMode++;
		if (Player.RepeatMode > 0 /* TODO: repeat mode enum */)
			Player.UpdateRepeatMode(0); /* idem */
	}

	public void ToggleRandom() {
		Player.EnableShuffleMode(!Player.isShuffleMode);
	}

	public void LoadContent(string content) {
		Player.Load (content);
	}

	private bool debugMode = false;
	dz_connect_configuration config;
	private DZConnection Connection { get; private set; } = null;
	public DZPlayer Player { get; private set; } = null;

	public static int PlayerOnEventCallback(IntPtr handle, dz_player_event_t eventType, IntPtr userData) {
		// TODO: If it doesnt work check type of attribute eventType.
		DZPlayer selfPlayer = (DZPlayer)(*userData);
		if (true) // TODO: change event_t enum values
			selfPlayer.Play();
		if (eventType == dz_player_event_t.DZ_PLAYER_EVENT_PLAYLIST_TRACK_RIGHTS_AFTER_AUDIOADS)
			selfPlayer.PlayAudioAds ();
		return 0;
	}

	public static int ConnectionOnEventCallback(IntPtr handle, dz_player_event_t eventType, IntPtr userData) {
		MyDeezerApp app = (MyDeezerApp)(*userData);
		if (eventType = dz_connect_event_t.DZ_CONNECT_EVENT_USER_LOGIN_OK)
			app.Player.Load ();
		if (eventType == dz_connect_event_t.DZ_CONNECT_EVENT_USER_LOGIN_FAIL_USER_INFO)
			app.Shutdown ();
	}

	public static void PlayerOnDeactivateCallback(IntPtr delegateFunc, IntPtr operationUserData, int status, int result) {
		MyDeezerApp app = (MyDeezerApp)(*operationUserData);
		app.Player.Active = false;
		app.Player.Handle = IntPtr.Zero;
		if (app.Connection.Handle)
			app.Connection.shutdown (MyDeezerApp.ConnectionOnDeactivateCallback, operationUserData);
	}

	public static void ConnectionOnDeactivateCallback(IntPtr delegateFunc, IntPtr operationUserData, int status, int result) {
		MyDeezerApp app = (MyDeezerApp)(*operationUserData);
		if (app.Connection.Handle) {
			app.Connection.Active = false;
			app.Connection.Handle = IntPtr.Zero;
		}
	}
}

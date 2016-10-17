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
		connection = new DZConnection (config);
		if (!debugMode)
			connection.DebugLogDisable ();
		Player = new DZPlayer (ref this, connection.Handle);
		// TODO: player.SetEventCallback (myCallback);
		connection.CachePathSet(config.user_profile_path);
		connection.SetAccessToken (userAccessToken);
		connection.SetOfflineMode (false);
		// TODO: connection and player on_deactivate cbs
	}

	public void Shutdown() {
	}

	private bool debugMode = false;
	dz_connect_configuration config;
	private DZConnection connection = null;
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

	public static int PlayerOnDeactivateCallback(IntPtr delegateFunc, IntPtr operationUserData, int status, int result) {
		MyDeezerApp app = (MyDeezerApp)(*operationUserData);
		app.Player.Active = false;
		app.Player.Handle = IntPtr.Zero;
	}
}

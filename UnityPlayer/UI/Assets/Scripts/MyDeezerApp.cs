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
		player = new DZPlayer (ref this, connection.Handle);
		// TODO: player.SetEventCallback (myCallback);
		connection.CachePathSet(config.user_profile_path);
		connection.SetAccessToken (userAccessToken);
		connection.SetOfflineMode (false);
		// TODO: connection and player on_deactivate cbs
	}

	private bool debugMode = false;
	dz_connect_configuration config;
	private DZConnection connection = null;
	private DZPlayer player = null;

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
	}
}

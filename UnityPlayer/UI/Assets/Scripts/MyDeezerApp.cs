using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;

public class MyDeezerApp {
	public MyDeezerApp() 
	{
		Debug.Log ("Set app info");
		string userAccessToken = "fr49mph7tV4KY3ukISkFHQysRpdCEbzb958dB320pM15OpFsQs";
		string userApplicationid = "190262";
		string userApplicationName = "UnityPlayer";
		string userApplicationVersion = "00001";
		// TODO: system-wise cache path
			string userCachePath = "/var/tmp/dzrcache_NDK_SAMPLE";
		dz_connect_configuration config = new dz_connect_configuration (
			userApplicationid,
			userApplicationName,
			userApplicationVersion,
			userCachePath,
			MyDeezerApp.ConnectionOnEventCallback,
			IntPtr.Zero,
			null
		);
		this.debugMode = true;
		this.config = config;
		GCHandle selfHandle = GCHandle.Alloc (this);
		this.appPtr = GCHandle.ToIntPtr(selfHandle);
		Connection = new DZConnection (config, appPtr);
		if (!debugMode)
			Connection.DebugLogDisable ();
		Player = new DZPlayer (appPtr, Connection.Handle);
		Player.SetEventCallback (MyDeezerApp.PlayerOnEventCallback);
		Connection.CachePathSet(config.user_profile_path);
		Connection.SetAccessToken (userAccessToken);
		Connection.SetOfflineMode (false);
		Debug.Log ("App info set");
	}

	public void Shutdown() {
		Debug.Log ("Shutdown the app");
		if (Player.Handle.ToInt64() != 0)
			Player.Shutdown (MyDeezerApp.PlayerOnDeactivateCallback, appPtr);
		else if (Connection.Handle.ToInt64() != 0)
			Connection.shutdown (MyDeezerApp.ConnectionOnDeactivateCallback, appPtr);
		Debug.Log ("App shut");
	}

	public void Stop() {
		Player.Stop ();
		isPaused = false;
		isStopped = true;
	}

	public void PlayPause() {
		if (isStopped) {
			Player.Play ();
			isPaused = false;
			isStopped = false;
		} else if (isPaused) {
			Player.Resume ();
			isPaused = false;
			;
		} else {
			Player.Pause ();
			isPaused = true;
		}
	}

	public void Next() {
		isPaused = false;
		isStopped = false;
		Int64 index = Marshal.SizeOf (IntPtr.Zero) == 4 ? DZPlayerIndex32.NEXT : DZPlayerIndex64.NEXT;
		Player.Play (command: DZPlayerCommand.START_TRACKLIST, index: index);
	}

	public void Previous() {
		isPaused = false;
		isStopped = false;
		Int64 index = Marshal.SizeOf (IntPtr.Zero) == 4 ? DZPlayerIndex32.PREVIOUS : DZPlayerIndex64.PREVIOUS;
		Player.Play (command: DZPlayerCommand.START_TRACKLIST, index: index);
	}

	public void ToggleRepeat() {
		if (Player.RepeatMode + 1 > 0 /* TODO: repeat mode enum */)
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
	public DZConnection Connection { get; private set; }
	public DZPlayer Player { get; private set; }
	private IntPtr appPtr = IntPtr.Zero;
	private bool isPaused;
	private bool isStopped;

	public static void PlayerOnEventCallback(IntPtr handle, IntPtr eventHandle, IntPtr userData) {
		Debug.Log ("Entering PlayerOnEventCallback");
		GCHandle selfHandle = GCHandle.FromIntPtr(userData);
		MyDeezerApp app = (MyDeezerApp)selfHandle.Target;
		DZPlayerEvent playerEvent = DZPlayer.GetEventFromHandle (eventHandle);
		Debug.Log (playerEvent);
		if (playerEvent == DZPlayerEvent.QUEUELIST_LOADED)
			app.Player.Play();
		if (playerEvent == DZPlayerEvent.QUEUELIST_TRACK_RIGHTS_AFTER_AUDIOADS)
			app.Player.PlayAudioAds ();
		Debug.Log ("Exiting PlayerOnEventCallback");
	}

	public static void ConnectionOnEventCallback(IntPtr handle, IntPtr eventHandle, IntPtr userData) {
		Debug.Log ("Entering ConnectionOnEventCallback");
		GCHandle selfHandle = GCHandle.FromIntPtr(userData);
		MyDeezerApp app = (MyDeezerApp)(selfHandle.Target);
		DZConnectionEvent connectionEvent = DZConnection.GetEventFromHandle (eventHandle);
		if (connectionEvent == DZConnectionEvent.USER_LOGIN_OK)
			app.Player.Load ("dzmedia:///album/607845");
		if (connectionEvent == DZConnectionEvent.USER_LOGIN_FAIL_USER_INFO)
			app.Shutdown ();
		Debug.Log ("Exiting ConnectionOnEventCallback");
	}

	public static void PlayerOnDeactivateCallback(IntPtr delegateFunc, IntPtr operationUserData, int status, int result) {
		Debug.Log ("Entering PlayerOnDeactivateCallback");
		GCHandle selfHandle = GCHandle.FromIntPtr(operationUserData);
		MyDeezerApp app = (MyDeezerApp)(selfHandle.Target);
		app.Player.Active = false;
		app.Player.Handle = IntPtr.Zero;
		if (app.Connection.Handle.ToInt64() != 0)
			app.Connection.shutdown (MyDeezerApp.ConnectionOnDeactivateCallback, operationUserData);
		Debug.Log ("Exiting PlayerOnDeactivateCallback");
	}

	public static void ConnectionOnDeactivateCallback(IntPtr delegateFunc, IntPtr operationUserData, int status, int result) {
		Debug.Log ("Entering ConnectionOnDeactivateCallback");
		GCHandle selfHandle = GCHandle.FromIntPtr(operationUserData);
		MyDeezerApp app = (MyDeezerApp)(selfHandle.Target);
		if (app.Connection.Handle.ToInt64() != 0) {
			app.Connection.Active = false;
			app.Connection.Handle = IntPtr.Zero;
		}
		Debug.Log ("Exiting ConnectionOnDeactivateCallback");
	}
}

﻿using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;

public class MyDeezerApp : MonoBehaviour {
	public void Start() 
	{
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
	}

	public void Shutdown() {
		if (Player.Handle.ToInt64() != 0)
			Player.Shutdown (MyDeezerApp.PlayerOnDeactivateCallback, appPtr);
		else if (Connection.Handle.ToInt64() != 0)
			Connection.shutdown (MyDeezerApp.ConnectionOnDeactivateCallback, appPtr);
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
		Player.Play (command: DZPlayerCommand.START_TRACKLIST, index: DZPlayerIndex.NEXT);
	}

	public void Previous() {
		Player.Play (command: DZPlayerCommand.START_TRACKLIST, index: DZPlayerIndex.PREVIOUS);
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

	public static void PlayerOnEventCallback(IntPtr handle, IntPtr eventHandle, IntPtr userData) {
		// TODO: If it doesnt work check type of attribute eventType.
		GCHandle selfHandle = GCHandle.FromIntPtr(userData);
		DZPlayer selfPlayer = (DZPlayer)selfHandle.Target;
		DZPlayerEvent playerEvent = DZPlayer.GetEventFromHandle (eventHandle);
		if (true) // TODO: change event_t enum values
			selfPlayer.Play();
		if (playerEvent == DZPlayerEvent.QUEUELIST_TRACK_RIGHTS_AFTER_AUDIOADS)
			selfPlayer.PlayAudioAds ();
	}

	public static void ConnectionOnEventCallback(IntPtr handle, IntPtr eventHandle, IntPtr userData) {
		GCHandle selfHandle = GCHandle.FromIntPtr(userData);
		MyDeezerApp app = (MyDeezerApp)(selfHandle.Target);
		DZConnectionEvent connectionEvent = DZConnection.GetEventFromHandle (eventHandle);
		if (connectionEvent == DZConnectionEvent.USER_LOGIN_OK)
			app.Player.Load ();
		if (connectionEvent == DZConnectionEvent.USER_LOGIN_FAIL_USER_INFO)
			app.Shutdown ();
	}

	public static void PlayerOnDeactivateCallback(IntPtr delegateFunc, IntPtr operationUserData, int status, int result) {
		GCHandle selfHandle = GCHandle.FromIntPtr(operationUserData);
		MyDeezerApp app = (MyDeezerApp)(selfHandle.Target);
		app.Player.Active = false;
		app.Player.Handle = IntPtr.Zero;
		if (app.Connection.Handle.ToInt64() != 0)
			app.Connection.shutdown (MyDeezerApp.ConnectionOnDeactivateCallback, operationUserData);
	}

	public static void ConnectionOnDeactivateCallback(IntPtr delegateFunc, IntPtr operationUserData, int status, int result) {
		GCHandle selfHandle = GCHandle.FromIntPtr(operationUserData);
		MyDeezerApp app = (MyDeezerApp)(selfHandle.Target);
		if (app.Connection.Handle.ToInt64() != 0) {
			app.Connection.Active = false;
			app.Connection.Handle = IntPtr.Zero;
		}
	}
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using System.Threading;
using System.Collections;
using System.Runtime.InteropServices;

public struct ApplicationData {
	public ApplicationData(DZPlayer Player, DZConnection Connection, string DZMediaLink, string ContentLink) {
		this.Player = Player;
		this.Connection = Connection;
		this.DZMediaLink = DZMediaLink;
		this.ContentLink = ContentLink;
		this.SelfPtr = IntPtr.Zero;
		GCHandle selfHandle = GCHandle.Alloc (this);
		SelfPtr = GCHandle.ToIntPtr(selfHandle);
	}

	public DZPlayer Player;
	public DZConnection Connection;
	public string DZMediaLink;
	public string ContentLink;
	public IntPtr SelfPtr;
}

public class ApplicationMainScript : MonoBehaviour {

	public PlayerPanelScript PlayerPanel;
	public TrackListScript TrackListPanel;
	private string contentLink;
	private string dzmediaLink;
	private ApplicationData applicationData;

	void Awake() {
		//string contentLink = "track/10287076"; // FIXME: choose your content here
		//string contentLink = "album/607845"; // FIXME: choose your content here
		contentLink = "playlist/1363560485"; // FIXME: choose your content here
		dzmediaLink = "dzmedia:///" + contentLink;
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
			ConnectionOnEventCallback,
			IntPtr.Zero,
			null
		);
		this.debugMode = true;
		Connection = new DZConnection (config);
		if (!debugMode)
			Connection.DebugLogDisable ();
		Player = new DZPlayer (Connection.Handle);
		applicationData = new ApplicationData(Player, Connection, dzmediaLink, contentLink);
		Connection.Activate (applicationData.SelfPtr);
		Player.Activate(applicationData.SelfPtr);
		Player.SetEventCallback (PlayerOnEventCallback);
		Connection.CachePathSet(config.user_profile_path);
		Connection.SetAccessToken (userAccessToken);
		Connection.SetOfflineMode (false);
	}

	void Start () {
		TrackListPanel.LoadtrackList("https://api.deezer.com/" + contentLink);
	}

	void OnApplicationQuit() {
		Shutdown();
	}

	public void Shutdown() {
		if (Player.Handle.ToInt64() != 0)
			Player.Shutdown (PlayerOnDeactivateCallback, applicationData.SelfPtr);
		else if (Connection.Handle.ToInt64() != 0)
			Connection.shutdown (ConnectionOnDeactivateCallback, applicationData.SelfPtr);
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
		Player.Play (command: DZPlayerCommand.NEXT, index: index);
	}

	public void LoadIndex(int index) {
		isPaused = false;
		isStopped = false;
		Player.Play (command: DZPlayerCommand.JUMP_IN_TRACKLIST, index: index);
	}

	public void Previous() {
		isPaused = false;
		isStopped = false;
		Int64 index = Marshal.SizeOf (IntPtr.Zero) == 4 ? DZPlayerIndex32.PREVIOUS : DZPlayerIndex64.PREVIOUS;
		Player.Play (command: DZPlayerCommand.PREV, index: index);
	}

	public void ToggleRepeat() {
		if (RepeatMode == DZPlayerRepeatMode.OFF)
			RepeatMode = DZPlayerRepeatMode.ALL;
		else if (RepeatMode == DZPlayerRepeatMode.ON)
			RepeatMode = DZPlayerRepeatMode.OFF;
		else
			RepeatMode = DZPlayerRepeatMode.ON;
		Player.UpdateRepeatMode (RepeatMode);
	}

	public void ToggleRandom() {
		isShuffleMode = !isShuffleMode;
		Player.EnableShuffleMode(isShuffleMode);
	}

	public void LoadContent(string content) {
		Player.Load (content);
	}

	public void Seek(int seconds) {
		Player.Seek (seconds * 1000000);
	}

	public static void PlayerOnEventCallback(IntPtr handle, IntPtr eventHandle, IntPtr userData) {
		Debug.Log ("Entering PlayerOnEventCallback");
		GCHandle selfHandle = GCHandle.FromIntPtr(userData);
		ApplicationData app = (ApplicationData)selfHandle.Target;
		DZPlayerEvent playerEvent = DZPlayer.GetEventFromHandle (eventHandle);
		Debug.Log (playerEvent);
		if (playerEvent == DZPlayerEvent.QUEUELIST_LOADED)
			app.Player.Play ();
		if (playerEvent == DZPlayerEvent.QUEUELIST_TRACK_RIGHTS_AFTER_AUDIOADS)
			app.Player.PlayAudioAds ();
	}

	public static void ConnectionOnEventCallback(IntPtr handle, IntPtr eventHandle, IntPtr userData) {
		Debug.Log ("Entering ConnectionOnEventCallback");
		GCHandle selfHandle = GCHandle.FromIntPtr(userData);
		ApplicationData app = (ApplicationData)(selfHandle.Target);
		DZConnectionEvent connectionEvent = DZConnection.GetEventFromHandle (eventHandle);
		if (connectionEvent == DZConnectionEvent.USER_LOGIN_OK)
			app.Player.Load (app.DZMediaLink);
		if (connectionEvent == DZConnectionEvent.USER_LOGIN_FAIL_USER_INFO) {
			if (app.Player.Handle.ToInt64 () != 0)
				app.Player.Shutdown (PlayerOnDeactivateCallback, app.SelfPtr);
			else if (app.Connection.Handle.ToInt64 () != 0)
				app.Connection.shutdown (ConnectionOnDeactivateCallback, app.SelfPtr);
		}
		Debug.Log ("Exiting ConnectionOnEventCallback");
	}

	public static string getContentJson (string content) {
		UnityWebRequest www = UnityWebRequest.Get (content);
		www.Send ();
		while (!www.isDone) {}
		if (www.isError) {
			Debug.Log (www.error);
			www.Dispose ();
			return "error";
		}
		return www.downloadHandler.text;
	}

	public static void PlayerOnDeactivateCallback(IntPtr delegateFunc, IntPtr operationUserData, int status, int result) {
		Debug.Log ("Entering PlayerOnDeactivateCallback");
		GCHandle selfHandle = GCHandle.FromIntPtr(operationUserData);
		ApplicationMainScript app = (ApplicationMainScript)(selfHandle.Target);
		app.Player.Active = false;
		app.Player.Handle = IntPtr.Zero;
		if (app.Connection.Handle.ToInt64() != 0)
			app.Connection.shutdown (ApplicationMainScript.ConnectionOnDeactivateCallback, operationUserData);
		Debug.Log ("Exiting PlayerOnDeactivateCallback");
	}

	public static void ConnectionOnDeactivateCallback(IntPtr delegateFunc, IntPtr operationUserData, int status, int result) {
		Debug.Log ("Entering ConnectionOnDeactivateCallback");
		GCHandle selfHandle = GCHandle.FromIntPtr(operationUserData);
		ApplicationMainScript app = (ApplicationMainScript)(selfHandle.Target);
		if (app.Connection.Handle.ToInt64() != 0) {
			app.Connection.Active = false;
			app.Connection.Handle = IntPtr.Zero;
		}
		Debug.Log ("Exiting ConnectionOnDeactivateCallback");
	}

	private bool debugMode = false;
	public DZConnection Connection { get; private set; }
	public DZPlayer Player { get; private set; }
	private bool isPaused;
	private bool isStopped;
	public DZPlayerRepeatMode RepeatMode { get; private set; }
	public bool isShuffleMode { get; private set; }

}

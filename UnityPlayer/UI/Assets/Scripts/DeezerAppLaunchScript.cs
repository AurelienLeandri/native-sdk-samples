using UnityEngine;
using System.Collections;
using System;

public class DeezerAppLaunchScript : MonoBehaviour {

	public MyDeezerApp app;

	// Use this for initialization
	void Start () {
		app = new MyDeezerApp ();
	}

	void OnApplicationQuit() {
		Debug.Log ("Application quit");
		app.Shutdown();
	}

	public void StopButton() {
		app.StartStop ();
	}

	public void PlayPauseButton() {
		app.PlayPause ();
	}
}

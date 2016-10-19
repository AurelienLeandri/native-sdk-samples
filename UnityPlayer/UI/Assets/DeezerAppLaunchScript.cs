using UnityEngine;
using System.Collections;
using System;

public class DeezerAppLaunchScript : MonoBehaviour {

	private MyDeezerApp app;

	// Use this for initialization
	void Start () {
		app = new MyDeezerApp ();
	}

	void OnApplicationQuit() {
		Debug.Log ("Application quit");
		app.Shutdown();
	}
}

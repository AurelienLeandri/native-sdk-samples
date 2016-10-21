using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class DeezerAppLaunchScript : MonoBehaviour {

	public MyDeezerApp app;
	private Button RepeatButton;
	private Button ShuffleButton;
	private Button PlayPauseButton;
	private Image OneImage;

	void Start () {
		RepeatButton = GameObject.Find ("RepeatButton").GetComponent<Button>();
		ShuffleButton = GameObject.Find ("ShuffleButton").GetComponent<Button>();
		PlayPauseButton = GameObject.Find ("PlayPauseButton").GetComponent<Button>();
		OneImage = GameObject.Find ("OneImage").GetComponent<Image> ();
		Color temp2 = new Color (1.0f, 1.0f, 1.0f, 0.0f);
		OneImage.color = temp2;
		app = new MyDeezerApp ();
	}

	void OnApplicationQuit() {
		Debug.Log ("Application quit");
		app.Shutdown();
	}

	public void StopButtonOnClick() {
		app.Stop ();
	}

	public void PlayPauseButtonOnClick() {
		app.PlayPause ();
	}

	public void NextbuttonOnClick() {
		app.Next ();
	}

	public void PreviousButtonOnClick() {
		app.Previous ();
	}

	public void RepeatButtonOnClick() {
		app.ToggleRepeat ();
		Color temp;
		Color temp2;
		temp2 = new Color (43f/255f, 216f/255f, 208f/255f, 0f);
		temp = new Color (43f/255f, 216f/255f, 208f/255f, 1.0f);
		if (app.RepeatMode == DZPlayerRepeatMode.ON)
			temp2 = new Color (43f/255f, 216f/255f, 208f/255f, 1.0f);
		else if (app.RepeatMode == DZPlayerRepeatMode.OFF)
			temp = Color.white;
		RepeatButton.image.color = temp;
		OneImage.color = temp2;
	}

	public void ShuffleButtonOnClick() {
		Color temp = Color.white;
		if (!app.isShuffleMode)
			temp = new Color (43f/255f, 216f/255f, 208f/255f, 1.0f);
		ShuffleButton.image.color = temp;
		app.ToggleRandom ();
	}
}

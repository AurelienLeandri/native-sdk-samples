using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using System.Threading;

public class DeezerAppLaunchScript : MonoBehaviour {

	public MyDeezerApp app;
	private Button RepeatButton;
	private Button ShuffleButton;
	private Button PlayPauseButton;
	public TrackListScript TrackList;
	private Image OneImage;
	private string contentLink;

	void Awake() {
		//string contentLink = "track/10287076"; // FIXME: choose your content here
		//string contentLink = "album/607845"; // FIXME: choose your content here
		contentLink = "playlist/1363560485"; // FIXME: choose your content here
		RepeatButton = GameObject.Find ("RepeatButton").GetComponent<Button>();
		ShuffleButton = GameObject.Find ("ShuffleButton").GetComponent<Button>();
		PlayPauseButton = GameObject.Find ("PlayPauseButton").GetComponent<Button>();
		OneImage = GameObject.Find ("OneImage").GetComponent<Image> ();
		Color temp2 = new Color (1.0f, 1.0f, 1.0f, 0.0f);
		OneImage.color = temp2;
		app = new MyDeezerApp ("dzmedia:///" + contentLink);
	}

	void Start () {
		LoadtrackList("https://api.deezer.com/" + contentLink);
	}

	public void LoadtrackList(string contentURL) {
		string jsonContent = MyDeezerApp.getContentJson (contentURL);
		if (contentURL.Contains ("album")) {
			AlbumInfo albumInfo = JsonUtility.FromJson<AlbumInfo> (jsonContent);
			contentURL += "/tracks";
			jsonContent = MyDeezerApp.getContentJson (contentURL);
			jsonContent = jsonContent.Substring (jsonContent.IndexOf ('['));
			jsonContent = jsonContent.Substring (0, jsonContent.LastIndexOf (']') + 1);
			jsonContent = "{\"Items\":" + jsonContent + "}";
			TrackInfo[] tracks = JsonHelper.FromJson<TrackInfo> (jsonContent);
			for (int i = 0; i < tracks.Length; i++) {
				tracks [i].album = albumInfo;
				TrackList.AddTrackList (tracks [i]);
			}
		} else if (contentURL.Contains ("track")) {
			TrackInfo trackInfo = JsonUtility.FromJson<TrackInfo> (jsonContent);
			TrackList.AddTrackList (trackInfo);
		} else if (contentURL.Contains ("playlist") || contentURL.Contains ("radio")) {
			contentURL += "/tracks";
			jsonContent = MyDeezerApp.getContentJson (contentURL);
			jsonContent = jsonContent.Substring (jsonContent.IndexOf ('['));
			jsonContent = jsonContent.Substring (0, jsonContent.LastIndexOf (']') + 1);
			jsonContent = "{\"Items\":" + jsonContent + "}";
			TrackInfo[] tracks = JsonHelper.FromJson<TrackInfo> (jsonContent);
			for (int i = 0; i < tracks.Length; i++) {
				TrackList.AddTrackList (tracks [i]);
			}
		}
		TrackList.ClickTrack (0);
	}

	void OnApplicationQuit() {
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

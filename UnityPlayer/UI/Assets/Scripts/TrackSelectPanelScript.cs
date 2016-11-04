using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TrackSelectPanelScript : ApplicationElement {
	public bool selected;
	private PlayingTrackScript PlayingTrack;
	public Text trackName;
	public Text artistName;
	public Image albumCover;
	public TrackInfo TrackInfo { get; private set; }
	private int index = 0;

	void Awake () {
		selected = false;
		PlayingTrack = GameObject.Find ("Canvas/TracklistPanel/PlayingTrackContainer").GetComponent<PlayingTrackScript> ();
	}

	void Start() {
	}
	
	void Update () {
	}

	public void OnCursorOver() {
		Image image = GetComponent<Image> ();
		Color color = image.color;
		color.a = 255;
		image.color = color;
	}

	public void OnCursorOut() {
		if (!selected) {
			Image image = GetComponent<Image> ();
			Color color = image.color;
			color.a = 0;
			image.color = color;
		}
	}

	public void OnClick() {
		SetSelected ();
		MainView.PlayTrackAtIndex (index);
	}
	
	public void SetSelected() {
		foreach (Transform child in transform.parent) {
			child.gameObject.GetComponent<Image> ().color = new Color32 (255, 255, 255, 0);
			child.gameObject.GetComponent<TrackSelectPanelScript> ().selected = false;
		}
		selected = true;
		GetComponent<Image> ().color = new Color32(83, 255, 248, 255);
		StartCoroutine (UpdatePlayingTrack ());
	}

	private IEnumerator UpdatePlayingTrack() {
		yield return albumCover.sprite;
		if (albumCover.sprite)
			PlayingTrack.UpdateInfo (trackName.text, artistName.text, albumCover.sprite.texture);
	}

	private IEnumerator LoadTexture(string textureUrl)
	{
		WWW www = new WWW(textureUrl);
		yield return www;
		albumCover.sprite = Sprite.Create (www.texture, new Rect (0, 0, www.texture.width, www.texture.height),
			new Vector2 (0.5f, 0.5f), 100);
	}

	public void SetTrackInfo(int index, TrackInfo info) {
		this.index = index;
		trackName = transform.Find ("TrackSelect/TrackSelectInfo/TrackName").gameObject.GetComponent<Text> ();
		artistName = transform.Find ("TrackSelect/TrackSelectInfo/ArtistName").gameObject.GetComponent<Text> ();
		albumCover = transform.Find ("TrackSelect/TrackSelectImage").gameObject.GetComponent<Image> ();
		TrackInfo = info;
		trackName.text = info.title;
		artistName.text = info.artist.name;
		StartCoroutine(LoadTexture (info.album.cover_small));
	}
}

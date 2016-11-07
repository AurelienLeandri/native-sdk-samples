using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TrackSelectPanelScript : ApplicationElement {
	public bool selected;
	private PlayingTrackScript PlayingTrack;
	public Text trackName;
	public Text artistName;
	public Image albumThumbnail;
	public Sprite albumCoverBig;
	public TrackInfo TrackInfo { get; private set; }
	private int index = 0;
	private bool coverToLoad = false;
	private bool indexToLoad = false;

	void Awake () {
		selected = false;
		PlayingTrack = GameObject.Find ("Canvas/TracklistPanel/PlayingTrackContainer").GetComponent<PlayingTrackScript> ();
	}

	void Start() {
	}
	
	void Update () {
		if (coverToLoad) {
			MainView.PlayerPanel.cover.overrideSprite = albumCoverBig;
			coverToLoad = false;
		}
		if (indexToLoad) {
			PlayingTrack.UpdateInfo (trackName.text, artistName.text, albumThumbnail.sprite.texture);
			indexToLoad = false;
		}
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
		if (albumThumbnail.sprite)
			PlayingTrack.UpdateInfo (trackName.text, artistName.text, albumThumbnail.sprite.texture);
		if (albumCoverBig)
			MainView.PlayerPanel.cover.overrideSprite = albumCoverBig;
	}

	private IEnumerator LoadCovers(string coverURL)
	{
		WWW www = new WWW(coverURL);
		yield return www;
		albumThumbnail.sprite = Sprite.Create (www.texture, new Rect (0, 0, www.texture.width, www.texture.height),
			new Vector2 (0.5f, 0.5f), 100);
		if (index == 0)
			indexToLoad = true;
	}

	private IEnumerator LoadAlbumSprite(string coverURL) {
		WWW www2 = new WWW(coverURL);
		yield return www2;
		albumCoverBig = Sprite.Create (www2.texture, new Rect (0, 0, www2.texture.width, www2.texture.height),
			new Vector2 (0.5f, 0.5f), 100);
		if (index == 0)
			coverToLoad = true;
	}

	public void SetTrackInfo(int index, TrackInfo info) {
		this.index = index;
		trackName = transform.Find ("TrackSelect/TrackSelectInfo/TrackName").gameObject.GetComponent<Text> ();
		artistName = transform.Find ("TrackSelect/TrackSelectInfo/ArtistName").gameObject.GetComponent<Text> ();
		albumThumbnail = transform.Find ("TrackSelect/TrackSelectImage").gameObject.GetComponent<Image> ();
		TrackInfo = info;
		trackName.text = info.title;
		artistName.text = info.artist.name;
		StartCoroutine(LoadCovers (info.album.cover_small));
		StartCoroutine(LoadAlbumSprite (info.album.cover_medium));
	}
}

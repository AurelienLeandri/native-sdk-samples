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
	private TimeSliderScript slider;
	private TrackInfo trackInfo;

	// Use this for initialization
	void Awake () {
		selected = false;
		PlayingTrack = GameObject.Find ("Canvas/TracklistPanel/PlayingTrackContainer").GetComponent<PlayingTrackScript> ();
		slider = GameObject.Find ("Canvas/PlayerPanel/PlayerSlider").GetComponent<TimeSliderScript> ();
	}

	void Start() {
	}
	
	// Update is called once per frame
	void Update () {
	}

	public void OnHover() {
		Image image = GetComponent<Image> ();
		Color color = image.color;
		color.a = 255;
		image.color = color;
	}

	public void OnHout() {
		if (!selected) {
			Image image = GetComponent<Image> ();
			Color color = image.color;
			color.a = 0;
			image.color = color;
		}
	}

	public void OnClick() {
		SetSelected ();
	}
	
	public void SetSelected() {
		foreach (Transform child in transform.parent) {
			child.gameObject.GetComponent<Image> ().color = new Color32 (255, 255, 255, 0);
			child.gameObject.GetComponent<TrackSelectPanelScript> ().selected = false;
		}
		selected = true;
		GetComponent<Image> ().color = new Color32(83, 255, 248, 255);
		StartCoroutine (UpdatePlayingTrack ());
		MainView.LoadIndex (transform.GetSiblingIndex ());
		slider.SliderComponent.maxValue = trackInfo.duration;
	}

	private IEnumerator UpdatePlayingTrack() {
		yield return albumCover.sprite;
		PlayingTrack.UpdateInfo (trackName.text, artistName.text, albumCover.sprite.texture);
	}

	private IEnumerator LoadTexture(string textureUrl)
	{
		WWW www = new WWW(textureUrl);
		yield return www;
		albumCover.sprite = Sprite.Create (www.texture, new Rect (0, 0, www.texture.width, www.texture.height),
			new Vector2 (0.5f, 0.5f), 100);
	}

	public void SetInfo(TrackInfo info) {
		trackName = transform.Find ("TrackSelect/TrackSelectInfo/TrackName").gameObject.GetComponent<Text> ();
		artistName = transform.Find ("TrackSelect/TrackSelectInfo/ArtistName").gameObject.GetComponent<Text> ();
		albumCover = transform.Find ("TrackSelect/TrackSelectImage").gameObject.GetComponent<Image> ();
		trackInfo = info;
		trackName.text = info.title;
		artistName.text = info.artist.name;
		StartCoroutine(LoadTexture (info.album.cover_small));
	}
}

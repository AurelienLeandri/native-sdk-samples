using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TrackSelectPanelScript : MonoBehaviour {
	public bool selected;
	private PlayingTrackScript PlayingTrack;
	private Text trackName;
	private Text artistName;
	private Image albumCover;

	// Use this for initialization
	void Start () {
		selected = false;
		PlayingTrack = GameObject.Find ("Canvas/TracklistPanel/PlayingTrackContainer").GetComponent<PlayingTrackScript> ();
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
		foreach (Transform child in transform.parent) {
			child.gameObject.GetComponent<Image> ().color = new Color32 (255, 255, 255, 0);
			child.gameObject.GetComponent<TrackSelectPanelScript> ().selected = false;
		}
		GetComponent<Image> ().color = new Color32(43, 216, 208, 255);
		PlayingTrack.UpdateInfo (trackName.text, artistName.text, albumCover.sprite.texture);
		selected = true;
	}

	private IEnumerator LoadTexture(string textureUrl)
	{
		WWW www = new WWW(textureUrl);
		yield return www;
		albumCover.sprite = Sprite.Create (www.texture, new Rect (0, 0, www.texture.width, www.texture.height),
			new Vector2 (0.5f, 0.5f), 100);
	}

	public void SetInfo(string title, string artist, string imageLink) {
		trackName = transform.Find ("TrackSelect/TrackSelectInfo/TrackName").gameObject.GetComponent<Text> ();
		artistName = transform.Find ("TrackSelect/TrackSelectInfo/ArtistName").gameObject.GetComponent<Text> ();
		albumCover = transform.Find ("TrackSelect/TrackSelectImage").gameObject.GetComponent<Image> ();
		trackName.text = title;
		artistName.text = artist;
		StartCoroutine(LoadTexture (imageLink));
	}
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayingTrackScript : MonoBehaviour {
	private Image AlbumCover;
	private Text TrackTitle;
	private Text ArtistTitle;

	public void Start() {
		AlbumCover = transform.Find ("AlbumCoverImage").gameObject.GetComponent<Image> ();
		TrackTitle = transform.Find ("TrackInfoContainer/TrackName").gameObject.GetComponent<Text> ();
		ArtistTitle = transform.Find ("TrackInfoContainer/ArtistName").gameObject.GetComponent<Text> ();
	}

	public void UpdateInfo(string trackTitle, string artistTitle, Texture2D cover) {
		TrackTitle.text = trackTitle;
		ArtistTitle.text = artistTitle;
		AlbumCover.sprite = Sprite.Create (cover, new Rect (0, 0, cover.width, cover.height),
			new Vector2 (0.5f, 0.5f), 100);
	}
}

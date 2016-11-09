using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class TrackListScript : ApplicationElement, Listener {
	public GameObject prefab;
	public PlayingTrackScript PlayingTrack;
	public InputField ContentField;
	private float lastTrackOffset;
	public List<TrackSelectPanelScript> Tracks { get; private set; }
	private Queue<Tuple<DZPlayerEvent, System.Object>> eventQueue = new Queue<Tuple<DZPlayerEvent, System.Object>> ();

	void Awake () {
		Tracks = new List<TrackSelectPanelScript> ();
		eventQueue = new Queue<Tuple<DZPlayerEvent, System.Object>> ();
	}

	void Start () {
		lastTrackOffset = 0;
		ContentField.text = "playlist/1363560485";
		MainView.Listeners.Add (this);
		// We load a default content at startup
		LoadTrackList(ContentField.text);
		if (Tracks.Count != 0)
			MainView.LoadContent (ContentField.text);
	}

	public void Notify(DZPlayerEvent playerEvent, System.Object data) {
		eventQueue.Enqueue (new Tuple<DZPlayerEvent, System.Object> (playerEvent, data));
	}

	private void ClearTrackList() {
		Tracks.ForEach(child => Destroy(child.gameObject));
		Tracks.Clear ();
		lastTrackOffset = 0;
	}

	public void AddTrackToList(TrackInfo track) {
		GameObject o = (GameObject) Instantiate (prefab, new Vector3(transform.position.x, transform.position.y, transform.position.z),
			transform.rotation);
		o.transform.parent = this.transform;
		o.transform.localPosition = new Vector3(prefab.transform.localPosition.x,
			prefab.transform.localPosition.y + lastTrackOffset, prefab.transform.localPosition.z);
		o.transform.localScale = new Vector3 (1, 1, 1);
		lastTrackOffset -= 45;
		RectTransform rt = (RectTransform)transform;
		if (rt.sizeDelta.y < -lastTrackOffset)
			rt.sizeDelta = new Vector2 (rt.sizeDelta.x, rt.sizeDelta.y + 45);
		o.GetComponent<TrackSelectPanelScript> ().SetTrackInfo(Tracks.Count, track);
		Tracks.Add (o.GetComponent<TrackSelectPanelScript> ());
	}

	/// <summary>
	/// Loads the track list by retrieving the content json from the API.
	/// </summary>
	/// <param name="contentURL">The id of the content that will be sent as requet to the API.</param>
	public void LoadTrackList(string contentURL) {
		if (contentURL == null || contentURL.Length == 0)
			return;
		contentURL = "https://api.deezer.com/" + contentURL;
		Debug.Log (contentURL);
		string jsonContent = ApplicationMainScript.GetContentJson (contentURL);
		Debug.Log (jsonContent);
		if (contentURL.Contains ("album")) {
			AlbumInfo albumInfo = JsonUtility.FromJson<AlbumInfo> (jsonContent);
			contentURL += "/tracks";
			jsonContent = ApplicationMainScript.GetContentJson (contentURL);
			jsonContent = jsonContent.Substring (jsonContent.IndexOf ('['));
			jsonContent = jsonContent.Substring (0, jsonContent.LastIndexOf (']') + 1);
			jsonContent = "{\"Items\":" + jsonContent + "}";
			TrackInfo[] tracks = JsonListParser.FromJson<TrackInfo> (jsonContent);
			if (tracks.Length == 0)
				return;
			Debug.Log (tracks.Length);
			for (int i = 0; i < tracks.Length; i++) {
				tracks [i].album = albumInfo;
				AddTrackToList (tracks [i]);
			}
			TrackSelectPanelScript firstChild = transform.GetChild (0).gameObject.GetComponent<TrackSelectPanelScript> ();
			PlayingTrack.UpdateInfo (firstChild.trackName.text, firstChild.artistName.text, tracks [0].album.cover_small);
		} else if (contentURL.Contains ("track")) {
			TrackInfo trackInfo = JsonUtility.FromJson<TrackInfo> (jsonContent);
			AddTrackToList (trackInfo);
			TrackSelectPanelScript firstChild = transform.GetChild (0).gameObject.GetComponent<TrackSelectPanelScript> ();
			PlayingTrack.UpdateInfo (firstChild.trackName.text, firstChild.artistName.text, trackInfo.album.cover_small);
		} else if (contentURL.Contains ("playlist") || contentURL.Contains ("radio")) {
			contentURL += "/tracks";
			jsonContent = ApplicationMainScript.GetContentJson (contentURL);
			jsonContent = jsonContent.Substring (jsonContent.IndexOf ('['));
			jsonContent = jsonContent.Substring (0, jsonContent.LastIndexOf (']') + 1);
			jsonContent = "{\"Items\":" + jsonContent + "}";
			TrackInfo[] tracks = JsonListParser.FromJson<TrackInfo> (jsonContent);
			if (tracks.Length == 0)
				return;
			for (int i = 0; i < tracks.Length; i++) {
				AddTrackToList (tracks [i]);
			}
		} else return;
		Tracks [0].SetSelected ();
	}

	void Update() {
		PollEvents ();
	}

	private void PollEvents() {
		while (eventQueue.Count > 0) {
			Tuple<DZPlayerEvent, System.Object> eventTuple = eventQueue.Dequeue ();
			switch (eventTuple.first) {
			case DZPlayerEvent.RENDER_TRACK_START:
				if (MainView.IndexInPlaylist >= 0)
					Tracks [MainView.IndexInPlaylist].SetSelected ();
				break;
			default:
				break;
			}
		}
	}

	public void GoButtonOnClick() {
		ClearTrackList ();
		LoadTrackList(ContentField.text);
		if (Tracks.Count != 0)
			MainView.LoadContent (ContentField.text);
	}
}

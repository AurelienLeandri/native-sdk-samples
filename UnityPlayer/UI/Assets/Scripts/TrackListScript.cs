using UnityEngine;
using System.Collections;

public class TrackListScript : ApplicationElement {
	public GameObject prefab;
	private float lastTrackOffset;

	// Use this for initialization
	void Start () {
		lastTrackOffset = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void AddTrackList(TrackInfo track) {
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
		o.GetComponent<TrackSelectPanelScript> ().SetInfo(track);
	}

	public void LoadtrackList(string contentURL) {
		string jsonContent = ApplicationMainScript.getContentJson (contentURL);
		if (contentURL.Contains ("album")) {
			AlbumInfo albumInfo = JsonUtility.FromJson<AlbumInfo> (jsonContent);
			contentURL += "/tracks";
			jsonContent = ApplicationMainScript.getContentJson (contentURL);
			jsonContent = jsonContent.Substring (jsonContent.IndexOf ('['));
			jsonContent = jsonContent.Substring (0, jsonContent.LastIndexOf (']') + 1);
			jsonContent = "{\"Items\":" + jsonContent + "}";
			TrackInfo[] tracks = JsonHelper.FromJson<TrackInfo> (jsonContent);
			for (int i = 0; i < tracks.Length; i++) {
				tracks [i].album = albumInfo;
				AddTrackList (tracks [i]);
			}
		} else if (contentURL.Contains ("track")) {
			TrackInfo trackInfo = JsonUtility.FromJson<TrackInfo> (jsonContent);
			AddTrackList (trackInfo);
		} else if (contentURL.Contains ("playlist") || contentURL.Contains ("radio")) {
			contentURL += "/tracks";
			jsonContent = ApplicationMainScript.getContentJson (contentURL);
			jsonContent = jsonContent.Substring (jsonContent.IndexOf ('['));
			jsonContent = jsonContent.Substring (0, jsonContent.LastIndexOf (']') + 1);
			jsonContent = "{\"Items\":" + jsonContent + "}";
			TrackInfo[] tracks = JsonHelper.FromJson<TrackInfo> (jsonContent);
			for (int i = 0; i < tracks.Length; i++) {
				AddTrackList (tracks [i]);
			}
		}
		transform.GetChild (0).gameObject.GetComponent<TrackSelectPanelScript> ().SetSelected ();
	}
}

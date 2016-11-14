﻿using UnityEngine;
using System.Collections;

public class TrackListScript : MonoBehaviour {
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

	public void ClickTrack(int index) {
		transform.GetChild (index).gameObject.GetComponent<TrackSelectPanelScript> ().SetSelected ();
	}
}

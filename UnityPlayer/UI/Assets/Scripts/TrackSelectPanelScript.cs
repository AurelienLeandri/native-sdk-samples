using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TrackSelectPanelScript : MonoBehaviour {
	public bool selected;

	// Use this for initialization
	void Start () {
		selected = false;
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
		selected = true;
	}
}

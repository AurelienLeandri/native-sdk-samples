using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeSliderScript : MonoBehaviour {
	public Slider SliderComponent { get; private set; }
	private MyDeezerApp app;

	// Use this for initialization
	void Awake () {
		SliderComponent = transform.GetComponent<Slider> ();
		app = GameObject.Find ("AppObject").GetComponent<DeezerAppLaunchScript> ().app;
	}

	public void OnValueChanged(int value) {
		app.Seek (value);
	}

	// Update is called once per frame
	void Update () {
	}
}

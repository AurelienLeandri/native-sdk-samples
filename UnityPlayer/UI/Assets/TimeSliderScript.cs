using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimeSliderScript : MonoBehaviour {
	public Slider SliderComponent { get; private set; }
	private MyDeezerApp app;

	// Use this for initialization
	void Awake () {
		SliderComponent = transform.GetComponent<Slider> ();
	}

	void Start() {
		app = GameObject.Find ("AppObject").GetComponent<DeezerAppLaunchScript> ().app;
	}

	public void OnValueChanged() {
		app.Seek ((int)(SliderComponent.value));
	}

	// Update is called once per frame
	void Update () {
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TimeSliderScript : MonoBehaviour, Listener {
	public Slider SliderComponent { get; private set; }
	private MyDeezerApp app;
	private Queue<Tuple<DZPlayerEvent, Object>> eventQueue;

	// Use this for initialization
	void Awake () {
		SliderComponent = transform.GetComponent<Slider> ();
	}

	void Notify(DZPlayerEvent playerEvent, Object data) {
		eventQueue.Enqueue (new Tuple<DZPlayerEvent, Object> (playerEvent, data));
	}

	void Start() {
		app = GameObject.Find ("AppObject").GetComponent<DeezerAppLaunchScript> ().app;
	}

	public void OnValueChanged() {
		app.Seek ((int)(SliderComponent.value));
	}

	private void pollEvents() {
		while (eventQueue.Count > 0) {
			Tuple<DZPlayerEvent, Object> eventObj = eventQueue.Dequeue ();
			DZPlayerEvent playerEvent = eventObj.first;
			switch (playerEvent) {
			case DZPlayerEvent.RENDER_TRACK_START:
				int trackIndex = eventObj.second as int;
				if (trackIndex == null)
					break;
				SliderComponent.value = 0;
				break;
			}
		}
	}

	// Update is called once per frame
	void Update () {
	}
}

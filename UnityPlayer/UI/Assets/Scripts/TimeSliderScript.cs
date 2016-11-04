using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

public class TimeSliderScript : ApplicationElement, Listener {
	public Slider SliderComponent;
	private Queue<Tuple<DZPlayerEvent, System.Object>> eventQueue = new Queue<Tuple<DZPlayerEvent, System.Object>> ();
	private float timeCounter = 0;
	private bool ignoreValueChange;

	void Awake () {
		ignoreValueChange = false;
		SliderComponent = transform.GetComponent<Slider> ();
	}

	public void Notify(DZPlayerEvent playerEvent, System.Object data) {
		eventQueue.Enqueue (new Tuple<DZPlayerEvent, System.Object> (playerEvent, data));
	}

	void Start() {
		MainView.Listeners.Add (this);
	}

	public void OnValueChanged() {
		if (!ignoreValueChange) {
			MainView.PlaySongAtTimestamp ((int)(SliderComponent.value));
			timeCounter = 0;
		}
	}

	private void PollEvents() {
		while (eventQueue.Count > 0) {
			Tuple<DZPlayerEvent, System.Object> eventTuple = eventQueue.Dequeue ();
			switch (eventTuple.first) {
			case DZPlayerEvent.RENDER_TRACK_START:
				int index = Convert.ToInt32 (eventTuple.second);
				ignoreValueChange = true;
				SliderComponent.maxValue = MainView.TrackListPanel.Tracks [index].TrackInfo.duration;
				SliderComponent.value = 0;
				ignoreValueChange = false;
				break;
			default:
				break;
			}
		}
	}

	// Update is called once per frame
	void Update () {
		PollEvents ();
		timeCounter += Time.deltaTime;
		if (timeCounter > 1) {
			timeCounter -= 1;
			ignoreValueChange = true;
			SliderComponent.value++;
			ignoreValueChange = false;
		}
	}
}

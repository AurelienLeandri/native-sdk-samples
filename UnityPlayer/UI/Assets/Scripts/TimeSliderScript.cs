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
		MainView.Listeners.Add (this);
	}

	public void Notify(DZPlayerEvent playerEvent, System.Object data) {
		eventQueue.Enqueue (new Tuple<DZPlayerEvent, System.Object> (playerEvent, data));
	}

	void Start() {
		SliderComponent.value = SliderComponent.maxValue;
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
			if (eventTuple.first == DZPlayerEvent.QUEUELIST_TRACK_RIGHTS_AFTER_AUDIOADS)
				SliderComponent.gameObject.SetActive (false);
			else
				SliderComponent.gameObject.SetActive (true);
			switch (eventTuple.first) {
			case DZPlayerEvent.RENDER_TRACK_START:
				int index = Convert.ToInt32 (eventTuple.second);
				ignoreValueChange = true;
					SliderComponent.value = 0;
				if (index >= 0)
					SliderComponent.maxValue = MainView.TrackListPanel.Tracks [index].TrackInfo.duration;
				else
					SliderComponent.maxValue = 0;
				ignoreValueChange = false;
				break;
			default:
				break;
			}
		}
	}

	void Update () {
		PollEvents ();
		if (!MainView.isPaused && !MainView.isStopped)
			timeCounter += Time.deltaTime;
			if (timeCounter > 1) {
				timeCounter -= 1;
				ignoreValueChange = true;
				SliderComponent.value++;
				ignoreValueChange = false;
			}
		if (MainView.isStopped) {
			ignoreValueChange = true;
			SliderComponent.value = 0;
			ignoreValueChange = false;
		}
	}
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TimeSliderScript : ApplicationElement {
	public Slider SliderComponent { get; private set; }
	private Queue<Tuple<DZPlayerEvent, Object>> eventQueue;

	// Use this for initialization
	void Awake () {
		SliderComponent = transform.GetComponent<Slider> ();
	}

	void Notify(DZPlayerEvent playerEvent, Object data) {
		eventQueue.Enqueue (new Tuple<DZPlayerEvent, Object> (playerEvent, data));
	}

	void Start() {
	}

	public void OnValueChanged() {
		MainView.Seek ((int)(SliderComponent.value));
	}

	private void pollEvents() {
	}

	// Update is called once per frame
	void Update () {
	}
}

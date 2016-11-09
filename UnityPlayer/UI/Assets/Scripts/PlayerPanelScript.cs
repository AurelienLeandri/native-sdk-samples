using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class PlayerPanelScript : ApplicationElement, Listener {
	public Button RepeatButton;
	private Image RepeatButtonOneImage;
	public Button ShuffleButton;
	public Button PlayPauseButton;
	public Button NextButton;
	public Button PreviousButton;
	public Button StopButton;
	public Sprite playSprite;
	public Sprite pauseSprite;
	public Image cover;
	public Queue<Tuple<DZPlayerEvent, System.Object>> eventQueue;
	public TimeSliderScript TimeSlider;

	void Awake() {
		eventQueue = new Queue<Tuple<DZPlayerEvent, System.Object>> ();
		MainView.Listeners.Add (this);
	}

	void Start() {
		Color temp = new Color (1.0f, 1.0f, 1.0f, 0.0f);
		RepeatButtonOneImage = transform.Find ("OneImage").GetComponent<Image> ();
		RepeatButtonOneImage.color = temp;
	}

	void Update() {
		PollEvents ();
	}

	private void PollEvents() {
		while (eventQueue.Count > 0) {
			Tuple<DZPlayerEvent, System.Object> eventTuple = eventQueue.Dequeue ();
			switch (eventTuple.first) {
			case DZPlayerEvent.RENDER_TRACK_START:
				PlayPauseButton.image.overrideSprite = pauseSprite;
				break;
			case DZPlayerEvent.RENDER_TRACK_PAUSED:
				PlayPauseButton.image.overrideSprite = playSprite;
				break;
			case DZPlayerEvent.RENDER_TRACK_REMOVED:
				PlayPauseButton.image.overrideSprite = playSprite;
				break;
			case DZPlayerEvent.RENDER_TRACK_RESUMED:
				PlayPauseButton.image.overrideSprite = pauseSprite;
				break;
			default:
				break;
			}
		}
	}

	public void Notify (DZPlayerEvent playerEvent, System.Object eventData) {
		eventQueue.Enqueue (new Tuple<DZPlayerEvent, System.Object> (playerEvent, eventData));
	}

	public void StopButtonOnClick() {
		PlayPauseButton.image.overrideSprite = playSprite;
		MainView.Stop ();
	}

	public void PlayPauseButtonOnClick() {
		MainView.PlayPause ();
	}

	public void NextbuttonOnClick() {
		PlayPauseButton.image.overrideSprite = playSprite;
		MainView.PlayNextTrack ();
	}

	public void PreviousButtonOnClick() {
		PlayPauseButton.image.overrideSprite = playSprite;
		MainView.PlayPreviousTrack ();
	}

	public void RepeatButtonOnClick() {
		MainView.ToggleRepeatMode ();
		Color temp;
		Color temp2;
		temp = new Color (43f/255f, 216f/255f, 208f/255f, 1.0f);
		temp2 = new Color (43f/255f, 216f/255f, 208f/255f, 0f);
		if (MainView.RepeatMode == DZPlayerRepeatMode.ON)
			temp2 = new Color (43f/255f, 216f/255f, 208f/255f, 1.0f);
		else if (MainView.RepeatMode == DZPlayerRepeatMode.OFF)
			temp = Color.white;
		RepeatButton.image.color = temp;
		RepeatButtonOneImage.color = temp2;
	}

	public void ShuffleButtonOnClick() {
		Color temp = Color.white;
		if (!MainView.isShuffleMode)
			temp = new Color (43f/255f, 216f/255f, 208f/255f, 1.0f);
		ShuffleButton.image.color = temp;
		MainView.ToggleRandomMode ();
	}
}

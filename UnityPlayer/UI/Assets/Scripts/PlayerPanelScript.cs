using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerPanelScript : ApplicationElement {
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
	public TimeSliderScript TimeSlider;

	void Start() {
		Color temp = new Color (1.0f, 1.0f, 1.0f, 0.0f);
		RepeatButtonOneImage = transform.Find ("OneImage").GetComponent<Image> ();
		RepeatButtonOneImage.color = temp;
	}

	public void StopButtonOnClick() {
		PlayPauseButton.image.overrideSprite = playSprite;
		MainView.Stop ();
	}

	public void PlayPauseButtonOnClick() {
		if (MainView.isPaused)
			PlayPauseButton.image.overrideSprite = playSprite;
		else
			PlayPauseButton.image.overrideSprite = pauseSprite;
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
		temp2 = new Color (43f/255f, 216f/255f, 208f/255f, 0f);
		temp = new Color (43f/255f, 216f/255f, 208f/255f, 1.0f);
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

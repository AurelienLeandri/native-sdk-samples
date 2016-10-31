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

	void Start() {
		Color temp = new Color (1.0f, 1.0f, 1.0f, 0.0f);
		RepeatButtonOneImage = transform.Find ("OneImage").GetComponent<Image> ();
		RepeatButtonOneImage.color = temp;
	}

	public void StopButtonOnClick() {
		MainView.Stop ();
	}

	public void PlayPauseButtonOnClick() {
		MainView.PlayPause ();
	}

	public void NextbuttonOnClick() {
		MainView.Next ();
	}

	public void PreviousButtonOnClick() {
		MainView.Previous ();
	}

	public void RepeatButtonOnClick() {
		MainView.ToggleRepeat ();
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
		MainView.ToggleRandom ();
	}
}

using UnityEngine;
using System.Collections;

public class ListPanelScript : MonoBehaviour {
	private bool mouseOn;
	private Transform rotationTo;
	private Transform rotationFrom;
	private float speed = 30f;

	// Use this for initialization
	void Awake () {
		rotationFrom = GameObject.Find("RotationFrom").transform;
		rotationTo = GameObject.Find("RotationTo").transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (mouseOn)
			transform.rotation = Quaternion.RotateTowards (transform.rotation, rotationTo.rotation, Time.deltaTime * speed);
		else
			transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationFrom.rotation, Time.deltaTime * speed);
	}

	public void  rotateIn() {
		mouseOn = true;
	}

	public void rotateOut() {
		mouseOn = false;
	}
}

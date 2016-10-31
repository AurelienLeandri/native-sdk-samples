using System;
using UnityEngine;

public abstract class ApplicationElement : MonoBehaviour
{
	public ApplicationMainScript MainView {
		get { return GameObject.Find ("AppObject").GetComponent<ApplicationMainScript> (); }
		private set { MainView = value; }
	}
}


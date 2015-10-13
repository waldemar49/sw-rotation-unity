using System;
using UnityEngine;
using UnityEngine.UI;

public class TextRotationListener : MonoBehaviour, Rotation.Listener {
	private Text text;

	void Start() {
		text = GetComponent<Text>();
		GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
		gameController.getRotation().addListener(this);
	}

	public void on(Quaternion q) {
		text.text = "x: " + q.x + "\ny: " + q.y + "\nz: " + q.z + "\nw: " + q.w;
	}
}

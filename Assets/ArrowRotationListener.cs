using UnityEngine;
using System.Collections;

public class ArrowRotationListener : MonoBehaviour, Rotation.Listener {
	void Start () {
		GameController gameController = GameObject.Find("GameController").GetComponent<GameController>();
		gameController.getRotation().addListener(this);
	}

	public void on(Quaternion q) {
		transform.rotation = q;
	}
}

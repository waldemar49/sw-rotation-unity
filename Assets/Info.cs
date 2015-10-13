using UnityEngine;
using UnityEngine.UI;

public class Info : MonoBehaviour, PlainRotation.Listener {
	private Text text;

	void Start() {
		text = GetComponent<Text>();
		RotationController.Rotation().Add(this);
	}

	public void On(Quaternion q) {
		text.text = "x: " + q.x + "\ny: " + q.y + "\nz: " + q.z + "\nw: " + q.w;
	}
}

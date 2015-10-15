using UnityEngine;
using UnityEngine.UI;

public class Info : MonoBehaviour, PlainRotation.Listener {
	private Text text;

	void Start() {
		text = GetComponent<Text>();
		RotationController.PlainRotation().Add(this);
	}

	public void On(Quaternion q) {
		Vector3 e = q.eulerAngles;
		text.text = "x: " + q.x + "\ny: " + q.y + "\nz: " + q.z + "\nw: " + q.w + "\n\nx: " + e.x + "\ny: " + e.y + "\nz: " + e.z;
	}
}

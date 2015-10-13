using UnityEngine;

public class Arrow : MonoBehaviour, PlainRotation.Listener {
	void Start () {
		RotationController.CalibratedRotation().Add(this);
	}

	public void On(Quaternion q) {
		transform.rotation = q;
	}
}

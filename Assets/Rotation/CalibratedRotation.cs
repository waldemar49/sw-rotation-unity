using System.Collections.Generic;
using UnityEngine;

public class CalibratedRotation : MonoBehaviour, Rotation.Listener {
	private readonly List<Rotation.Listener> listeners;
	private Quaternion zero;

	public CalibratedRotation() {
		listeners = new List<Rotation.Listener>();
	}

	void Start() {
		RotationController.PlainRotation().Add(this);

		Reset();
	}

	public void Add(Rotation.Listener listener) {
		listeners.Add(listener);
	}

	public void Remove(Rotation.Listener listener) {
		listeners.Remove(listener);
	}

	public void On(Quaternion q) {
		if (Input.GetKeyDown(KeyCode.Q)) {
			if (Input.GetKey(KeyCode.LeftShift)) {
				Reset();
			} else if (Input.GetKey(KeyCode.LeftControl)) {
				Calibrate(q);
			}
		}
		Quaternion r = zero * q;
		foreach (Rotation.Listener listener in listeners) {
			listener.On(r);
		}
	}

	private void Reset() {
		zero = Quaternion.identity;
	}

	private void Calibrate(Quaternion q) {
		zero = Quaternion.Inverse(q);
	}
}

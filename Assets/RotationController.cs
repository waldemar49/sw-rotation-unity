using UnityEngine;

public class RotationController : MonoBehaviour {
	private PlainRotation rotation;
	private CalibratedRotation calibratedRotation;

	void Awake() {
		rotation = base.gameObject.GetComponent<PlainRotation>();
		calibratedRotation = base.gameObject.GetComponent<CalibratedRotation>();
	}

	public static PlainRotation PlainRotation() {
		return Controller().rotation;
	}

	public static CalibratedRotation CalibratedRotation() {
		return Controller().calibratedRotation;
	}

	private static GameObject sGameObject;
	private static GameObject GameObject() {
		if (sGameObject == null) {
			sGameObject = UnityEngine.GameObject.Find("RotationController");
		}
		return sGameObject;
	}

	private static RotationController sController;
	private static RotationController Controller() {
		if (sController == null) {
			sController = GameObject().GetComponent<RotationController>();
		}
		return sController;
	}
}

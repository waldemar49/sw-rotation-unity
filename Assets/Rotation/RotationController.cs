using UnityEngine;

public class RotationController : MonoBehaviour {

    private Rotation rotation;
    private CalibratedRotation calibratedRotation;

    void Awake() {
        rotation = GetComponent<Rotation>();
        calibratedRotation = GetComponent<CalibratedRotation>();
    }

    public static Rotation PlainRotation() {
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

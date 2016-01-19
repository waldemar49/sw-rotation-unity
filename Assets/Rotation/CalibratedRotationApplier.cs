using UnityEngine;

public class CalibratedRotationApplier : MonoBehaviour, Rotation.Listener {

    void Start () {
        RotationController.CalibratedRotation().Add(this);
    }

    public void On(Quaternion q) {
        transform.rotation = q;
    }
}

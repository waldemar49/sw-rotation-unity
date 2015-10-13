using UnityEngine;

public class GameController : MonoBehaviour {
	private Rotation rotation;

	void Awake() {
		rotation = gameObject.GetComponent<Rotation>();
	}

	public Rotation getRotation() {
		return rotation;
	}
}

using UnityEngine;
using System.Collections;

public class GameOptions : MonoBehaviour {

	public bool canChat;
	
	void Start () {
		if (!canChat) {
			NetworkChat script = GetComponent<NetworkChat>();
			script.enabled = false;
		}
	}
}

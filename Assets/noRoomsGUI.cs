using UnityEngine;
using System.Collections;

public class noRoomsGUI : MonoBehaviour {

	public bool OpenRoomGUI;

	void OnGUI () {
		if (OpenRoomGUI) {
			NetworkManager tp = GetComponent<NetworkManager> ();

			GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			GUILayout.BeginVertical ();
			GUILayout.FlexibleSpace ();

			if (GUILayout.Button ("Teams")) {
				tp.teamPlay = true;
				OpenRoomGUI = false;
				PhotonNetwork.CreateRoom (null);
			}
			if (GUILayout.Button ("All for All")) {
				tp.teamPlay = false;
				OpenRoomGUI = false;
				PhotonNetwork.CreateRoom (null);
			}
			
			GUILayout.FlexibleSpace ();
			GUILayout.EndVertical ();
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
			GUILayout.EndArea ();	
		}
	}
}

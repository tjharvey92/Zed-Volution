using UnityEngine;
using System.Collections.Generic;

public class NetworkChat : Photon.MonoBehaviour {
	
	List<string> messages = new List<string>();
	private string inputField = "";
	private Vector2 scrollPosition = Vector2.zero;
	public int maxMessages = 10;

	public	bool  showChat = false;
	
	void  Start ()
	{
		Debug.Log (PhotonNetwork.player.name);
	}
	
	//If player leaves room tell other players about it.
	void  OnPhotonPlayerDisconnected (PhotonPlayer dPlayer){
		if(PhotonNetwork.isMasterClient) photonView.RPC("Chat", PhotonTargets.All, dPlayer.name + " left game", "");	
	}
	
	//If master player goes offline inform all players about new master player.
	void  OnMasterClientSwitched (PhotonPlayer dPlayer){
		Debug.Log("MasterClient changed to: " + dPlayer);
		
		if (PhotonNetwork.connected){
			if(PhotonNetwork.isMasterClient) photonView.RPC("Chat", PhotonTargets.All, "NEW MASTER PLAYER - " + dPlayer, "");
		}
	}
	
	//Come back to this, need to add guiskin or go with Ngui.
	void  OnGUI (){
		if(showChat){
			Screen.lockCursor = false;
			GUILayout.BeginArea( new Rect(2, 2, 350, 250), "", "box");
			GUILayout.BeginHorizontal("box");
			GUILayout.Label("<b> CHAT [<color=blue>ALL</color>] </b>");
			GUILayout.FlexibleSpace();
			
			
			GUILayout.EndHorizontal();
			if (Event.current.type == EventType.keyDown && Event.current.keyCode == KeyCode.Escape){ 
				showChat = false;
			}
		}else {
			GUILayout.BeginArea( new Rect(2, 32, 350, 187), "", "");
		}
		
		scrollPosition = GUILayout.BeginScrollView(scrollPosition, "box");
		GUILayout.FlexibleSpace();
		for ( int u = 0; u < messages.Count; u++){
			GUILayout.Label(messages[u].ToString());
		}
		GUILayout.EndScrollView ();
		
		if(Event.current.type == EventType.keyDown && Event.current.character == "\n"[0]){
			if(inputField.Length > 0){
				photonView.RPC("Chat", PhotonTargets.All, inputField, PhotonNetwork.player.name);
				inputField = "";
				showChat = false;
				Screen.lockCursor = true;
			}
		}
		
		if(showChat){
			GUI.SetNextControlName("ChatField");
			GUILayout.BeginHorizontal("box");
			inputField = GUILayout.TextField(inputField, GUILayout.Width(280));
			if(GUILayout.Button("<b>Send</b>", GUILayout.Width(50)) && inputField.Length > 0){
				showChat = false;
				photonView.RPC("Chat", PhotonTargets.All, inputField, PhotonNetwork.player.name);
				inputField = "";
				Screen.lockCursor = true; 
			}
			GUI.FocusControl("ChatField");	
			GUILayout.EndHorizontal();
			
		}
		GUILayout.EndArea();
		
		//Turn on chat
		if (Event.current.type == EventType.keyDown && Event.current.character == "\n"[0] && !showChat || Event.current.type == EventType.keyDown && Event.current.character == "y"[0] && !showChat){ 
			showChat = true;
			Screen.lockCursor = false;
		}else if(Event.current.type == EventType.keyDown && Event.current.character == '\n' && inputField.Length == 0){
			showChat = false;
			Screen.lockCursor = true; 
		}
	}
	//Rpc for Msg
	[RPC]
	void  Chat (string message,string senderName){ 
		string nSender = "";
		scrollPosition.y = Mathf.Infinity;
		if (senderName != null){
			if (senderName == ""){
				nSender = "<b><color=cyan>[SERVER]</color></b>";
			}else{
				nSender = "<b><color=lime>" + senderName + "</color></b>";
			}
		}
		
		if(messages.Count > maxMessages){
			messages.RemoveAt(0);
		}
		
		messages.Add( nSender + ": " + "<b>" + message + "</b>");
	}
	
	
}

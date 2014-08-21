﻿using UnityEngine;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour {

	public GameObject standbyCamera;
	//public GameObject _GUIStuff.Crosshairs;
	SpawnSpot[] spawnSpots;

	public bool offlineMode = false;

	bool connecting = false;

	List<string> chatMessages;
	int maxChatMessages = 5;

	public float respawnTimer = 0f;

	bool hasPickedTeam = false;
	int teamID=0;

	public bool teamPlay;
	public bool roomBuildGUI;
	public bool mainMenu;

	// Use this for initialization
	void Start () {
		//Crosshairs.SetActive (false);
		mainMenu = false;
		spawnSpots = GameObject.FindObjectsOfType<SpawnSpot>();
		PhotonNetwork.player.name = PlayerPrefs.GetString("Username", "ZED_Player");
		chatMessages = new List<string> ();
	}

	void OnDestroy() {
		PlayerPrefs.SetString ("Username", PhotonNetwork.player.name);
		//Hashtable props = new Hashtable();
		//props["test"] = 1;
		//PhotonNetwork.player.SetCustomProperties(props);

	}
	
	public void AddChatMessage(string m) {
		GetComponent<PhotonView> ().RPC ("AddChatMessage_RPC", PhotonTargets.AllBuffered, m);
	}

	[RPC]
	void AddChatMessage_RPC(string m) {
		while (chatMessages.Count >= maxChatMessages) {
			chatMessages.RemoveAt (0);
		}
		chatMessages.Add(m);
	}

	void Connect() {
		PhotonNetwork.ConnectUsingSettings( "Zed!Volution v0.04" );
	}
	 
	void OnGUI() {
		if (mainMenu) {
			GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
			GUILayout.BeginHorizontal ();
			GUILayout.FlexibleSpace ();
			GUILayout.BeginVertical ();
			GUILayout.FlexibleSpace ();

			GUILayout.Button("TESTING");
			
			GUILayout.FlexibleSpace ();
			GUILayout.EndVertical ();
			GUILayout.FlexibleSpace ();
			GUILayout.EndHorizontal ();
			GUILayout.EndArea ();
		}

		GUILayout.Label( PhotonNetwork.connectionStateDetailed.ToString() );

		if (PhotonNetwork.connected == false && connecting == false ) {
			// We have not yet connected, so ask the player for online vs. offline mode
			GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.BeginVertical();
			GUILayout.FlexibleSpace();

			GUILayout.BeginHorizontal();
			GUILayout.Label("Username: ");
			PhotonNetwork.player.name = GUILayout.TextField(PhotonNetwork.player.name);
			GUILayout.EndHorizontal();

			if( GUILayout.Button("Single Player") ) {
				connecting = true;
				PhotonNetwork.offlineMode = true;
				OnJoinedLobby();
			}	
			if( GUILayout.Button("Multiplayer") ) {
				connecting = true;
				Connect ();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndVertical();
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			GUILayout.EndArea();
		}

		if (PhotonNetwork.connected == true && connecting == false) {

			if(hasPickedTeam) {
			// We are fully connected, make sure to display the chat box
				GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();

				foreach(string msg in chatMessages) {
					GUILayout.Label(msg);
				}

				GUILayout.EndVertical();
				GUILayout.EndArea();
			}
			else {
				// Player has not yet selected a team
				GUILayout.BeginArea( new Rect(0, 0, Screen.width, Screen.height) );
				GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				GUILayout.BeginVertical();
				GUILayout.FlexibleSpace();

				if( teamPlay ) {
					if( GUILayout.Button("Infected") ) {
						SpawnMyPlayer(1);
					}		
					if( GUILayout.Button("Non-Infected") ) {
						SpawnMyPlayer(2);
					}
					if( GUILayout.Button("Random") ) {
						SpawnMyPlayer(Random.Range (0,3)); // 0, 1, or 2
					}
				}
				if ( !teamPlay ) {
					if( GUILayout.Button("Renegade") ) {
						SpawnMyPlayer(0);
					}
				}

				GUILayout.FlexibleSpace();
				GUILayout.EndVertical();
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
				GUILayout.EndArea();
			}
		}
	}

	void OnJoinedLobby() {
		Debug.Log ("OnJoinedLobby");
		PhotonNetwork.JoinRandomRoom();
	}

	void OnPhotonRandomJoinFailed() {
		noRoomsGUI nr = GetComponent<noRoomsGUI> ();
		nr.OpenRoomGUI = true;
		Debug.Log ("OnPhotonRandomJoinFailed");
		//PhotonNetwork.CreateRoom( null );
	}

	void OnJoinedRoom() {
		Debug.Log ("OnJoinedRoom");
		connecting = false;
		//SpawnMyPlayer();
	}

	void SpawnMyPlayer(int teamID) {
		this.teamID = teamID;
		hasPickedTeam = true;
		AddChatMessage("Spawning Player: " + PhotonNetwork.player.name);

		if(spawnSpots == null) {
			Debug.LogError ("No spawn positions exist");
			return;
		}

		SpawnSpot mySpawnSpot = spawnSpots[ Random.Range (0, spawnSpots.Length) ];
		GameObject myPlayerGO = (GameObject)PhotonNetwork.Instantiate("PlayerController", mySpawnSpot.transform.position, mySpawnSpot.transform.rotation, 0);
		standbyCamera.SetActive(false);
		//Crosshairs.SetActive (true);

		//((MonoBehaviour)myPlayerGO.GetComponent("FPSInputController")).enabled = true;
			((MonoBehaviour)myPlayerGO.GetComponent ("MouseLook")).enabled = true;
			((MonoBehaviour)myPlayerGO.GetComponent ("PlayerMovement")).enabled = true;
			((MonoBehaviour)myPlayerGO.GetComponent ("PlayerShooting")).enabled = true;

		myPlayerGO.GetComponent<PhotonView>().RPC ("SetTeamID", PhotonTargets.AllBuffered, teamID);

		myPlayerGO.transform.FindChild("Main Camera").gameObject.SetActive(true);
		Screen.lockCursor = true;
	}

	void Update() {
		if (respawnTimer > 0) {
			respawnTimer -= Time.deltaTime;

			if(respawnTimer <= 0) {
				// Time to respawn the player!
				SpawnMyPlayer(teamID);
			}
		} 
		if (Input.GetKeyDown ("escape")) {
			if (Screen.lockCursor == false) {
				Screen.lockCursor = true;
				mainMenu = false;
				Debug.Log("Locking cursor");
			} else {
				Screen.lockCursor = false;
				mainMenu = true;
				Debug.Log("Unlocking cursor");
			}
		}
	}
}

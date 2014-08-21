using UnityEngine;
using System.Collections;

public class NetworkCharacter : Photon.MonoBehaviour {

	Vector3 realPosition = Vector3.zero;
	Quaternion realRotation = Quaternion.identity;
	float realAimAngle = 0;

	Animator anim;

	bool gotFirstUpdate = false;

	// Use this for initialization
	void Start () {
		CacheComponents();
	}

	void CacheComponents() {
		if (anim == null) {
			anim = GetComponent<Animator>();
			if(anim == null) {
				Debug.LogError ("No animator component on prefab.");
			}
		}

		// Cache more components here if required.
	}
	
	// Update is called once per frame
	void Update () {
		if( photonView.isMine ) {
			// Do nothing -- the character motor/input/etc... is moving us
		}
		else {
			transform.position = Vector3.Lerp(transform.position, realPosition, 0.1f);
			transform.rotation = Quaternion.Lerp(transform.rotation, realRotation, 0.1f);
			anim.SetFloat("AimAngle", Mathf.Lerp(anim.GetFloat("AimAngle"), realAimAngle, 0.1f));
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info) {
		CacheComponents();

		if(stream.isWriting) {
			// This is OUR player. We need to send our actual position to the network.

			stream.SendNext(transform.position);
			stream.SendNext(transform.rotation);
			stream.SendNext(anim.GetFloat("SpeedV"));
			stream.SendNext(anim.GetFloat("SpeedH"));
			stream.SendNext(anim.GetFloat("AimAngle"));
			stream.SendNext(anim.GetBool("Jumping"));
		}
		else {
			// This is someone else's player. We need to receive their position (as of a few
			// millisecond ago, and update our version of that player.

			// Right now, "realPosition" holds the other person's position at the LAST frame.
			// Instead of simply updating "realPosition" and continuing to lerp,
			// we MAY want to set our transform.position immediately to this old "realPosition"
			// and then update realPosition


			realPosition = (Vector3)stream.ReceiveNext();
			realRotation = (Quaternion)stream.ReceiveNext();
			anim.SetFloat("SpeedV", (float)stream.ReceiveNext());
			anim.SetFloat("SpeedH", (float)stream.ReceiveNext());
			realAimAngle = (float)stream.ReceiveNext();
			anim.SetBool("Jumping", (bool)stream.ReceiveNext());

			if(gotFirstUpdate == false) {
				transform.position=realPosition;
				transform.rotation=realRotation;
				anim.SetFloat("AimAngle", realAimAngle);
				gotFirstUpdate = true;
			}
		}

	}
}

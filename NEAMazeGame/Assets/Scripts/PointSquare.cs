using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PointSquare : MonoBehaviour
{

    public PhotonView photonView; // represents an entity in a network

    public int scoreValue = 100; // how much collecting a point will add to score

    // when another collider 2d collides with this one
    private void OnTriggerEnter2D(Collider2D collision) {

        if(!photonView.IsMine) return; // only check for current entity

        if(collision.CompareTag("Player")){ // if player
            if(!collision.GetComponent<PlayerMovement>().isDead){
                // add score for all players
                collision.GetComponent<PlayerMovement>().photonView.RPC("AddScore", RpcTarget.All, scoreValue);
                PhotonNetwork.Destroy(photonView); // then destroy this point square
            }
        }
    }

}

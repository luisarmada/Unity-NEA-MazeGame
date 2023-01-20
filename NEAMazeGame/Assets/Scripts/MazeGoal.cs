using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MazeGoal : MonoBehaviour
{

    private bool goalReached = false; // has a player touched the goal

    // reference to the PlayerMovement script of the player who touches the goal
    private PlayerMovement playerScript; 

    // collision check
    private void OnTriggerEnter2D(Collider2D collision) {

        if(collision.CompareTag("Player") && !goalReached){ // is player and goal has not been called already
            if(!collision.GetComponent<PlayerMovement>().isDead){
                playerScript = collision.GetComponent<PlayerMovement>(); // set player script
                goalReached = true;

                // add message to indicate in chat someone has reached the goal
                collision.GetComponent<ChatController>().photonView.RPC("addMessageToFeed", RpcTarget.All, playerScript.photonView.Owner.NickName + " has reached the goal!");
                collision.GetComponent<ChatController>().photonView.RPC("addMessageToFeed", RpcTarget.All, "Starting next stage in 5 seconds..");
                StartCoroutine("LoadNextStage"); // start countdown before loading next stage
            }
        }
    }

    IEnumerator LoadNextStage(){
        yield return new WaitForSeconds(5f); // wait 5 seconds before loading next stage
        if(PhotonNetwork.IsMasterClient){ // only master client should load levels

            // update player prefs so score and stage number carries across scenes
            PlayerPrefs.SetInt("RecentScore", playerScript.gameScore);
            playerScript.photonView.RPC("IncrementStage", RpcTarget.All);
            PhotonNetwork.LoadLevel("Maze");
        }
    }

}

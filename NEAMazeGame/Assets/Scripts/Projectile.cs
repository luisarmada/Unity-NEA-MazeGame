using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Projectile : MonoBehaviour
{

    // photonView is used to identify an object across the network
    public PhotonView photonView;

    // how far outside of the maze bounds before the projectile is destroyed
    private float destroyBounds = 10f;

    // how much damage to player health the projectile should do
    public float damage = 30f;

    // Update is called once per frame
    void Update()
    {

        if(!photonView.IsMine) return; // only call for current projectile

        // check to see if projectile is out of maze outer walls bounds + destroyBounds offset
        if(transform.position.x < -destroyBounds || transform.position.x > (MazeGenerator.xTiles * MazeGenerator.tileScale) + destroyBounds
            || transform.position.y > destroyBounds || transform.position.y < (-MazeGenerator.yTiles * MazeGenerator.tileScale) - destroyBounds){
                PhotonNetwork.Destroy(photonView); // destroy projectile from the network
            }
    }

    // check collider 2d overlaps
    private void OnTriggerEnter2D(Collider2D collision) {

        if(!photonView.IsMine) return;

        if(collision.CompareTag("Player")){ // is player?
            // ensures player is not dead and is not dashing
            if(!collision.GetComponent<PlayerMovement>().isDead && !(collision.GetComponent<PlayerMovement>().dashCounter > 0)){
                collision.GetComponent<PlayerMovement>().TakeDamage(damage); // deal damage to overlapping player
                PhotonNetwork.Destroy(photonView); // then destroy to avoid constant damage dealing
            }
        }
    }

}

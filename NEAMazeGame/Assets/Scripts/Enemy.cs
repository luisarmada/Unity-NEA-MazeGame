using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Enemy : MonoBehaviour
{

    // photonView is used to identify an object across the network
    public PhotonView photonView;

    // bool used to determine when projectiles should be shot, 
    // used as a do once conditon
    private bool canShootProjectiles = true;

    float shootDelay = 2f; // delay between enemy firing projectiles

    public int tileX, tileY; // the position of the enemy in the level

    // Update is called once per frame
    void Update()
    {
        if(!photonView.IsMine) return; // only call for current enemy

        if(canShootProjectiles){
            canShootProjectiles = false; // set to false so projectile firing only called once
            ShootProjectiles();
            StartCoroutine("StartShootDelay"); // start coroutine for setting flag back to true
        }
    }

    private void ShootProjectiles(){
        for(int i=0; i < Random.Range(5, 10); i++){ // fires 5 to 10 projectiles
            GameObject proj = PhotonNetwork.InstantiateRoomObject("Projectile", new Vector2(tileX * MazeGenerator.tileScale, -tileY * MazeGenerator.tileScale), Quaternion.identity);
            float projAngle = Random.Range(0f, 360f); // shoot each projectile at a random angle
            float projSpeed = 8f; // sets velocity of projectile
            proj.GetComponent<Rigidbody2D>().velocity = new Vector2 (Mathf.Sin(projAngle), Mathf.Cos(projAngle)) * projSpeed; // uses vector math to determine direction and velocity
        }
    }

    IEnumerator StartShootDelay(){ // a countdown between firing projectiles
        yield return new WaitForSeconds(shootDelay); // a set delay to encourage strategising
        canShootProjectiles = true; // flag set to true after delay
    }

    // run whenever anything with a collider 2d overlaps the enemy
    private void OnTriggerEnter2D(Collider2D collision) {

        if(!photonView.IsMine) return; // check for current enemy

        if(collision.CompareTag("Player")){ // if enemy collides with player, kill (destroy) this enemy
            if(!collision.GetComponent<PlayerMovement>().isDead){ // ensure the player is not dead
                PhotonNetwork.Destroy(photonView);
            }
        }
    }
    
}

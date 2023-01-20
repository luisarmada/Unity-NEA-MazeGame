using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class PlayerMovement : MonoBehaviour
{

    public float moveSpeed = 6;
    public Rigidbody2D rb;
    private Vector2 moveDirection;

    private float activeMoveSpeed; // current speed

    public float dashSpeed;
    public float dashLength = .4f; // dash duration
    public float dashCooldown = 1f;
    public float dashCounter; // used to count how long dash lasts
    private float dashCoolCounter; // used to count dash cooldown

    public GameObject playerCamera; // referencing to the child camera component of the sprite
    public TextMeshProUGUI playerNameText; // the username entered will display as a text above the user sprite

    // photonView is used to identify an object across the network
    public PhotonView photonView;

    public float maxHealth = 100f; // also starting health
    public float currentHealth;
    public bool isDead = false;

    // when the player dies, they turn into a ghost which is able to move around the map freely
    // until all players die (game over). deadSpeed is movement speed when the player is a ghost
    public float deadSpeed = 12f; 
    
    public int numPlayersDead = 0; // used to check for game over

    public int currentStage = 1; // stage number
    public int gameScore = 0; // current score

    public BoxCollider2D col;
    public SpriteRenderer spriteRenderer;

    private void Awake(){ // Called before game starts, used for initialising
        if(photonView.IsMine){ // if the photon view of the created player belongs to the user
            playerCamera.SetActive(true); // activates attached camera
        }
        playerNameText.text = photonView.Owner.NickName; // sets the username text displayed above sprite
    }

    void Start(){
        if(photonView.IsMine){
            activeMoveSpeed = moveSpeed;
            currentHealth = maxHealth;
        }
        currentStage =  PlayerPrefs.GetInt("CurrentStage");
        if(GameObject.Find("StageText") != null){
            GameObject.Find("StageText").GetComponent<TextMeshProUGUI>().text = "STAGE " + currentStage;
        }
        gameScore = PlayerPrefs.GetInt("RecentScore");
    }

    [PunRPC]
    public void AddScore(int score){
        gameScore += score;
        // set text on hud to show current score
        GameObject.Find("ScoreText").GetComponent<TextMeshProUGUI>().text = gameScore + " PTS";
    }

    [PunRPC]
    public void IncrementPlayerDead(){
        numPlayersDead += 1;
    }

    [PunRPC]
    public void IncrementStage(){
        currentStage += 1;

        // update player pref to save current stage number
        PlayerPrefs.SetInt("CurrentStage", currentStage);
    }

    public void TakeDamage(float dmg){
       
       if(!photonView.IsMine) return; // only deal damage to current player

        if(dashCounter > 0) return; // only deal damage if not dashing

        // ensures health does not go below 0
        currentHealth = Mathf.Max(currentHealth - dmg, 0f);

        // update health bar value
        GameObject.Find("HealthBar").GetComponent<Slider>().value = currentHealth/maxHealth;

        if(currentHealth <= 0){ // has player been damaged to 0?
            isDead = true;
            playerNameText.text = photonView.Owner.NickName + " (DEAD)"; // append dead to nickname to show other players
            col.enabled = false; // disable player collision in ghost mode
            moveSpeed = deadSpeed; // set speed to designated dead speed
            spriteRenderer.color = new Color(1, 1, 1, 0.6f); // make sprite slightly transparent to indicate ghost

            // print message in chat to show dead
            GetComponent<ChatController>().photonView.RPC("addMessageToFeed", RpcTarget.All, PhotonNetwork.NickName + " has died!");
            photonView.RPC("IncrementPlayerDead", RpcTarget.All); // add 1 to player dead count
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(photonView.IsMine){ // encapsulate the update function to only call this update for current user
            float moveX = Input.GetAxis("Horizontal");
            float moveY = Input.GetAxis("Vertical");

            moveDirection = new Vector2(moveX, moveY);

            if(Input.GetKeyDown(KeyCode.Space) && !isDead){ // bind dash to space bar
                if(dashCoolCounter <= 0 && dashCounter <= 0){ // can dash check
                    activeMoveSpeed = dashSpeed; // dash -> move faster
                    dashCounter = dashLength; // start dash timer
                }
            }

            if(dashCounter > 0){
                dashCounter -= Time.deltaTime; //countdown dash length

                if(dashCounter <= 0){ // end dash
                    activeMoveSpeed = moveSpeed;
                    dashCoolCounter = dashCooldown; // start dash cooldown timer
                }
            }

            if(dashCoolCounter > 0 ){ // countdown dash timer
                dashCoolCounter -= Time.deltaTime;
            }
        }
    }

    // Update() gets called per frame, so as frames may vary, it is inconsistent
    // FixedUpdate() gets called a set amount of times per update loop, so it is consistent
    // Update is used for processing inputs, FixedUpdate is for physiscs calculations
    // To keep movement, physics, etc, smooth
    void FixedUpdate(){
       Vector2 moveVelocity = new Vector2(moveDirection.x * activeMoveSpeed, moveDirection.y * activeMoveSpeed);
        if(photonView.IsMine){ // only move our player
            transform.position += (Vector3)moveVelocity * Time.fixedDeltaTime; // change transform in accordance with frame updates
        }
    }
}


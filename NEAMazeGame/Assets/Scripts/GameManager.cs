using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class GameManager : MonoBehaviourPunCallbacks
{

    private GameObject createdPlayer;// this script will instantiate the player, this variable can be used to refer to it
    [SerializeField] private GameObject sceneCamera; // default main camera in the scene, not the player one.

    [SerializeField] private TextMeshProUGUI pingText, roomCodeText, playerCountText; // the text components on the scene

    [SerializeField] private GameObject inGameOptions; // pause window parent, used for setting window visibility

    [SerializeField] private GameObject startGameButton;

    [SerializeField] private GameObject playerHealthBar;

    public bool isLobby = true;

    private int playerCount, maxPlayer;

    private bool isGameOver = false;

    public void Start(){
        sceneCamera.SetActive(false); // disable the main camera, as player camera will activate instead
        
        // create the player prefab in the lobby when the player joins
        createdPlayer = PhotonNetwork.Instantiate("Player", new Vector2(1.3f, -1.3f), Quaternion.identity, 0);

        if(isLobby){
            playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
            maxPlayer = PhotonNetwork.CurrentRoom.MaxPlayers;
            playerCountText.text = playerCount + "/" + maxPlayer + " Players"; // show player count
            roomCodeText.text = "Room Code: " + PhotonNetwork.CurrentRoom.Name; // the room code is the same as the room name
            allowStartIfMaster();
        } else {
            playerCountText.gameObject.SetActive(false);
            roomCodeText.gameObject.SetActive(false);
        }
        

        inGameOptions.SetActive(false);
    }

    private void Update(){
        pingText.text = PhotonNetwork.GetPing() + "ms"; // update ping text
        if(Input.GetKeyUp(KeyCode.Escape)){ // toggle in-game options menu when escape button clicked
            inGameOptions.SetActive(!inGameOptions.activeSelf); 
        }

        // check to see if in game, and not game over
        if(!isLobby && PhotonNetwork.IsMasterClient && !isGameOver){ 
            // is number of players dead = number of players in game?
            if(createdPlayer.GetComponent<PlayerMovement>().numPlayersDead == PhotonNetwork.CurrentRoom.PlayerCount){
                isGameOver = true; // if so, then game over
                // update score in player prefs
                PlayerPrefs.SetInt("RecentScore", createdPlayer.GetComponent<PlayerMovement>().gameScore);
                LeaveRoom(); // leave the game
            }
        }
        
    }



    // shows start game button if current client is the master/creator/owner of room
    private void allowStartIfMaster(){
        if(PhotonNetwork.IsMasterClient){
            startGameButton.SetActive(true);
        } else{
            startGameButton.SetActive(false);
        }
    }

    public void startGamePressed(){ // start button on click
        StartCoroutine("startGameCountdown");
        
        // Closes the room so no more players can join once the game starts
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;
    }

    IEnumerator startGameCountdown(){ // load maze level after countdown
        // for loop used as timer in order to display the countdown onto text
        for(float countdown = 5; countdown> 0; countdown -= Time.deltaTime){
            playerCountText.text = "Starting Game in " + Mathf.Ceil(countdown) + "...";
            yield return null;
        }
        if(PhotonNetwork.IsMasterClient){ // load level after countdown is complete
            PhotonNetwork.LoadLevel("Maze");
        }
    }

    // run when resume button clicked in in-game pause window
    public void ClosePauseWindowMenu(){ 
        inGameOptions.SetActive(false); 
    }

    public void LeaveRoom(){ // run when leave button clicked in pause window
        sceneCamera.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom(){ // run after leaving room
        // gives outgoing commands from this player priority so they are complete before the player leaves
        PhotonNetwork.SendAllOutgoingCommands();
        if(isGameOver){
            PhotonNetwork.LoadLevel("GameOverScene");
        } else {
            PhotonNetwork.LoadLevel("MainMenuScene"); // return to main menu
        }
        
        base.OnLeftRoom(); // calls photon functions for leaving a room
    }

    public void OnApplicationQuit(){ // run if application exits while in room
        PhotonNetwork.SendAllOutgoingCommands();
        PhotonNetwork.LeaveRoom(); // ensures the player leaves the room
    }

    // ensures that disconnection is smooth
    public override void OnDisconnected(DisconnectCause cause){
        base.OnDisconnected(cause);
    }

    public void toggleFullscreen(){
        Screen.fullScreen = !Screen.fullScreen;
        if(Screen.fullScreen){
            Debug.Log("Fullscreen mode has been turned on");
        } else {
            Debug.Log("Fullscreen mode has been turned off");
        }
    }

    public override void OnPlayerEnteredRoom(Player player){
        createdPlayer.GetComponent<ChatController>().addMessageToFeed(player.NickName + " has joined the game");
        playerCount++;
        if(isLobby){
            playerCountText.text = playerCount + "/" + maxPlayer + " Players"; // show player count
        }
        
    }

    public override void OnPlayerLeftRoom(Player player){
        createdPlayer.GetComponent<ChatController>().addMessageToFeed(player.NickName + " has left the game");
        playerCount--;
        if(isLobby){
            playerCountText.text = playerCount + "/" + maxPlayer + " Players"; // show player count
            allowStartIfMaster();
        }
    }

}

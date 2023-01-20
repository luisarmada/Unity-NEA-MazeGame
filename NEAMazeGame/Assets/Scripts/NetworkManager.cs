using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class NetworkManager : MonoBehaviourPunCallbacks
{

    // version of the game. used for compatibility
    //e.g. version 1 players cannot play with v2 players, etc.
    [SerializeField] private string gameVersion = "1";

    [SerializeField] private byte maxPlayersPerRoom = 4;
    [SerializeField] private byte roomCodeLength = 6; // used for generating room code

    [SerializeField] private TMP_InputField userNameInput; // username input box in lobby finder scene
    [SerializeField] private TMP_InputField joinGameInput; // game code input box in lobby finder scene
    

    [SerializeField] private GameObject ProgressLabel; // "Connecting..."
    [SerializeField] private GameObject InvalidUsernameLabel; // "Invalid Username!"
    [SerializeField] private GameObject InvalidGameCodeLabel; // "Invalid Game Code!"

    [SerializeField] private string joinLevelName; // level(scene) name to load when entering game

    void Start(){

        // Initialise and setup photon network usage
        PhotonNetwork.AutomaticallySyncScene = true; // determines if the client should follow the master client when loading scenes
                                                        // this will be useful for game loading, so value is set to true
        if(!PhotonNetwork.IsConnected){ // force connection to server
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.GameVersion = gameVersion;
        }

        ProgressLabel.SetActive(false);
        InvalidUsernameLabel.SetActive(false);
        InvalidGameCodeLabel.SetActive(false);
        
        // Runs function ChangeUsername() whenever username input box is changed 
        userNameInput.onValueChanged.AddListener(delegate{ChangeUsername();});

        // username input validation - username can only contain letters and spaces, and maximum 16 characters
        // username cannot start in a space character
        userNameInput.onValidateInput += delegate (string s, int i, char c)
            {
                if (s.Length >= 16) { return '\0'; }
                return (char.IsLetter(c) || (c.ToString() == " " && s.Length > 0 && s[i-1].ToString() != " ")) ? c : '\0';
            };


        // join game input validation and sanitisation, letters can only be inputted 
        // and are automatically capitalised
        joinGameInput.onValidateInput += delegate (string s, int i, char c)
        {
            if (s.Length >= roomCodeLength) { return '\0'; }
            c = char.ToUpper(c);
            return char.IsLetter(c) ? c : '\0';
        };

    }

    private void ChangeUsername(){ // on username input field changed
        PhotonNetwork.NickName = userNameInput.text; // updates username in photon settings
        // saves username in a playerpref
        PlayerPrefs.SetString("PlayerName", userNameInput.text);
        InvalidUsernameLabel.SetActive(false); // reset invalid username label
    }

    private void invalidUsername(){
        Debug.Log("Username is invalid!");
        InvalidUsernameLabel.SetActive(true); // show invalid username text
    }

    public void CreateGame(){ // Run when Create Game button is clicked

        // Check if username exists in input field
        if(userNameInput.text.Length == 0){
            invalidUsername();
            return;
        }

        string roomCode =  generateUniqueRoomCode(); // generates room code
        Debug.Log("Successfully created room with code " + roomCode);

        // initialise new room using Photon
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = maxPlayersPerRoom;
        roomOptions.CleanupCacheOnLeave = true; // automatically delete player created objects on leave
        PhotonNetwork.CreateRoom(roomCode, roomOptions); // create room using options and room code as name
 
        // show "Connecting..." text
        ProgressLabel.SetActive(true);
    }

    // Generates a 6-letter capitalised code to use as the room code
    // e.g. ABCDEF, LAOBEF, AVHWDF
    private string generateUniqueRoomCode(){
        string possibleChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string roomName = "";
        for(int i=0; i<roomCodeLength; i++){
            char c = possibleChars[Random.Range(0, possibleChars.Length)];
            roomName = roomName + c;
        }
        return roomName;
    }

    public override void OnJoinedRoom(){
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room.");
        PhotonNetwork.LoadLevel(joinLevelName); // loads level using level name.
        // loading using photon network allows for players to connect to eachother
    }

    public override void OnDisconnected(DisconnectCause cause) {
        Debug.LogWarningFormat("OnDisconnected() was called by PUN with reason {0}", cause);
        if(ProgressLabel != null)ProgressLabel.SetActive(false); // hides connecting text
    }

    public void FindGame(){ // run when find game button is clicked

        // Check if username exists
        if(userNameInput.text.Length == 0){
            invalidUsername();
            return;
        }

        ProgressLabel.SetActive(true);
        PhotonNetwork.JoinRandomRoom(); // utilises photon network function
    }

    public override void OnJoinRandomFailed(short returnCode, string message){
        Debug.Log("OnJoinRandomFailed() was called by PUN. No random room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
        CreateGame(); // if cant find a room, create a new one instead
    }

    public void JoinGame(){ // run when join game button is clicked
        // Check if username exists
        if(userNameInput.text.Length == 0){
            invalidUsername();
            return;
        }

        // check if join game input = room code length (6)
        if(joinGameInput.text.Length != roomCodeLength){
            Debug.Log("Room code invalid!");
            InvalidGameCodeLabel.SetActive(true); // show invalid code label
            return;
        } else {
            InvalidGameCodeLabel.SetActive(false);
        }

        // join room if the code exists, if not create a new room with the code inputted
        PhotonNetwork.JoinOrCreateRoom(joinGameInput.text, new RoomOptions{ MaxPlayers = maxPlayersPerRoom }, TypedLobby.Default);

        ProgressLabel.SetActive(true); // Connecting...
    }

}

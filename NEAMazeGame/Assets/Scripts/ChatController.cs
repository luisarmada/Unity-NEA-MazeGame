using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class ChatController : MonoBehaviour
{

    private GameObject chatInput;
    private GameObject chatFeed;

    [SerializeField] private GameObject chatMessagePrefab;

    [SerializeField] public PhotonView photonView;

    private int maxMessages = 6;
    private Queue<GameObject> chatQueue = new Queue<GameObject>();

    void Awake(){
        // finds component in scene named 'ChatInput'
        chatInput = GameObject.Find("ChatInput");
        chatFeed = GameObject.Find("ChatFeed");
    }

    void Start(){
        if(!photonView.IsMine) return; // check if current player = client
            
        chatInput.SetActive(false); // hide input field
        chatFeed.SetActive(false); // hide input field
    }

    // Update is called once per frame
    void Update()
    {
        if(!photonView.IsMine) return; // check if current player = client

        if(Input.GetKeyUp(KeyCode.Return)){ // escape key press check

            if(!chatInput.activeSelf){ // is chat input field not visible?
                chatInput.SetActive(true);
                chatFeed.SetActive(true);
                chatInput.GetComponent<TMP_InputField>().ActivateInputField(); // set to visible and enable input
            } else {
                if(chatInput.GetComponent<TMP_InputField>().text != ""){ // ensure chat input is not empty

                    // call RPC event to all clients in the game, all clients will run addMessageToFeed() with the inputted message as a parameter.
                    photonView.RPC("addMessageToFeed", RpcTarget.All, PhotonNetwork.NickName + ": " + chatInput.GetComponent<TMP_InputField>().text);

                    chatInput.GetComponent<TMP_InputField>().text = ""; // reset the input field
                } else {
                    StartCoroutine("HideMessages");
                }
                chatInput.SetActive(false); // hide input field
            }
        }

    }

    [PunRPC]
    public void addMessageToFeed(string message){

        if(chatQueue.Count == maxMessages){ // remove object at front of queue if above max messages
            Destroy(chatQueue.Dequeue());
        }

        GameObject obj = Instantiate(chatMessagePrefab, new Vector2(0,0), Quaternion.identity); // create chat message component
        chatQueue.Enqueue(obj); // add object to back of queue
        obj.transform.SetParent(chatFeed.transform, false); // parent new component to child feed box
        obj.GetComponent<TextMeshProUGUI>().text = message; // set display message
        
        chatFeed.SetActive(true);
        StartCoroutine("HideMessages");
    }

    IEnumerator HideMessages(){
        yield return new WaitForSeconds(100f);
        chatFeed.SetActive(false);
    }

}

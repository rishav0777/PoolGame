using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Photon.Pun;
using Unity.VisualScripting;
using System.Collections;
using UnityEngine.SceneManagement;

public class PhotonConnections : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] NextGameObject;
    [SerializeField] private GameObject CurrentGameObject;
    [SerializeField] private TextMeshProUGUI debug;
    //[SerializeField] private ContestJoining contestJoining;
    [SerializeField] private FindingTimer SmileAnimation;

    private bool gameFlag = false;
    
    public void StartFinding()
    {
        Debug.Log("Connecting to Photon Master Server");
        DataSaver.Instance.gameMode = 0;
        try
        {PhotonNetwork.ConnectUsingSettings();
        }
        catch (Exception e) { print("hh " + e); }
    }
    private void Update()
    {
        if (gameFlag)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
                StartCoroutine(JoinGame());
            else if(SmileAnimation.Findingtime<=5){
               DataSaver.Instance.gameMode=1;
               StartCoroutine(JoinGame());
            }
        }
         else if(SmileAnimation.Findingtime <= 5){
              DataSaver.Instance.gameMode=1;
             StartCoroutine(JoinGame());
         }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon Master Server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Connected to Photon lobby");
        debug.text = debug.text + "lobbby";
        PhotonNetwork.JoinRandomRoom();
        base.OnJoinedLobby();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRandomFailed");
        debug.text = debug.text + "OnJoinRandomFailed";
        string RoomName = UnityEngine.Random.Range(11111, 99999).ToString();
        
        PhotonNetwork.CreateRoom(RoomName);
        base.OnJoinRandomFailed(returnCode, message);
    }



    public override void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom()");
        PhotonNetwork.JoinRandomRoom();
        base.OnCreatedRoom();
    }

    public override void OnJoinedRoom()
    {
        debug.text = debug.text+"OnJoinedRoom()" + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log("OnJoinedRoom()" + PhotonNetwork.CurrentRoom.Name + " " + PhotonNetwork.CurrentRoom.PlayerCount);
        gameFlag = true;

        base.OnJoinedRoom();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("OnJoinRoomFailed");
        //StartCoroutine(JoinGame());
        base.OnJoinRoomFailed(returnCode, message);
    }

    public void OnClick()
    {

    }



    IEnumerator JoinGame()
    {
        //Debug.Log("actor no ")
        // Debug.Log("Join Game()");
        //contestJoining.ContestJoined();
        yield return new WaitForSeconds(3.0f);
        for(int j=0;j<NextGameObject.Length;j++) NextGameObject[j].SetActive(true);
        CurrentGameObject.SetActive(false);
    }





}

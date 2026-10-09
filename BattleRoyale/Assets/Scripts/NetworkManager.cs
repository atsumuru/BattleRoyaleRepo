using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    public int maxPlayers = 10;
    public bool isConnectedToMaster = false;

    // instance
    public static NetworkManager instance;
    void Awake ()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        // instance = this;
        // DontDestroyOnLoad(gameObject);

        if (instance != null && instance != this)
            gameObject.SetActive(false);
        else
        {
            // set the instance
            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // not here :(
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // connect to the master server
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster ()
    {
        isConnectedToMaster = true;

        Debug.Log("We've connected to the master server!");
    }

// attempts to create a room
    public void CreateRoom (string roomName)
    {
        if (!isConnectedToMaster || PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        {
            Debug.LogWarning("Photon is not ready. Wait for connection to the Master Server.");
            return;
        }

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = (byte)maxPlayers;
        PhotonNetwork.CreateRoom(roomName, options);
    }

    //attempts to join a room
    public void JoinRoom (string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    [PunRPC]
    public void ChangeScene (string sceneName)
    {
        PhotonNetwork.LoadLevel(sceneName);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        isConnectedToMaster = false;

        PhotonNetwork.LoadLevel("Menu");
    }

    public override void OnPlayerLeftRoom (Player otherPlayer)
    {
        GameManager.instance.alivePlayers--;
        GameUI.instance.UpdatePlayerInfoText();
        
        if(PhotonNetwork.IsMasterClient)
        {
            GameManager.instance.CheckWinCondition();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

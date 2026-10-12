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
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster ()
    {
        isConnectedToMaster = true;

        Debug.Log("We've connected to the master server!");

        // Join the lobby so room listings can be received
        if (!PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
        {
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Successfully joined the Photon Lobby!");
    }

// attempts to create a room
    public void CreateRoom (string roomName)
    {
        if (!PhotonNetwork.IsConnectedAndReady ||
            PhotonNetwork.NetworkClientState != ClientState.JoinedLobby)
        {
            Debug.LogWarning("Photon is not ready or has not joined the lobby.");
            return;
        }

        if (string.IsNullOrWhiteSpace(roomName))
        {
            Debug.LogWarning("Room name cannot be empty.");
            return;
        }

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = (byte)maxPlayers;
        options.IsVisible = true;
        options.IsOpen = true;

        PhotonNetwork.CreateRoom(roomName.Trim(), options);

        // if (!isConnectedToMaster || PhotonNetwork.NetworkClientState != ClientState.ConnectedToMasterServer)
        // {
        //     Debug.LogWarning("Photon is not ready. Wait for connection to the Master Server.");
        //     return;
        // }

        // RoomOptions options = new RoomOptions();
        // options.MaxPlayers = (byte)maxPlayers;
        // PhotonNetwork.CreateRoom(roomName, options);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Room created successfully: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Room creation failed: " + message);
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

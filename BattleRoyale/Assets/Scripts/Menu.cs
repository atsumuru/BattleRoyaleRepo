using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class Menu : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    // declared variables

    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject createRoomScreen;
    public GameObject lobbyScreen;
    public GameObject lobbyBrowserScreen;

    [Header("Main Screen")]
    public Button createRoomButton;
    public Button findRoomButton;

    [Header("Lobby")]
    public TextMeshProUGUI playerListText;
    public TextMeshProUGUI roomInfoText;
    public Button startGameButton;

    [Header("Lobby Browser")]
    public RectTransform roomListContainer;
    public GameObject roomButtonPrefab;

    private bool refreshingRooms = false;

    private List<GameObject> roomButtons = new List<GameObject>();
    private List<RoomInfo> roomList = new List<RoomInfo>();

    void Start()
    {
        // disable the menu buttons at the start
        createRoomButton.interactable = false;
        findRoomButton.interactable = false;

        // enable the cursor since we hide it when we play the game
        Cursor.lockState = CursorLockMode.None;

        // are we in a game?
        if(PhotonNetwork.InRoom)
        {
            // go to the lobby
            SetScreen(lobbyScreen);
            UpdateLobbyUI();

            // make the room visible
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }

// changes the currently visible screen
    void SetScreen (GameObject screen)
    {
        // disable all other screens
        mainScreen.SetActive(false);
        createRoomScreen.SetActive(false);
        lobbyScreen.SetActive(false);
        lobbyBrowserScreen.SetActive(false);

        // activate the requested screen
        screen.SetActive(true);

        if(screen == lobbyBrowserScreen)
        UpdateLobbyBrowserUI();
    }

// -------------------------------
// -------- Main Screen ----------
// -------------------------------

    public void OnPlayerNameValueChanged (TMP_InputField playerNameInput)
    {
        PhotonNetwork.NickName = playerNameInput.text.Trim();

        Debug.Log("Player nickname set to: " + PhotonNetwork.NickName);
    }

    public override void OnConnectedToMaster ()
    {
        // // enable the menu buttons once we connect to the server
        // createRoomButton.interactable = true;
        // findRoomButton.interactable = true;

        // Join the lobby to receive available room updates
        if (!PhotonNetwork.InLobby && !PhotonNetwork.InRoom)
        {
            PhotonNetwork.JoinLobby();
        }
    }

// called when the "Create Room" button has been pressed.
    public void OnCreateRoomButton ()
    {
        SetScreen(createRoomScreen);

        // works
    }

    public void OnFindRoomButton()
    {
        SetScreen(lobbyBrowserScreen);

        if (PhotonNetwork.InLobby)
        {
            UpdateLobbyBrowserUI();
        }
        else if (PhotonNetwork.IsConnectedAndReady && !PhotonNetwork.InRoom)
        {
            roomList.Clear();
            UpdateLobbyBrowserUI();
            PhotonNetwork.JoinLobby();
        }
    }

// // called when the "Find Room" button has been pressed
//     public void OnFindRoomButton ()
//     {
//         SetScreen(lobbyBrowserScreen);
//     }

// -------------------------------
// ----- Create Room Screen ------
// -------------------------------

// called when the "Back" button gets pressed
    public void OnBackButton ()
    {
        SetScreen(mainScreen);
    }

    public void OnCreateButton (TMP_InputField roomNameInput)
    {
        NetworkManager.instance.CreateRoom(roomNameInput.text);
    }

// -------------------------------
// -------- Lobby Screen ---------
// -------------------------------

    public override void OnJoinedRoom ()
    {
        SetScreen(lobbyScreen);
        UpdateLobbyUI();
        // photonView.RPC("UpdateLobbyUI", RpcTarget.All);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player joined the room: " + newPlayer.NickName);

        UpdateLobbyUI();
    }

    public override void OnPlayerLeftRoom (Player otherPlayer)
    {
        Debug.Log("Player left the room: " + otherPlayer.NickName);
        
        UpdateLobbyUI();
    }

    void UpdateLobbyUI ()
    {
        // enable or disable the start game button depending on if we're the host
        startGameButton.interactable = PhotonNetwork.IsMasterClient;
        
        // display all the players
        playerListText.text = "";
        foreach(Player player in PhotonNetwork.PlayerList)
            playerListText.text += player.NickName + "\n";

        // set the room info text
        roomInfoText.text = "<b>Room Name</b>\n" + PhotonNetwork.CurrentRoom.Name;
    }

    public void OnStartGameButton ()
    {
        // hide the room
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        // not here

        // tell everyone to load the game scene
        NetworkManager.instance.photonView.RPC("ChangeScene", RpcTarget.All, "Game");

        // here
        Debug.Log("I need you to work please.");
    }

    public void OnLeaveLobbyButton ()
    {
        PhotonNetwork.LeaveRoom();
        SetScreen(mainScreen);
    }

// -------------------------------
// ---- Lobby Browser Screen -----
// -------------------------------

    void UpdateLobbyBrowserUI ()
    {
        // disable all room buttons
        foreach(GameObject button in roomButtons)
        button.SetActive(false);

        // display all current rooms in the master server
        for(int x = 0; x < roomList.Count; ++x)
        {
            // get or create the button object
            GameObject button = x >= roomButtons.Count ? CreateRoomButton() : roomButtons [x];
            button.SetActive(true);

            // set the room name and player count texts
            button.transform.Find("RoomNameText").GetComponent<TextMeshProUGUI>().text = roomList[x].Name;
            button.transform.Find("PlayerCountText").GetComponent<TextMeshProUGUI>().text = roomList[x].PlayerCount + " / " + roomList[x].MaxPlayers;
            
            // set the button OnClick event
            Button buttonComp = button.GetComponent<Button>();
            string roomName = roomList[x].Name;
            buttonComp.onClick.RemoveAllListeners();
            buttonComp.onClick.AddListener(() => { OnJoinRoomButton(roomName); });
        }
    }

    GameObject CreateRoomButton ()
    {
        Debug.Log("this is working");
        
        GameObject buttonObj = Instantiate(roomButtonPrefab, roomListContainer.transform);
        roomButtons.Add(buttonObj);
        return buttonObj;
    }

    public void OnJoinRoomButton (string roomName)
    {
        NetworkManager.instance.JoinRoom(roomName);
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Successfully joined Photon Lobby.");

        createRoomButton.interactable = true;
        findRoomButton.interactable = true;

        if (lobbyBrowserScreen.activeSelf)
        {
            UpdateLobbyBrowserUI();
        }

        refreshingRooms = false;
    }

    public override void OnLeftLobby()
    {
        if (refreshingRooms && PhotonNetwork.IsConnectedAndReady)
        {
            roomList.Clear();
            PhotonNetwork.JoinLobby();
        }
    }

    public void OnRefreshButton()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogWarning("Cannot refresh: Photon is not ready.");
            return;
        }

        if (PhotonNetwork.InRoom)
        {
            Debug.LogWarning("Leave the current room before refreshing.");
            return;
        }

        if (PhotonNetwork.InLobby)
        {
            refreshingRooms = true;
            PhotonNetwork.LeaveLobby();
        }
        else
        {
            refreshingRooms = true;
            roomList.Clear();
            PhotonNetwork.JoinLobby();
        }
    }

    public override void OnRoomListUpdate(List<RoomInfo> allRooms)
    {
        foreach (RoomInfo room in allRooms)
        {
            int index = roomList.FindIndex(r => r.Name == room.Name);

            // Remove rooms that are no longer available
            if (room.RemovedFromList)
            {
                if (index != -1)
                    roomList.RemoveAt(index);
            }
            else
            {
                // Add new rooms or update existing rooms
                if (index == -1)
                    roomList.Add(room);
                else
                    roomList[index] = room;
            }
        }

        Debug.Log("Available rooms: " + roomList.Count);

        // Refreshes the browser if ti is currently visible
        if (lobbyBrowserScreen.activeSelf)
        {
            UpdateLobbyBrowserUI();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
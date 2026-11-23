using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class Menu : MonoBehaviourPunCallbacks, ILobbyCallbacks
{
    // declared variables

    [Header("Screens")]
    public GameObject mainScreen;
    public GameObject loginScreen;
    public GameObject joinScreen;
    public GameObject lobbyScreen;

    [Header("Main Screen")]
    public Button playButton;
    public Button creditsButton;

    [Header("Login Screen")]
    public TMP_InputField usernameInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button createButton;

    [Header("Join Screen")]
    public TMP_InputField roomCodeInput;
    public Button joinButton;
    public Button createRoomButton;

    [Header("Lobby Screen")]
    public TextMeshProUGUI playerListText;
    public TextMeshProUGUI roomListText;

    void Start()
    {
        // disable buttons here? or no?

        // enable the cursor since we hide it when we play the game
        Cursor.lockState = CursorLockMode.None;

        // are we in a game?
        if (PhotonNetwork.InRoom)
        {
            // go to the lobby
            // SetScreen(lobbyScreen);

            // make the room visible
            PhotonNetwork.CurrentRoom.IsVisible = true;
            PhotonNetwork.CurrentRoom.IsOpen = true;
        }
    }

    void SetScreen (GameObject screen)
    {
        // disable all screens
        mainScreen.SetActive(false);
        loginScreen.SetActive(false);
        joinScreen.SetActive(false);
        lobbyScreen.SetActive(false);

        // enable the requested screen
        screen.SetActive(true);
    }

    // -------------------------------
    // -------- Main Screen ----------
    // -------------------------------
}

using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System.Collections;

public class GameManager : MonoBehaviourPun
{
    [Header("Players")]
    public string playerPrefabLocation;
    public PlayerController[] players;
    public Transform[] spawnPoints;
    public int alivePlayers;

    private int playersInGame;

    public float postGameTime;

    [Header("Round Timer")]
    public float roundDuration = 60f;
    
    private double roundEndTime;
    private bool timerStarted;

    public float scoreboardDisplayTime = 10f;

    private bool roundEnding = false;

    // instance
    public static GameManager instance;

    void Awake ()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        players = new PlayerController[PhotonNetwork.PlayerList.Length];
        alivePlayers = players.Length;
        photonView.RPC("ImInGame", RpcTarget.AllBuffered);
        
        if (PhotonNetwork.IsMasterClient)
        {
            StartRoundTimer();
        }
    }

    [PunRPC]
    void ImInGame ()
    {
        playersInGame++;
        if(PhotonNetwork.IsMasterClient && playersInGame == PhotonNetwork.PlayerList.Length)
        photonView.RPC("SpawnPlayer", RpcTarget.All);
    }

    [PunRPC]
    void SpawnPlayer ()
    {
        GameObject playerObj = PhotonNetwork.Instantiate(playerPrefabLocation, spawnPoints[Random.Range(0, spawnPoints.Length)].position, Quaternion.identity);
        // initialize the player for all other players

        playerObj.GetComponent<PlayerController>().photonView.RPC("Initialize", RpcTarget.All, PhotonNetwork.LocalPlayer);
    }

    public PlayerController GetPlayer (int playerId)
    {
        foreach(PlayerController player in players)
        {
            if(player != null && player.id == playerId)
            return player;
        }


        Debug.LogWarning("GetPlayer could not find Actor ID: " + playerId);
        return null;
    }

    public PlayerController GetPlayer (GameObject playerObject)
    {
        foreach(PlayerController player in players)
        {
            if(player != null && player.gameObject == playerObject)
            return player;
        }
        
        return null;
    }

    public void CheckWinCondition ()
    {
        if (roundEnding)
            return;

        if (alivePlayers <= 1 && PhotonNetwork.IsMasterClient)
        {
                roundEnding = true;
                timerStarted = false;
                EndRoundByKills();
        }
    }

    public void RegisterPlayer(PlayerController player)
    {
        if (player == null)
            return;

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null && players[i].id == player.id)
            {
                players[i] = player;
                return;
            }
        }

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
            {
                players[i] = player;
                return;
            }
        }

        Debug.LogError("No empty slot for player ID: " + player.id);
    }

    void GoBackToMenu ()
    {
        NetworkManager.instance.ChangeScene("Menu");
    }

    public void StartRoundTimer()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        
        double endTime = PhotonNetwork.Time + roundDuration;

        photonView.RPC("SyncRoundTimer", RpcTarget.All, endTime);
    }

    [PunRPC]
    public void SyncRoundTimer(double endTime)
    {
        roundEndTime = endTime;
        timerStarted = true;
    }

    public void EndRoundByKills()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        // Create a list of players
        System.Collections.Generic.List<PlayerController> playerList = new System.Collections.Generic.List<PlayerController>();

        foreach (PlayerController p in players)
        {
            if (p != null)
                playerList.Add(p);
        }

        string scoreboard = "";

        // Sort from most kills to fewest
        playerList.Sort((a, b) => b.kills.CompareTo(a.kills));

        int count = Mathf.Min(3, playerList.Count);


        // display only the top 3 players
        for (int i = 0; i < count; i++)
        {
            PlayerController p = playerList[i];

            string playerName = p.photonPlayer != null ? p.photonPlayer.NickName : "Player " + (i + 1);

            scoreboard += playerName + "\t\t\t" + p.kills + "\n";
        }

        if (count == 0)
            scoreboard = "No players found.";

        photonView.RPC("ShowFinalResults", RpcTarget.All, scoreboard);
    }

    [PunRPC]
    public void ShowFinalResults(string scoreboard)
    {
        roundEnding = true;
        timerStarted = false;

        if (GameUI.instance != null)
            GameUI.instance.ShowFinalScoreboard(scoreboard);

        // only the master client schedules the scene change
        if (PhotonNetwork.IsMasterClient)
        {
            StopAllCoroutines();
            StartCoroutine(ReturnToMenu());
        }
    }

    IEnumerator ReturnToMenu()
    {
        // Keep the scoreboard visible for 10 seconds
        yield return new WaitForSeconds(scoreboardDisplayTime);

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ReturnToMenuForAll", RpcTarget.All);
        }
    }

    [PunRPC]
    public void ReturnToMenuForAll()
    {
        PhotonNetwork.LoadLevel("Menu");
    }

    // Update is called once per frame
    void Update()
    {
        if (timerStarted)
        {
            float timeRemaining = (float)(roundEndTime - PhotonNetwork.Time);

            if (GameUI.instance != null)
                GameUI.instance.UpdateTimer(timeRemaining);

            if (timeRemaining <= 0 && !roundEnding)
            {
                timerStarted = false;

                if (PhotonNetwork.IsMasterClient)
                {
                    roundEnding = true;
                    EndRoundByKills();
                }
            }
        }
    }
}

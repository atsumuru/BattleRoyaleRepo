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
    public float roundDuration = 120f;
    
    private double roundEndTime;
    private bool timerStarted;

    public float scoreboardDisplayTime = 5f;

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
        if(alivePlayers == 1)
        photonView.RPC("WinGame", RpcTarget.All, players.First(x => !x.dead).id);
    }

    [PunRPC]
    void WinGame (int winningPlayer)
    {
        // set the UI win text
        Invoke("GoBackToMenu", postGameTime);
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
        timerStarted = false;

        if (GameUI.instance != null)
            GameUI.instance.ShowFinalScoreboard(scoreboard);

        // only the master client schedules the scene change
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(ReturnToMenu());
    }

    IEnumerator ReturnToMenu()
    {
        // Keep the scoreboard visible for 5 seconds
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

            if (timeRemaining <= 0)
            {
                timerStarted = false;

                if (PhotonNetwork.IsMasterClient)
                    EndRoundByKills();
            }
        }
    }
}

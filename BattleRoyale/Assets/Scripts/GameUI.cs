using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameUI : MonoBehaviour
{
    public Slider healthBar;
    public TextMeshProUGUI playerInfoText;
    public TextMeshProUGUI ammoText;
    public TextMeshProUGUI winText;
    public Image winBackground;
    public Image crosshair;

    private PlayerController player;

    // instance
    public static GameUI instance;

    public TextMeshProUGUI timerText;

    void Awake ()
    {
        instance = this;
    }

    public void Initialize (PlayerController localPlayer)
    {
        player = localPlayer;
        healthBar.maxValue = player.maxHp;
        healthBar.value = player.curHp;
        UpdatePlayerInfoText();
        UpdateAmmoText();
    }

    public void UpdateHealthBar ()
    {
        healthBar.value = player.curHp;
    }

    public void UpdatePlayerInfoText ()
    {
        playerInfoText.text = "<b>Alive:</b> " + GameManager.instance.alivePlayers + "\n<b>Kills:</b> " + player.kills;
    }

    public void UpdateAmmoText ()
    {
        ammoText.text = player.weapon.curAmmo + " / " + player.weapon.maxAmmo;
    }

    public void SetWinText (string winnerName)
    {
        winBackground.gameObject.SetActive(true);
        crosshair.gameObject.SetActive(false);
        winText.text = winnerName + " wins";
    }

    public void HidePlayerUI()
    {
        healthBar.gameObject.SetActive(false);
        ammoText.gameObject.SetActive(false);
        crosshair.gameObject.SetActive(false);

    }

    public void UpdateTimer(float timeRemaining)
    {
        timeRemaining = Mathf.Max(0, timeRemaining);

        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        int milliseconds = Mathf.FloorToInt((timeRemaining * 100) % 100);
    
        timerText.text = minutes.ToString("00") + ":" + seconds.ToString("00") + "." + milliseconds.ToString("00");

        // turn the timer red at 10 seconds
        if (timeRemaining <= 10f)
            timerText.color = Color.red;
        else
            timerText.color = Color.white;
    }

    public void ShowFinalScoreboard(string scoreboard)
    {
        winBackground.gameObject.SetActive(true);

        winText.text = "<b>GAME OVER!</b>\n" + "FINAL KILL BOARD\n" + scoreboard;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

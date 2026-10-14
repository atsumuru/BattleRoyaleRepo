using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class PlayerController : MonoBehaviourPun
{
    [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;

    [Header("Components")]
    public Rigidbody rig;

    public int id;

    [System.NonSerialized]
    public Player photonPlayer;
    private int curAttackerId;

    public int curHp;
    public int maxHp;
    public int kills;
    public bool dead;
    private bool flashingDamage;
    public MeshRenderer mr;

    public PlayerWeapon weapon;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Move ()
    {
        // get the input axis
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        // calculate a direction relative to where we're facing
        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.linearVelocity.y;

        // set that as our velocity
        rig.linearVelocity = dir;
    }

    void TryJump ()
    {
        // create a ray facing down
        Ray ray = new Ray(transform.position, Vector3.down);

        // shoot the raycast
        if(Physics.Raycast(ray, 1.5f))
        rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    [PunRPC]
    public void Initialize (Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;

        GameManager.instance.RegisterPlayer(this);

        // is this not our local player?
        if(!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);
            rig.isKinematic = true;
        }
        else
        {
            GameUI.instance.Initialize(this);
        }
    }

    [PunRPC]
    public void TakeDamage (int attackerId, int damage)
    {
        if(dead)
        return;
        curHp -= damage;
        curAttackerId = attackerId;

        // flash the player red
        photonView.RPC("DamageFlash", RpcTarget.Others);

        // update the health bar UI
        if (photonView.IsMine)
            GameUI.instance.UpdateHealthBar();

        // die if no health left
        if(curHp <= 0)
        photonView.RPC("Die", RpcTarget.All);
    }

    [PunRPC]
    void DamageFlash ()
    {
        if(flashingDamage)
        return;

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine ()
        {
            flashingDamage = true;

            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;
        }
    }

    [PunRPC]
    void Die ()
    {
        if (dead)
            return;

        curHp = 0;
        dead = true;

        GameManager.instance.alivePlayers--;

        if (GameUI.instance != null)
            GameUI.instance.UpdatePlayerInfoText();

        // the master client awards the kill before checking the win condition
        if (PhotonNetwork.IsMasterClient)
        {
            if (curAttackerId != 0)
            {
                PlayerController attacker = GameManager.instance.GetPlayer(curAttackerId);

                if (attacker != null)
                {
                    attacker.photonView.RPC("AddKill", RpcTarget.All);
                }
            }

            StartCoroutine(CheckWinConditionAfterKill());
        }

        // is this our local player?
        if(photonView.IsMine)
        {
            // set the cam to spectator
            GetComponentInChildren<CameraController>().SetAsSpectator();

            // hide player
            if (GameUI.instance != null)
                GameUI.instance.HidePlayerUI();

            // disable the physics and hide the player
            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);

            // hide gun
            weapon.gunModel.SetActive(false);
        }
    }

    IEnumerator CheckWinConditionAfterKill()
    {
        // Allow the kill-count RPC to be processed before building the scoreboard
        yield return new WaitForSeconds(0.1f);

        GameManager.instance.CheckWinCondition();
    }

    [PunRPC]
    public void AddKill ()
    {
        kills++;

        Debug.Log("AddKill received. Player ID: " + id + ", Kills: " + kills + ", IsMine: " + photonView.IsMine);

        // update kill counter for the player who earned the kill
        if (photonView.IsMine && GameUI.instance != null)
        {
            GameUI.instance.UpdatePlayerInfoText();
        }
    }

    [PunRPC]
    public void Heal (int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);

        // update the health bar UI
        if (photonView.IsMine)
            GameUI.instance.UpdateHealthBar();
    }

    // Update is called once per frame
    void Update()
    {
        if(!photonView.IsMine || dead)
        return;

        Move();
        
        if(Input.GetKeyDown(KeyCode.Space))
            TryJump();
        if(Input.GetMouseButtonDown(0))
            weapon.TryShoot();
    }
}

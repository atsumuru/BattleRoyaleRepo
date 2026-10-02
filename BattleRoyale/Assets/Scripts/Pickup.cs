using UnityEngine;
using Photon.Pun;

public class Pickup : MonoBehaviour
{
    public PickupType type;
    public int value;

    public enum PickupType
    {
        Health,
        Ammo
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void OnTriggerEnter (Collider other)
    {
        if(!PhotonNetwork.IsMasterClient)
        return;
        
        if(other.CompareTag("Player"))
        {
            // get the player
            PlayerController player = GameManager.instance.GetPlayer(other.gameObject);

            if(type == PickupType.Health)
            player.photonView.RPC("Heal", player.photonPlayer, value);
            else if(type == PickupType.Ammo)
            player.photonView.RPC("GiveAmmo", player.photonPlayer, value);

            // destroy the object
            PhotonNetwork.Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

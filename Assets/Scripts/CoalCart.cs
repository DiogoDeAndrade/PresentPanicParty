using System.Collections.Generic;
using UC;
using UnityEngine;

public class CoalCart : MonoBehaviour
{
    [SerializeField]
    private AudioSource     audioSound;

    List<Player> playersInArea = new();

    // Update is called once per frame
    void Update()
    {
        int count = 0;
        foreach (var player in playersInArea)
        {
            if (player.invulnerable)
                player.ResetGatherCoal();
            else
            {
                if (player.AddGatherCoal(Time.deltaTime)) count++;

            }
        }

        playersInArea.RemoveAll((player) => (player == null) && (player.invulnerable));
        
        if (count > 0)
        {
            audioSound.FadeTo(1.0f, 0.1f);
        }
        else
        {
            audioSound.FadeTo(0.0f, 0.1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            if (playersInArea.IndexOf(player) == -1)
            {
                playersInArea.Add(player);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            if (playersInArea.IndexOf(player) != -1)
            {
                player.ResetGatherCoal();
                playersInArea.Remove(player);
            }
        }
    }
}

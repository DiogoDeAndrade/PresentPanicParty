using System.Collections.Generic;
using UnityEngine;

public class CoalCart : MonoBehaviour
{
    List<Player> playersInArea = new();

    // Update is called once per frame
    void Update()
    {
        foreach (var player in playersInArea)
        {
            if (player.invulnerable)
                player.ResetGatherCoal();
            else
                player.AddGatherCoal(Time.deltaTime);
        }

        playersInArea.RemoveAll((player) => (player == null) && (player.invulnerable));
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

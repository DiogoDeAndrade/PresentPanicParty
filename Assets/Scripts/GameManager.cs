using System.Collections.Generic;
using UC;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public class PlayerData
    {
        public Color hatColor;
        public Color hairColor;
        public Color clothesColor;
    }

    List<PlayerData>    playerData;
    MasterInputManager  masterInputManager;

    static GameManager instance;

    void Start()
    {
        if ((instance != null) && (instance != this))
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        playerData = new();
        for (int i = 0; i < MasterInputManager.GetMaxPlayers(); i++)
        {
            playerData.Add(null);
        }
    }

    PlayerData _GetPlayerData(int playerId)
    {
        return playerData[playerId];
    }

    void _SetPlayerData(int playerId, PlayerData playerData)
    {
        this.playerData[playerId] = playerData;
    }
    
    void _ResetPlayerData()
    {
        for (int i = 0; i < playerData.Count; i++) playerData[i] = null;
    }

    public static void ResetPlayerData() => instance?._ResetPlayerData();

    public static void SetPlayerData(int playerId, PlayerData playerData) => instance?._SetPlayerData(playerId, playerData);
    public static PlayerData GetPlayerData(int playerId) => instance?._GetPlayerData(playerId) ?? null;
}

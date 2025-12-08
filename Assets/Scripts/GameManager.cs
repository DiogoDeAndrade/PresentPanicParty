using System.Collections.Generic;
using UC;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerData
    {
        public bool  enable = true;
        public Color hatColor;
        public Color hairColor;
        public Color clothesColor;
    }

    [SerializeField] private List<PlayerData>   debugPlayerData;

    List<PlayerData>    playerData;
    MasterInputManager  masterInputManager;

    static GameManager instance;

    void Awake()
    {
        if ((instance != null) && (instance != this))
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

#if UNITY_EDITOR
        playerData = new();
        for (int i = 0; i < MasterInputManager.GetMaxPlayers(); i++)
        {
            if ((debugPlayerData != null) && (debugPlayerData.Count > i) && (debugPlayerData[i] != null) && (debugPlayerData[i].enable))
                playerData.Add(debugPlayerData[i]);
            else
                playerData.Add(null);
        }        
#else
        playerData = new();
        for (int i = 0; i < MasterInputManager.GetMaxPlayers(); i++)
        {
            playerData.Add(null);
        }
#endif
    }

    PlayerData _GetPlayerData(int playerId)
    {
        var pd = playerData[playerId];

        if ((pd != null) && (pd.enable)) return pd;

        return null;
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

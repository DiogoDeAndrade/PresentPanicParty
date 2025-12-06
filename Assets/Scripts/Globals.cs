using NaughtyAttributes;
using UC;
using UnityEngine;

[CreateAssetMenu(fileName = "Globals", menuName = "PPP/Globals")]
public class Globals : GlobalsBase
{
    [HorizontalLine(color: EColor.Green)]
    [SerializeField] 
    private bool _canDropOnAnyBag = false;
    [SerializeField, ShowIf(nameof(_canDropOnAnyBag))] 
    private bool _ownerWinsDrop = false;

    protected static Globals _instance = null;

    public static Globals instance
    {
        get
        {
            if (_instance) return _instance;

            Debug.Log("Globals not loaded, loading...");

            var allConfigs = Resources.LoadAll<Globals>("");
            if (allConfigs.Length == 1)
            {
                _instance = allConfigs[0];
            }

            return _instance;
        }
    }

    public static bool canDropOnAnyBag => instance?._canDropOnAnyBag ?? false;
    public static bool ownerWinsDrop => instance?._ownerWinsDrop ?? false;
}

using NaughtyAttributes;
using UnityEngine;

public class ElfCustomizer : MonoBehaviour
{
    [SerializeField] private Color hairColor = Color.yellow;
    [SerializeField] private Color clothColor = Color.green;
    [SerializeField] private Color hatColor = Color.red;

    [SerializeField] Renderer mainRenderer;

    Material _material;

    public Material material => _material;

    void Start()
    {
        Setup();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [Button("Setup")]
    void Setup()
    {
        _material = new Material(mainRenderer.material);
        _material.name = "CustomizedMaterial";

        _material.SetColor("_Color_0", clothColor);
        _material.SetColor("_Color_1", hairColor);
        _material.SetColor("_Color_2", hatColor);
        mainRenderer.material = _material;
        // This is needed because a copy of the material is actually done
        _material = mainRenderer.material;
    }

    public Color GetClothColor() => clothColor;
    public Color GetHatColor() => hatColor;

}

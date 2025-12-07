using NaughtyAttributes;
using System;
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

    [Button("Setup")]
    void Setup()
    {
        if (_material == null)
        {
            _material = new Material(mainRenderer.material);
            _material.name = "CustomizedMaterial";
        }

        _material.SetColor("_Color_0", clothColor);
        _material.SetColor("_Color_1", hairColor);
        _material.SetColor("_Color_2", hatColor);
        mainRenderer.material = _material;
    }

    public Color GetClothColor() => clothColor;
    public Color GetHatColor() => hatColor;

    public void SetColors(Color hatColor, Color hairColor, Color clothesColor)
    {
        this.hatColor = hatColor;
        this.hairColor = hairColor;
        this.clothColor = clothesColor;

        Setup();
    }
}

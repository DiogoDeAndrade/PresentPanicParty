using NaughtyAttributes;
using UnityEngine;

public class ElfCustomizer : MonoBehaviour
{
    [SerializeField] private Color hairColor = Color.yellow;
    [SerializeField] private Color clothColor = Color.green;
    [SerializeField] private Color hatColor = Color.red;

    [SerializeField] private Texture2D hairTexture;
    [SerializeField] private Texture2D clothTexture;
    [SerializeField] private Texture2D hatTexture;
    [SerializeField] private Texture2D baseTexture;

    [SerializeField] Renderer mainRenderer;

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
        var material = new Material(mainRenderer.material);
        material.name = "CustomizedMaterial";

        material.SetTexture("_BaseMap", BakeTexture());
        mainRenderer.material = material;
    }

    Texture2D BakeTexture()
    {
        var imageData = baseTexture.GetPixels();
        var hairData = hairTexture.GetPixels();
        var clothData = clothTexture.GetPixels();
        var hatData = hatTexture.GetPixels();

        int index = 0;
        for (int y = 0; y < baseTexture.width; y++)
        {
            for (int x = 0; x < baseTexture.height; x++)
            {
                var color = imageData[index];
                color = Color.Lerp(color, hairColor, hairData[index].a);
                color = Color.Lerp(color, clothColor, clothData[index].a);
                color = Color.Lerp(color, hatColor, hatData[index].a);

                imageData[index] = color;
                index++;
            }
        }

        Texture2D texture = new Texture2D(baseTexture.width, baseTexture.height, TextureFormat.ARGB32, true, false);
        texture.SetPixels(imageData);
        texture.Apply();

        return texture;
    }
}

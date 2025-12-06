using System.Collections;
using System.Collections.Generic;
using UC;
using UnityEngine;

public class Essence : MonoBehaviour
{
    class EssenceElem
    {
        public Transform    transform;
        public Vector3      axis;
        public float        speed; 
        public float        elapsedTime;
        public Vector3      startPos;

        public void UpdatePos()
        {
            transform.localPosition = Quaternion.AngleAxis(elapsedTime * speed, axis) * startPos;

            elapsedTime += Time.deltaTime;
        }
    }
    [Header("Elements")]
    [SerializeField] private int        elements = 3;
    [SerializeField] private float      radius = 0.25f;
    [SerializeField] private Vector2    speed = new Vector2(180.0f, 360.0f);
    [SerializeField] private GameObject essenceElementPrefab;

    List<EssenceElem>   elementList = new();
    Light               essenceLight;
    float               lightIntensity;

    void Start()
    {
        for (int i = 0; i < elements; i++)
        {
            var elem = new EssenceElem();
            elem.transform = Instantiate(essenceElementPrefab, transform).transform;
            elem.speed = speed.Random();
            elem.axis = Random.onUnitSphere;
            elem.startPos = radius * elem.axis.Perpendicular();

            elem.UpdatePos();

            elementList.Add(elem);
        }

        essenceLight = GetComponentInChildren<Light>();
        lightIntensity = essenceLight.intensity;
        essenceLight.intensity = 0.0f;

        essenceLight.FadeTo(lightIntensity, 0.25f);
    }

    void Update()
    {
        foreach (var elem in elementList)
        {
            elem.UpdatePos();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();
        if (player)
        {
            player.AddEssence(1);
            essenceLight.FadeTo(0.0f, 0.1f);
            GetComponent<Collider>().enabled = false;
            StartCoroutine(DestroyInCR(0.1f));
        }
    }

    IEnumerator DestroyInCR(float delay)
    {
        yield return new WaitForSeconds(delay);

        Destroy(gameObject);
    }
}

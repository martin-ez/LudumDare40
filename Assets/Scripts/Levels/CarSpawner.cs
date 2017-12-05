using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSpawner : MonoBehaviour
{
    public GameObject carPrefab;
    public Material[] colors;
    public Transform[] wayPoints;

    public float rate = 10;
    public float spawnChance = 0.75f;

    private float nextSpawn;

    void Start()
    {
        nextSpawn = Time.time + rate + (rate/2 * Random.Range(-1f, 1f));
    }

    void Update()
    {
        if (Time.time > nextSpawn)
        {
            float chance = Random.value;
            if (chance < spawnChance)
            {
                GameObject car = Instantiate(carPrefab);
                car.transform.position = transform.position;
                Material color = colors[Random.Range(0, colors.Length)];
                foreach (Transform child in car.transform)
                {
                    Renderer rend = child.gameObject.GetComponent<Renderer>();
                    if (child.name == "Body")
                    {
                        rend.material = color;
                    }
                }
                CarAI ai = car.GetComponent<CarAI>();
                ai.SetWayPoints(wayPoints);
            }
            nextSpawn = Time.time + rate * Random.value;
        }
    }
}

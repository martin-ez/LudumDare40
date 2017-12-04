using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StationMaster : MonoBehaviour
{
    public Station[] stops;

    private int current = 0;
    private bool finish = false;

    void Start()
    {
        if (stops.Length != 0)
        {
            stops[0].OnPickup += OnPickup;
            stops[0].SetActive(true);
        }
    }

    void OnPickup()
    {
        stops[current].OnPickup -= OnPickup;
        stops[current].SetActive(false);
        current++;
        finish = current >= stops.Length;
        if (!finish)
        {
            stops[current].OnPickup += OnPickup;
            stops[current].SetActive(true);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Bus") && finish)
        {
            FindObjectOfType<Bus>().FinishLevel();
        }
    }
}

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
        for (int i = 0; i<stops.Length; i++)
        {
            if(i == 0)
            {
                stops[i].OnPickup += OnPickup;
                stops[i].SetActive(true);
            }
            else
            {
                stops[i].SetActive(false);
            }
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
            other.gameObject.GetComponent<Bus>().FinishLevel();
        }
    }
}

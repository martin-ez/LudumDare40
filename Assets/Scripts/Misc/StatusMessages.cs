using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusMessages : MonoBehaviour {

    public int level = 1;

    public string GetMessage(int passengers)
    {
        if (passengers > 230)
        {
           return "!?!";
        }
        else if (passengers > 210)
        {
           return "I'm done";
        }
        else if (passengers > 180)
        {
           return "Seriously?";
        }
        else if (passengers > 150)
        {
           return "Stop It!!";
        }
        else if (passengers > 120)
        {
           return "Illegal";
        }
        else if (passengers > 90)
        {
           return "Full!";
        }
        else if (passengers > 60)
        {
           return "Limited space";
        }
        else if (passengers > 30)
        {
           return "Keep em' coming";
        }
        else if (passengers > 0)
        {
           return "Near empty";
        }
        else
        {
            return "Empty";
        }
    }
}

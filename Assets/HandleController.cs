using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleController : MonoBehaviour
{
    public string side;
    public bool handOnBars;

    private void OnTriggerEnter(Collider other)
    {
        if(side == "Left" && other.gameObject.tag == "Left Hand") handOnBars = true;
        else if(side == "Right" && other.gameObject.tag == "Right Hand") handOnBars = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (side == "Left" && other.gameObject.tag == "Left Hand") handOnBars = false;
        else if (side == "Right" && other.gameObject.tag == "Right Hand") handOnBars = false;
    }
}

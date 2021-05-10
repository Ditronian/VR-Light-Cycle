using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;
    public PlayerController player;
    public TrailRenderer trail;
    public bool isTrailDisabled;
    public int countDown = 5;

    void Awake()
    {
        player = gameObject.GetComponent<PlayerController>();
    }

    private void FixedUpdate()
    {
        if (isTrailDisabled && countDown == 0)
        {
            trail.enabled = true;
            isTrailDisabled = false;
            countDown = 5;
        }
        else if (isTrailDisabled) countDown--;
    }

    public void SetTrailRenderer()
    {
        isTrailDisabled = false;
        if (id == Client.instance.myId) trail = gameObject.transform.GetChild(3).GetComponent<TrailRenderer>();
        else trail = gameObject.transform.GetChild(1).GetComponent<TrailRenderer>();
    }

}

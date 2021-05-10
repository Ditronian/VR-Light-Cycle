using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager instance;
    public GameObject playerPrefab;
    public Vector3[] spawnLocation = new Vector3[4];

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null) instance = this;
        else if(instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        spawnLocation[0] = new Vector3(-40f, 0f, 0f);
        spawnLocation[1] = new Vector3(40f, 0f, 0f);
        spawnLocation[2] = new Vector3(0f, 0f, -40f);
        spawnLocation[3] = new Vector3(0f, 0f, 40f);
        
    }

    private void Start()
    {

        Server.Start(4,2550);

    }

    public Player InstantiatePlayer(int playerID)
    {
        return Instantiate(playerPrefab, spawnLocation[playerID-1], Quaternion.identity).GetComponent<Player>();
    }
}

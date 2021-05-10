using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    public Material[] cycleColors = new Material[4];
    public Material[] trailColors = new Material[4];
    private static EffectsController effectsController;

    private void Awake()
    {
        if (instance == null) instance = this;
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }

        effectsController = GameObject.Find("Effects Player").GetComponent<EffectsController>();
    }

    public void SpawnPlayer(int id, string username, Vector3 position, Quaternion rotation)
    {
        
        GameObject player;
        if(id == Client.instance.myId) player = Instantiate(localPlayerPrefab, position, rotation);
        else player = Instantiate(playerPrefab, position, rotation);

        //Set player color
        player.transform.GetChild(0).GetComponent<Renderer>().material.color = cycleColors[id-1].color;
        TrailRenderer trail = player.transform.GetChild(3).GetComponent<TrailRenderer>();
        trail.material.color = cycleColors[id-1].color;

        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        
        playerManager.id = id;

        playerManager.username = username;
        players.Add(id, playerManager);
    }

    public static void Crash()
    {
        effectsController.PlayCrashAudio();
    }
}

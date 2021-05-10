using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        TrailRenderer trail;
        if (id == Client.instance.myId)
        {
            player = Instantiate(localPlayerPrefab, position, rotation);
            trail = player.transform.GetChild(3).GetComponent<TrailRenderer>();
        }
        else 
        { 
            player = Instantiate(playerPrefab, position, rotation);
            trail = player.transform.GetChild(1).GetComponent<TrailRenderer>();
        }

        //Set player color
        player.transform.GetChild(0).GetComponent<Renderer>().material.color = cycleColors[id-1].color;
        trail.material.color = cycleColors[id-1].color;

        PlayerManager playerManager = player.GetComponent<PlayerManager>();
        
        playerManager.id = id;
        playerManager.username = username;
        playerManager.SetTrailRenderer();

        if(!players.ContainsKey(id)) players.Add(id, playerManager);
        else
        {
            players.Remove(id);
            players.Add(id, playerManager);
        }
    }


    //Handles the playing of crash audio
    public static void Crash()
    {
        effectsController.PlayCrashAudio();
    }

    //Handles the reseting of the game
    public static void PreReset()
    {
        Debug.Log("Round reset");
        //Reset trails and bar position
        foreach (PlayerManager player in players.Values)
        {
            player.trail.emitting = false;
        }
    }

    //Handles the reseting of the game
    public void RoundReset()
    {
        Debug.Log("Round reset");
        //Reset trails and bar position
        foreach(PlayerManager player in players.Values)
        {
            player.trail.time = 0;
            StartCoroutine(ResetTrail(player));
        }
    }

    IEnumerator ResetTrail(PlayerManager player)
    {
        yield return new WaitForSeconds(.1f);
        player.trail.time = 999999999999999999;
    }
}

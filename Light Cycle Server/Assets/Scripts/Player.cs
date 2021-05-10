using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public int id;
    public string username;
    public GameObject trailColliderPrefab;
    public Client client;

    public float moveSpeed = 10f / Constants.TICKS_PER_SEC;
    private float turnSpeed = 2f / Constants.TICKS_PER_SEC;
    private float trailColliderTracker = 0f;
    public bool isDead = false;
    private bool isFirstCollider = true;
    private GameController game;

    //Rotation
    public float pitch;
    public float yaw;
    public float roll;
    public float maximumRoll;

    public void Initialize(int id, string username, Client client)
    {
        this.id = id;
        this.username = username;
        this.client = client;
        if (!Server.hasStarted) isDead = true;

        game = GameObject.Find("GameManager").GetComponent<GameController>();
    }

    public void FixedUpdate()
    {
        if (isDead) return;
        Move();
    }

    //Handles Player Movement and Rotation, with related functions
    private void Move()
    {
        Vector3 euler = transform.eulerAngles;

        //If my upcoming movement will exceed 2 units, drop a collider and reset the tracker.  I need to replace 2f with a non-magic number at some point. 2*width of player's collider.
        //Skip first collider since this would be behind the trail's start position
        if (trailColliderTracker + moveSpeed > 2f && !isFirstCollider)
        {
            DropTrailCollider(euler);
            trailColliderTracker = 0f;
        }
        else if (isFirstCollider) isFirstCollider = false;

        //Calculate Turn Strength
        float turnAmount = roll * 0.0444f;
        yaw += turnAmount;

        //Update Position and Rotation
        transform.position += transform.forward * moveSpeed;

        //Yaw Rotation
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        transform.Rotate(0,0,-roll);

        trailColliderTracker += moveSpeed;

        //Kill the player if collided with grid wall.  The -0.5f is the player model's width.
        if (Mathf.Abs(transform.position.x) >= game.gridRadius - 0.5f || Mathf.Abs(transform.position.z) >= game.gridRadius - 0.5f)
        {
            Debug.Log("A player has hit the edge and crashed!");
            KillPlayer();
        }
            
            

        //Send New Position and Rotation to all players
        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    //Instantiates an invisible box collider along the player's trail.
    private void DropTrailCollider(Vector3 euler)
    {
        GameObject trailCollider = Instantiate(trailColliderPrefab);
        trailCollider.transform.eulerAngles = new Vector3(euler.x, euler.y, 0f);    //Rotate the box so that it lines up somewhat nicely with the trail.  Not a big difference probably.

        trailCollider.transform.position = transform.position - transform.forward;
    }

    //Sets the bike's roll value
    public void SetRoll(float barAngle)
    {
        roll = barAngle;
        if (roll < -maximumRoll) roll = -maximumRoll;
        else if (roll > maximumRoll) roll = maximumRoll;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isDead && other.tag == "Wall") 
        {
            Debug.Log("A player has hit a wall and crashed!");
            KillPlayer();
        }
    }

    //Handle Player's death
    private void KillPlayer()
    {
        //Game Setting Overides
        if(!game.canCrash) return;
        if(!game.debugPlayer1CanCrash && id == 1) return;

        isDead = true;
        DropTrailCollider(transform.eulerAngles);
        ServerSend.PlayerCrashed(this.id, this);

        //Round Over
        Server.livingPlayers--;
        if (Server.livingPlayers <= 1 && Server.clients.Count >= 2) Server.Restart();
    }
}

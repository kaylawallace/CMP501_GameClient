using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeySpawner : MonoBehaviour
{
    public int spawnerId;
    public bool hasKey;
    public MeshRenderer key;
    public float rotSpeed = 50f;
    public float bobSpeed = 2f;
    private Vector3 basePos;

    private void Update()
    {
        // Animates the key in the spawner by rotating and 'bobbing' the key up and down if the spawner 'hasKey' is true
        if (hasKey)
        {
            transform.Rotate(Vector3.up, rotSpeed * Time.deltaTime, Space.World);
            transform.position = basePos + new Vector3(0f, 0.25f * Mathf.Sin(Time.time * bobSpeed), 0f);
        }
    }

    /*
     * Method to intialise a key spawner 
     * Initialises spawner id & hasKey values, renders the key based on hasKey value, and sets the base position (used for the animating) 
     * Params: spawner ID and whether it has a key 
     */
    public void Initialize(int _spawnerId, bool _hasKey)
    {
        spawnerId = _spawnerId;
        hasKey = _hasKey;
        key.enabled = _hasKey;
        basePos = transform.position;
    }

    /*
     * Method run when a key is spawned 
     * Sets hasKey to true and renders the key 
     */
    public void KeySpawned()
    {
        hasKey = true;
        key.enabled = true;
    }

    /*
     * Method run when a player picks up a key 
     * Sets hasKey to false and removes the key from the scene view 
     */
    public void KeyPickedUp()
    {
        hasKey = false;
        key.enabled = false;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public static Dictionary<int, KeySpawner> keySpawners = new Dictionary<int, KeySpawner>();
    public GameObject localPlrPrefab;
    public GameObject plrPrefab;
    public GameObject keySpawnerPrefab;
    public Transform bridge;
    
    // Singleton initialiser 
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object.");
            Destroy(this);
        }
    }

    /*
     * Method to handle player spawning 
     * Params: player ID, username, position, rotation 
     */
    public void SpawnPlayer(int _id, string _uname, Vector3 _pos, Quaternion _rot)
    {
        // If the ID passed to the method is that of this client instance 
        GameObject _plr;
        if (_id == Client.instance.myId)
        {
            // Instantiate the player in the level at the position/rotation passed in
            _plr = Instantiate(localPlrPrefab, _pos, _rot);
            // Set the players tag 
            _plr.tag = "Player";
        }
        else
        {
            _plr = Instantiate(plrPrefab, _pos, _rot);
        }

        // Intialise the player through its player manager component and add it to the players dictionary 
        _plr.GetComponent<PlayerManager>().Initialize(_id, _uname);
        players.Add(_id, _plr.GetComponent<PlayerManager>());
    }

   /*
    * Method to create key spawners 
    * Params: spawner ID, position, bool hasKey (whether the spawner has a key) 
    */
    public void CreateKeySpawner(int _spawnerId, Vector3 _pos, bool _hasKey)
    {
        // Instantiates a spawner using the passed in position 
        GameObject _spawner = Instantiate(keySpawnerPrefab, _pos, keySpawnerPrefab.transform.rotation);
        // Initialise the spawner using the ID and hasKey variables 
        _spawner.GetComponent<KeySpawner>().Initialize(_spawnerId, _hasKey);
        // Add the spawner to the dictionary of key spawners 
        keySpawners.Add(_spawnerId, _spawner.GetComponent<KeySpawner>());
    }

    /*
     * Method to move the bridge 
     * Calls the coroutine that interpolates the positions 
     * Params: pos - new position of the bridge 
     */
    public void MoveBridge(Vector3 _pos)
    {
        StartCoroutine(MoveBridge(_pos, 1f));
    }

    /*
     * Coroutine to interpolate the bridge position 
     * Uses Lerp() to linearly interpolate the bridge position passed from the server to display a smooth movement instead of snapping movement 
     * Params: new bridge position and duration of the Lerp
     */
    IEnumerator MoveBridge(Vector3 _newPos, float _duration)
    {
        for (float time = 0; time < _duration * 2; time += Time.deltaTime)
        {
            float progress = Mathf.PingPong(time, _duration) / _duration;
            bridge.position = Vector3.Lerp(bridge.position, _newPos, progress);
            yield return null;
        }
    }

    /*
     * Method to move platforms 
     * Calls the coroutine that will interpolate the position of the correct colour platform 
     * Params: _tag - specifies platform colour to move, _pos - new position of platform
     */
    public void MovePlatform(string _tag, Vector3 _pos)
    {
        StartCoroutine(MovePlatform(_tag, _pos, 1f));
    }

    /*
     * Coroutine to interpolate the platform position
     * Uses Lerp() to linearly interpolate the platform posiiton passed from the server to display a smooth movement instead of a snapping movement
     * Params: _tag - specifies platform colour to move, _pos - new position of platform, _duration - Lerp duration 
     */
    IEnumerator MovePlatform(string _tag, Vector3 _newPos, float _duration)
    {
        GameObject platform = GameObject.FindGameObjectWithTag(_tag);
        for (float time = 0; time < _duration * 2; time += Time.deltaTime)
        {
            float progress = Mathf.PingPong(time, _duration) / _duration;
            platform.transform.position = Vector3.Lerp(platform.transform.position, _newPos, progress);
            yield return null;
        }
    }
}

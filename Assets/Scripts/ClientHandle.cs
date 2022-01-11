using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net;

public class ClientHandle : MonoBehaviour
{
    /*
     * Method receives welcome packet from the server
     * Sets client ID with ID read from the welcome packet and connects the client instance for UDP communication 
     */
    public static void RcvWelcome(Packet _packet)
    {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Welcome message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeRcvd();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    /*
     * Method reads SpawnPlayer packet from server
     * Reads out player ID, username, position and rotation and spawns the player accordingly 
     */
    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _uname = _packet.ReadString();
        Vector3 _pos = _packet.ReadVector3();
        Quaternion _rot = _packet.ReadQuaternion();

        print(_id + ", " + _uname + ", " + _pos + ", " + _rot);

        GameManager.instance.SpawnPlayer(_id, _uname, _pos, _rot);
    }

    /*
     * Method reads PlayerPosition packet 
     * Reads out player ID and posiiton 
     * Calls PlayerMovementInterpolation method to interpolate the players position with the last for smooth movement
     */
    public static void PlayerPosition(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _pos = _packet.ReadVector3();

        // Sets players position without interpolation 
        //GameManager.players[_id].transform.position = _pos;

        GameManager.players[_id].PlayerMovementInterpolation(_id, _pos);
    }

    /*
     * Method reads PlayerRotation packet
     * Reads out the player ID and rotation values
     * Sets the players rotation to the new rotation
     */
    public static void PlayerRotation(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Quaternion _rot = _packet.ReadQuaternion();

        GameManager.players[_id].transform.rotation = _rot;
    }

    /*
     * Method reads PlayerDisconnect packet 
     * Reads out the playres ID
     * Destroys the player's game object and removes them from the players dictionary 
     */
    public static void PlayerDisconnect(Packet _packet)
    {
        int _id = _packet.ReadInt();

        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
    }

    /*
     * Method reads CreateKeySpawner packet 
     * Reads out the spawner ID, position, and whether it contains a key 
     * Calls the game manager's CreateKeySpawner() method using this info
     */
    public static void CreateKeySpawner(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        Vector3 _spawnerPos = _packet.ReadVector3();
        bool _hasKey = _packet.ReadBool();

        GameManager.instance.CreateKeySpawner(_spawnerId, _spawnerPos, _hasKey);
    }

    /*
     * Methods reads KeySpawned packet 
     * Reads out the spawner ID 
     * Calls the spawners KeySpawned() method on the spawner
     */
    public static void KeySpawned(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();

        GameManager.keySpawners[_spawnerId].KeySpawned();
    }

    /*
     * Method reads KeyPickedUp packet 
     * Reads out the spawner ID and player ID
     * Calls the spawners KeyPickedUp() method and increments the players number of keys 
     */
    public static void KeyPickedUp(Packet _packet)
    {
        int _spawnerId = _packet.ReadInt();
        int _plrId = _packet.ReadInt();

        GameManager.keySpawners[_spawnerId].KeyPickedUp();
        GameManager.players[_plrId].keys++;
        
    }

    /*
     * Method reads PlayerHealth packet 
     * Reads out the players ID and health value
     * Sets the players health to the new health value 
     */
    public static void PlayerHealth(Packet _packet)
    {
        int _plrId = _packet.ReadInt();
        int _plrHealth = _packet.ReadInt();

        GameManager.players[_plrId].SetHealth(_plrHealth);
    }

    /*
     * Method reads PlayerRespawn packet 
     * Reads out the players ID
     * Calls the players Respawn() method 
     */
    public static void PlayerRespawn(Packet _packet)
    {
        int _plrId = _packet.ReadInt();

        GameManager.players[_plrId].Respawn();
    }

    /*
     * Method reads the BridgePosition packet 
     * Reads out the position of the bridge 
     * Calls the game managers MoveBridge() method with the new position 
     */
    public static void BridgePosition(Packet _packet)
    {
        Vector3 _bridgePos = _packet.ReadVector3();

        GameManager.instance.MoveBridge(_bridgePos);
    }

    /*
     * Method reads PlatformPosition packet 
     * Reads out the tag of the platform and its position
     * Calls the game managers MovePlatform() method with the tag and new position
     */
    public static void PlatformPosition(Packet _packet)
    {
        string _tag = _packet.ReadString();
        Vector3 _platformPos = _packet.ReadVector3();

        GameManager.instance.MovePlatform(_tag, _platformPos);
    }

    /*
     * Method reads the EndOfLevel packet
     * Reads out boolean value isEOL (returns true if end of level has been triggered)
     * Sets the end of level UI menu to active 
     */
    public static void EndOfLevel(Packet _packet)
    {
        bool isEOL = _packet.ReadBool();
        print("got eol packet");
        if (isEOL)
        {
            print("GAME OVER");
            UIManager.instance.eolMenu.SetActive(true);
        }
    }
}

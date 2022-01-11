using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string uname;
    public int health;
    public int maxHealth;
    public MeshRenderer plrModel;
    public int keys;

    /*
     * Method to initialise a players info - id, username and health 
     * Params: _id - players ID, _uname - players username
     */
    public void Initialize(int _id, string _uname)
    {
        id = _id;
        uname = _uname;
        health = maxHealth;
    }

    /*
     * Method sets health to value input to it
     * Calls Die() method if players health is less than or equal to 0
     * Params: _health - new player health value 
     */
    public void SetHealth(int _health)
    {
        health = _health;

        if (health <= 0)
        {
            Die();
        }
    }

    /*
     * Handles player death 
     * Disables the player model upon death 
     */
    public void Die()
    {
        plrModel.enabled = false;
    }

    /*
     * Handles player respawn
     * Re-enables player model and resets their health 
     */
    public void Respawn()
    {
        plrModel.enabled = true;
        SetHealth(maxHealth);
    }

    /*
     * Method to handle player movement interpolation 
     * Calls the coroutine to calculate the interpolation 
     * Params: _id - player ID, _pos - new player position 
     */
    public void PlayerMovementInterpolation(int _id, Vector3 _pos)
    {
        StartCoroutine(PlayerMovementInterpolation(_id, _pos, 0.01f));
    }

    /*
     * Coroutine to handle calculations in regard to player movement interpolation 
     * Params: _id - player ID, _pos - position to move player to, _duration - Lerp() duration 
     */
    public IEnumerator PlayerMovementInterpolation(int _id, Vector3 _pos, float _duration)
    {
        // Gets the current position of the player 
        Vector3 currPos = GameManager.players[_id].transform.position;
        Vector3 newPos;
        for (float time = 0; time < _duration * 2; time += Time.deltaTime)
        {
            float progress = Mathf.PingPong(time, _duration) / _duration;
            // Lerps between position extracted from player and new position passed to the coroutine 
            newPos = Vector3.Lerp(currPos, _pos, progress);
            GameManager.players[_id].transform.position = newPos;
        }
        yield return null;
    }
}


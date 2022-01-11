using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Camera controller class 
 */
public class CameraController : MonoBehaviour
{
    public PlayerManager plr;
    public float sensitivity = 100f;
    public float clampAngle = 85f;

    private float vertRot; //vertical rotation
    private float horizRot; //horizontal rotation

    private void Start()
    {
        // Initialise vertical and horizontal rotation 
        vertRot = transform.localEulerAngles.x;
        horizRot = transform.localEulerAngles.y;
    }

    private void Update()
    {
        // Toggle the cursor mode on 'M' key press - allows user to toggle between controlling camera and using their mouse as normal 
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleCursorMode();
        }

        // If in locked state, camera control enabled 
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            Look();
        }
    }

    /*
     * Look() method
     * Allows for camera rotation based on users mouse position
     */
    private void Look()
    {
        // Get mouse input on X and Y axes
        float mouseVert = -Input.GetAxis("Mouse Y");
        float mouseHoriz = Input.GetAxis("Mouse X");

        // Translate vertical and horizontal rotation variables based on input, sensitivity and delta time 
        vertRot += mouseVert * sensitivity * Time.deltaTime;
        horizRot += mouseHoriz * sensitivity * Time.deltaTime;

        // Restrict the vertical rotation to +- clamp angle 
        vertRot = Mathf.Clamp(vertRot, -clampAngle, clampAngle);
        // Transform this (camera) rotation by vertical rotation value 
        transform.localRotation = Quaternion.Euler(vertRot, 0f, 0f);
        // Transform player rotation by horizontal rotation value
        plr.transform.rotation = Quaternion.Euler(0f, horizRot, 0f);
    }

    /*
     * ToggleCursorMode()
     * Called on press of 'M' in Update()
     * Toggles cursor mode between visible/invisible and locked/none 
     */
    private void ToggleCursorMode()
    {
        Cursor.visible = !Cursor.visible;

        if (Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }

}

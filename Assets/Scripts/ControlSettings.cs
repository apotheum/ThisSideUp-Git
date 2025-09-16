using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum MovementMode
{
    Mouse,   //The mouse controls where the block gets placed.
    Keyboard //The arrow keys move the block left and right.
}

public class ControlSettings : MonoBehaviour
{

    /* SETTINGS */
    //Customized by the player and saved to config file.

    //Event that gets called when any setting updates.
    public UnityEvent OnControlSettingsUpdate=new UnityEvent();

    //Movement Mode: Whether arrow keys move the blocks or the mouse does.
    public MovementMode movementMode;


    private void Awake()
    {
        //TODO settings file retrieval
        movementMode = MovementMode.Mouse;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log("Movement mode inverted!");
            if (movementMode == MovementMode.Mouse)
            {
                movementMode = MovementMode.Keyboard;
                OnControlSettingsUpdate.Invoke();
            }
            else
            {
                movementMode = MovementMode.Mouse;
                OnControlSettingsUpdate.Invoke();
            }
        }
    }

    private static ControlSettings instance;
    public static ControlSettings Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject instanceObject = new GameObject("Control Settings Object");
                instance = instanceObject.AddComponent<ControlSettings>();
            }

            return instance;
        }
    }



}

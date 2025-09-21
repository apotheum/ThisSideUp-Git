using System;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject instanceObject = new GameObject("Game Manager Object");
                instance = instanceObject.AddComponent<GameManager>();
            }

            return instance;
        }
    }

    //Singleton initialization
    private void Awake()
    {
        Shader.WarmupAllShaders();

        if (instance == null) { instance = this; }
        else { Destroy(gameObject); return;}

        gameOver = false;
    }

    private void Start()
    {
        BlockGravity.Instance.PlacedInvalidBlock.AddListener(GameOver);

        if (GetComponent<GameOverSequence>() == null)
        {
            gameObject.AddComponent<GameOverSequence>();
        }
    }

    public void NewGame()
    {
        GameStartEvent.Invoke();

        gameOver = false;
    }

    private void GameOver(BoxInstance instance)
    {
        gameOver = true;

        GameOverSequence gameOverSequence = GetComponent<GameOverSequence>();
        gameOverSequence.StartSequence(instance);

    }

    public bool gameOver { get; private set; }

    //Game reset event. Clears inventory, occupied spaces, etc.
    //Invoking this will destroy every block, clear every list, etc.
    //This gets called when the game starts.
    public UnityEvent GameResetEvent = new UnityEvent();

    //When the game starts.
    public UnityEvent GameStartEvent = new UnityEvent();

    //When the game ends. Gets called by MouseTracker when a block is placed incorrectly.
    public UnityEvent GameEndEvent = new UnityEvent();

    //When we return to the title screen after clicking the "continue" button on the scoreboard
    public UnityEvent ReturnToTitleEvent = new UnityEvent();


}

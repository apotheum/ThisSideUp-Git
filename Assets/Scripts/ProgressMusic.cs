using System;
using ThisSideUp.Boxes.Core;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class ProgressMusic : MonoBehaviour
{
    [SerializeField] private AudioSource titleAudio;

    [SerializeField] private AudioSource earlyGameAudio;
    [SerializeField] private AudioSource lateGameAudio;

    [SerializeField] private AudioSource gameOverAudio;

    [SerializeField] private float waitBeforeStartingAudio = 1.0f;

    [SerializeField] private float lerpSpeed = 1.0f;

    private float desiredMenuLevel = 1.0f;
    private float desiredEarlyLevel = 0.0f;
    private float desiredLateLevel = 0.0f;

    private float lerpDelayTimer = 0.0f;

    private void Start()
    {
        BlockGravity.Instance.PlacedInvalidBlock.AddListener(OnPlaceInvalidBlock);
        GameManager.Instance.GameStartEvent.AddListener(OnGameStart);
        MouseTracker.Instance.BlockPlaceEvent.AddListener(OnBlockPlace);
        GameManager.Instance.ReturnToTitleEvent.AddListener(OnReturnToTitleScreen);
    }

    private void OnReturnToTitleScreen()
    {
        titleAudio.volume = 1.0f;
        earlyGameAudio.volume = 0.0f;
        lateGameAudio.volume = 0.0f;

        titleAudio.Play();

        desiredEarlyLevel = 0.0f;
        desiredLateLevel = 0.0f;
        desiredMenuLevel = 1.0f;
    }

    private void OnPlaceInvalidBlock(BoxInstance instance)
    {
        titleAudio.Stop();
        earlyGameAudio.Stop();
        lateGameAudio.Stop();

        desiredEarlyLevel = 0.0f;
        desiredLateLevel = 0.0f;
        desiredMenuLevel = 0.0f;

        titleAudio.volume = 0.0f;
        earlyGameAudio.volume = 0.0f;
        lateGameAudio.volume = 0.0f;

        gameOverAudio.Play();
    }

    private void OnGameStart()
    {
        lerpDelayTimer = waitBeforeStartingAudio;

        earlyGameAudio.Play();
        lateGameAudio.Play();

        desiredEarlyLevel = 1.0f;
        desiredLateLevel = 0.0f;
        desiredMenuLevel = 0.0f;
    }

    private void OnBlockPlace(Vector3 pos)
    {
        float highestZ = MouseTracker.Instance.highestKnownZ;
        float gridMax = GridManager.Instance.highestGridZ;

        float ratio = highestZ / gridMax;

        Debug.Log("Ratio for audio: " + ratio);

        

        if (ratio > 0.5)
        {
            desiredEarlyLevel = 0.0f;
            desiredLateLevel = 1.0f;
        }
    }

    private void LateUpdate()
    {
        if (lerpDelayTimer > 0.0f)
        {
            lerpDelayTimer -= Time.deltaTime;
            return;
        }


        titleAudio.volume = Mathf.Lerp(titleAudio.volume, desiredMenuLevel, lerpSpeed * Time.deltaTime);
        earlyGameAudio.volume = Mathf.Lerp(earlyGameAudio.volume, desiredEarlyLevel, lerpSpeed * Time.deltaTime);
        lateGameAudio.volume = Mathf.Lerp(lateGameAudio.volume, desiredLateLevel, lerpSpeed * Time.deltaTime);

    }

}

using System;
using ThisSideUp.Boxes.Core;
using UnityEngine;

[System.Serializable]
public struct CameraPosition
{
    public Vector3 pos;
    public Quaternion rot;
    public float fov;
}

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private CameraPosition closestPos;
    [SerializeField] private CameraPosition furthestPos;

    [SerializeField] private CameraPosition titlePos;

    [SerializeField] Camera[] cameras;

    [SerializeField] private float ingameLerpSpeed=1.0f;
    [SerializeField] private float titleLerpSpeed=1.0f;

    private float lerpSpeed;
    private float highestZOffset = 0;

    private Vector3 desiredPos;
    private Quaternion desiredRot;
    private float desiredFov;


    private void Start()
    {
        transform.position = titlePos.pos;
        transform.rotation = titlePos.rot;

        foreach (Camera camera in cameras)
        {
            camera.fieldOfView = titlePos.fov;
        }

        desiredPos = titlePos.pos;
        desiredFov = titlePos.fov;
        desiredRot = titlePos.rot;

        lerpSpeed = titleLerpSpeed;

        MouseTracker.Instance.BlockPlaceEvent.AddListener(OnBlockPlace);
        GameManager.Instance.GameStartEvent.AddListener(OnGameStart);
        GameManager.Instance.GameEndEvent.AddListener(OnGameEnd);
    }

    private void OnGameEnd()
    {
        desiredPos = titlePos.pos;
        desiredFov = titlePos.fov;
        desiredRot = titlePos.rot;
    }

    private void OnGameStart()
    {
        lerpSpeed = titleLerpSpeed;

        highestZOffset = 0;

        desiredPos = closestPos.pos;
        desiredFov = closestPos.fov;
        desiredRot = closestPos.rot;
    }

    private void Update()
    {

        Vector3 currentPos = transform.position;

        if (currentPos != desiredPos)
        {
            Vector3 newPos = Vector3.Lerp(currentPos, desiredPos, Time.deltaTime * lerpSpeed);
            transform.position = newPos;
        }

        Quaternion currentRot = transform.rotation;
        if(currentRot != desiredRot)
        {
            Quaternion newRot = Quaternion.Lerp(currentRot, desiredRot, Time.deltaTime * lerpSpeed);
            transform.rotation = newRot;
        }

        foreach (Camera camera in cameras)
        {
            float newFov = Mathf.Lerp(camera.fieldOfView, desiredFov, Mathf.Clamp01(Time.deltaTime * lerpSpeed));

            camera.fieldOfView = newFov;
        }


    }

    private void OnBlockPlace(Vector3 pos)
    {

        lerpSpeed = ingameLerpSpeed;

        float placedZLevel = pos.z;

        if (placedZLevel > highestZOffset)
        {
            highestZOffset = Mathf.Min(placedZLevel, GridManager.Instance.highestGridZ);

            float lowestZ = closestPos.pos.z; //15
            float highestZ = furthestPos.pos.z; //20

            float difference = highestZ - lowestZ;
            float ratio = (highestZOffset / GridManager.Instance.highestGridZ);

            Vector3 newDesiredPos = Vector3.Lerp(closestPos.pos, furthestPos.pos, ratio);
            float newDesiredFov = Mathf.Lerp(closestPos.fov, furthestPos.fov, ratio);

            desiredPos = newDesiredPos;
            desiredFov = newDesiredFov;
        }

    }

}

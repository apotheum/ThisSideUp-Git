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

    [SerializeField] Camera[] cameras;

    [SerializeField] private float lerpSpeed=1.0f;

    private float highestZOffset = 0;

    private Vector3 desiredPos;
    private float desiredFov;


    private void Start()
    {
        transform.position = closestPos.pos;

        foreach (Camera camera in cameras)
        {
            camera.fieldOfView = closestPos.fov;
        }

        desiredPos = closestPos.pos;
        desiredFov = closestPos.fov;

        MouseTracker.Instance.BlockPlaceEvent.AddListener(OnBlockPlace);
    }

    private void Update()
    {

        Vector3 currentPos = transform.position;

        if (currentPos != desiredPos)
        {
            Vector3 newPos = Vector3.Lerp(currentPos, desiredPos, Mathf.Clamp01(Time.deltaTime * lerpSpeed));


            transform.position = newPos;
        }

        foreach (Camera camera in cameras)
        {
            float newFov = Mathf.Lerp(camera.fieldOfView, desiredFov, Mathf.Clamp01(Time.deltaTime * lerpSpeed));

            camera.fieldOfView = newFov;
        }


    }

    private void OnBlockPlace(Vector3 pos)
    {
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

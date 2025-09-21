using System;
using System.Runtime.CompilerServices;
using ThisSideUp.Boxes.Core;
using UnityEngine;

public class ProgressLighting : MonoBehaviour
{
    [SerializeField] private Quaternion startRotation;
    [SerializeField] private Quaternion endRotation;
    [SerializeField] private Vector3 startPos;

    [SerializeField] private Color startColor;
    [SerializeField] private Color endColor;
    [SerializeField] private Vector3 endPos;

    [SerializeField] private float transitionSpeed = 1.0f;
    [SerializeField] Light currentLight;

    private ColorPositionData startData;
    private ColorPositionData endData;

    private ColorPositionData currentData;
    private ColorPositionData desiredData;

    void Start()
    {
        startData = new ColorPositionData(startColor, startRotation, startPos);
        endData = new ColorPositionData(endColor, endRotation, endPos);

        currentData = startData;
        desiredData = startData;

        MouseTracker.Instance.BlockPlaceEvent.AddListener(UpdateCurrentColor);
        GameManager.Instance.GameResetEvent.AddListener(OnGameReset);
    }

    private float highestZOffset = 0;

    private void OnGameReset()
    {
        highestZOffset = 0;
        desiredData = startData;
    }


    void UpdateCurrentColor(Vector3 pos) {

        Debug.Log("Updating color level");
        float placedZLevel = pos.z;

        if (placedZLevel > highestZOffset)
        {
            highestZOffset = Mathf.Min(placedZLevel, GridManager.Instance.highestGridZ);

            float ratio = (highestZOffset / GridManager.Instance.highestGridZ);

            Color nextColor = Color.Lerp(startData.c, endData.c, ratio);
            Quaternion nextRotation=Quaternion.Lerp(startData.r, endData.r, ratio);
            Vector3 nextPos=Vector3.Lerp(startData.p, endData.p, ratio);

            desiredData.c = nextColor;
            desiredData.r = nextRotation;
            desiredData.p = nextPos;
        }
    }

    private void LateUpdate()
    {
        //Rotation
        Quaternion currentRotation = transform.rotation;
        Quaternion desiredRotation = desiredData.r;
        Quaternion newRotation = Quaternion.Lerp(currentRotation, desiredRotation, Mathf.Clamp01(Time.deltaTime * transitionSpeed));
        transform.rotation=newRotation;
        currentData.r = newRotation;

        Color currentColor = currentData.c;
        Color desiredColor = desiredData.c;
        Color newColor = Color.Lerp(currentColor, desiredColor, transitionSpeed * Time.deltaTime);

        currentData.c = newColor;
        currentLight.color= newColor;

        Vector3 currentPos = currentData.p;
        Vector3 desiredPos = desiredData.p;
        Vector3 newPosition = Vector3.Lerp(currentPos, desiredPos, transitionSpeed * Time.deltaTime);
        transform.position=newPosition;
        currentData.p = newPosition;
    }

    struct ColorPositionData
    {
        public Color c;
        public Quaternion r;
        public Vector3 p;

        public ColorPositionData(Color color, Quaternion rotation, Vector3 pos)
        {
            this.c = color;
            this.r = rotation;
            this.p = pos;
        }
    }
    
}

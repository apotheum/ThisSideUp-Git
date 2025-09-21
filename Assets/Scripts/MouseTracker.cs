using System.Collections.Generic;
using ThisSideUp.Boxes;
using ThisSideUp.Boxes.Core;
using ThisSideUp.Boxes.Effects;
using UnityEngine;
using UnityEngine.Events;
using static UnityEditor.PlayerSettings;

namespace ThisSideUp.Boxes.Core
{
    public class MouseTracker : MonoBehaviour
    {
        private static MouseTracker instance;
        public static MouseTracker Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject instanceObject = new GameObject("Mouse Tracker Object");
                    instance = instanceObject.AddComponent<MouseTracker>();
                }

                return instance;
            }
        }

        //Singleton initialization
        private void Awake()
        {

            if (instance == null)
            {
                instance = this;
            }

            else
            {
                Destroy(gameObject);
                return;
            }

        }

        private Camera cam;
        [SerializeField] private LayerMask mask;
        [SerializeField] public GameObject debugIndicator;

        private Vector3 lastHoveredPosition;

        //When a block gets placed.
        public UnityEvent<Vector3> BlockPlaceEvent = new UnityEvent<Vector3>();

        //Highest Z level any block has been placed.
        public float highestKnownZ = 0;

        /* USER SETTINGS */
        private MovementMode movementMode;
        private void OnSettingsUpdate()
        {
            movementMode = ControlSettings.Instance.movementMode;

            //If the movement mode changes to KEYBOARD, the cursor position is auto-centered and snapped to grid
            if (movementMode == MovementMode.Keyboard)
            {
                debugIndicator.transform.position = BoxUtils.roundToGrid(
                    new Vector3(GridManager.Instance.gridWidthHeight / 2, GridManager.Instance.gridWidthHeight / 2, 1.0f)
                    );

            }

        }

        private MovingBlock selectedBlock;

        //Deny the next movement; used to deny additional input until all action keys are released.
        //Action keys have a gravity of 1000, so this is usually within a frame.
        bool denyNextMovement = false;

        //Select a MovingBlock component on a BLOCK INSTANCE gameobject.
        public void SelectBlock(MovingBlock block)
        {
            if (selectedBlock != null)
            {
                PlaceCurrentBlock();
            }

            selectedBlock = block;

            block.StartPlacing();

            block.transform.position = lastHoveredPosition;
            GridManager.Instance.FindClampedLocationInGrid(lastHoveredPosition, selectedBlock);

            Debug.Log("Selected block '"+block.gameObject.name+"'");
        }

        //Deselect a movingblock. Does not place the block.
        public void DeselectBlock()
        {
            selectedBlock.StopPlacing();

            selectedBlock = null;           
            Debug.Log("Deselected block");
        }

        //Place the current block.
        public void PlaceCurrentBlock()
        {
            GameObject selectedBlockObject = selectedBlock.gameObject;

            BoxInstance selectedInstance=selectedBlock.GetComponent<BoxInstance>();            

            //Do block gravity calculation FIRST
            BlockGravity.Instance.CheckGravity(selectedInstance);

            //Mark the block as Placed; it can no longer follow the cursor
            selectedBlock.StopPlacing();

            //Invoke BlockPlaceEvent (MIGHT REMOVE)
            BlockPlaceEvent.Invoke(selectedBlock.transform.position);

            CalcHighestKnownZ(selectedBlock.transform.position);

            DeselectBlock();
        }

        void CalcHighestKnownZ(Vector3 pos)
        {

            float placedZLevel = pos.z;

            if (placedZLevel > highestKnownZ)
            {
                highestKnownZ = Mathf.Min(placedZLevel, GridManager.Instance.highestGridZ);
            }
        }


        void Start()
        {
            cam = Camera.main;

            //Register OnControlSettingsUpdate event
            ControlSettings.Instance.OnControlSettingsUpdate.AddListener(OnSettingsUpdate);

            //Update settings from ControlSettings for the first time
            OnSettingsUpdate();
        }

        void Update()
        {

            //Mouse movement
            if (movementMode == MovementMode.Mouse)
            {
                Vector3 mousePos = Input.mousePosition;

                Ray ray = cam.ScreenPointToRay(mousePos);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100, mask))
                {
                    Vector3 roundedPoint = BoxUtils.roundToGrid(hit.point);
                    HandleRotate(roundedPoint);

                    if ((lastHoveredPosition == null) || (lastHoveredPosition != roundedPoint))
                    {
                        lastHoveredPosition = roundedPoint;
                        debugIndicator.transform.position = lastHoveredPosition;

                        //Validate movement of the attached block and move it
                        if (selectedBlock != null)
                        {
                            selectedBlock.transform.position = roundedPoint;
                            GridManager.Instance.FindClampedLocationInGrid(roundedPoint, selectedBlock);
                        }
                    }

                    Debug.DrawLine(transform.position, hit.point, Color.green);
                    Debug.DrawLine(transform.position, roundedPoint, Color.yellow);
                }
            }

            //Block placement
            if (Input.GetMouseButtonDown(2))
            {
                if(selectedBlock != null) {
                    Debug.Log("Attempting to place...");

                    PlaceCurrentBlock();
                }

            }

        }

        //Handle rotation; independent of mouse and keyboard movement
        private void HandleRotate(Vector3 roundedPoint)
        {

            float horizontal = Input.GetAxis("Horizontal Rotate");
            float vertical = Input.GetAxis("Vertical Rotate");

            if ((!denyNextMovement) && (selectedBlock != null))
            {
                if (horizontal != 0)
                {
                    denyNextMovement = true;

                    if (horizontal > 0)
                    {
                        //Rotate to the right
                        selectedBlock.transform.Rotate(new Vector3(0.0f, 0.0f, 90.0f));
                        selectedBlock.transform.position = roundedPoint;
                        GridManager.Instance.FindClampedLocationInGrid(roundedPoint, selectedBlock);
                        Debug.Log("Right");
                    }
                    else
                    {
                        //Rotate to the left
                        selectedBlock.transform.Rotate(new Vector3(0.0f, 0.0f, -90.0f));
                        selectedBlock.transform.position = roundedPoint;
                        GridManager.Instance.FindClampedLocationInGrid(roundedPoint, selectedBlock);
                        Debug.Log("Left");
                    }

                } 
            }
            else
            {
                if ((horizontal == 0) && (vertical == 0))
                {
                    denyNextMovement = false;
                }
            }
        }


        //

    }




}
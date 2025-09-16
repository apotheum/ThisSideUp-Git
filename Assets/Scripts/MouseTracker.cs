using ThisSideUp.Boxes;
using ThisSideUp.Boxes.Core;
using ThisSideUp.Boxes.Effects;
using UnityEngine;

namespace ThisSideUp.Boxes.Core
{
    public class MouseTracker : MonoBehaviour
    {
        private Camera cam;
        [SerializeField] private LayerMask mask;
        [SerializeField] public GameObject debugIndicator;

        private Vector3 lastHoveredPosition;

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
            selectedBlock = block;
            Debug.Log("Selected block '"+block.gameObject.name+"'");
        }

        //Deselect a movingblock. Does not place the block.
        public void DeselectBlock()
        {
            selectedBlock = null;
            Debug.Log("Deselected block");
        }

        //Place the current block.
        public void PlaceCurrentBlock()
        {
            GridManager.Instance.FindClampedLocationInGrid(BoxUtils.roundToGrid(selectedBlock.transform.position), selectedBlock);

            DelayedFollow follow = selectedBlock.childFollow;

            follow.StopFollowing();
            follow.gameObject.transform.parent = transform;

            selectedBlock.blockState = BlockState.Placed;
            selectedBlock.enabled = false;

            DeselectBlock();
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

            //Keyboard movement; buggy for now
            else
            {
                bool updating = false;

                if (selectedBlock != null)
                {
                    float horizontalMove = Input.GetAxis("Horizontal Move");
                    float verticalMove = Input.GetAxis("Vertical Move");

                    Vector3 dir = selectedBlock.transform.position;

                    if (!denyNextMovement)
                    {
                        if (horizontalMove != 0)
                        {
                            denyNextMovement = true;
                            updating = true;

                            if (horizontalMove > 0)
                            {
                                dir.x += 1;
                                Debug.Log("Right");
                            }
                            else
                            {
                                dir.x -= 1;
                                Debug.Log("Left");
                            }
                        }
                        if (verticalMove != 0)
                        {
                            updating = true;
                            denyNextMovement = true;

                            if (verticalMove > 0)
                            {
                                dir.y += 1;
                                Debug.Log("Up");
                            }
                            else
                            {
                                dir.y -= 1;
                                Debug.Log("Down");
                            }
                        }
                    }
                    else
                    {
                        if ((horizontalMove == 0) && (verticalMove == 0))
                        {
                            denyNextMovement = false;
                        }
                    }

                    if (updating)
                    {
                        Vector3 roundedPoint = BoxUtils.roundToGrid(dir);
                        HandleRotate(roundedPoint);

                        if ((lastHoveredPosition == null) || (lastHoveredPosition != roundedPoint))
                        {
                            lastHoveredPosition = roundedPoint;
                            debugIndicator.transform.position = lastHoveredPosition;

                            selectedBlock.transform.position = roundedPoint;
                            Debug.Log("HEHE");
                            GridManager.Instance.FindClampedLocationInGrid(roundedPoint, selectedBlock);
                        }
                    }
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
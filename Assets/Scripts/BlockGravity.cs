using NUnit.Framework;
using System.Collections.Generic;
using ThisSideUp.Boxes;
using ThisSideUp.Boxes.Core;
using UnityEngine;
using UnityEngine.Events;

public class BlockGravity : MonoBehaviour
{
    private static BlockGravity instance;
    public static BlockGravity Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject instanceObject = new GameObject("Block Gravity Object");
                instance = instanceObject.AddComponent<BlockGravity>();
            }

            return instance;
        }
    }

    //Singleton initialization
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); return; }
    }

    /* Block Gravity */
    //The Gravity Check event. This gets called through two methods:
    //1. A player places the block. Checks once.
    //2. A block finishes its gravity routine.

    //When a BoxInstance finishes setting up, it adds a listener for this event.
    //When the event fires, it checks its surroundings for whether it needs to fall or not.
    //If it needs to fall, the block adds itself to the "gravity queue."
    public UnityEvent GlobalGravityCheckEvent = new UnityEvent();

    //The Gravity Queue. A BoxInstance in here is scheduled to be moved.
    //The only way a block can be added here is through the QueueGravity() method.
    private List<GravityPos> gravityQueue = new List<GravityPos>();

    //Check gravity for this block.
    public void CheckGravity(BoxInstance instance)
    {
        GameObject instanceObject= instance.gameObject;
        Vector3 initialPos=instanceObject.transform.position;

        MovingBlock thisBlock=instanceObject.GetComponent<MovingBlock>();
        BoxCollider[] colliders=instanceObject.GetComponents<BoxCollider>();

        Vector3 movePos = initialPos;

        //Whether we've found the next place. Becomes true as soon as at least one grid space is occupied by another block.
        bool foundNextPlace = false;


        while (!foundNextPlace)
        {
            //Grid spaces in the colliders at their current location.
            List<Vector3> gridSpacesInCollider=BoxUtils.GridSpacesInColliders(colliders);

            foreach (Vector3 gridSpace in gridSpacesInCollider)
            {
                //Grid space one space lower on the Y level.
                Vector3 oneSpaceDown=new Vector3(gridSpace.x,gridSpace.y-1, gridSpace.z);

                List<GameObject> otherBlocksAtThisPosition = BoxUtils.FindBlocksAtPosition(oneSpaceDown, thisBlock);

                bool foundExistingBlockHere = false;

                //If there are more blocks at this position...
                if (otherBlocksAtThisPosition.Count > 0)
                {
                    foreach(GameObject otherBlock in otherBlocksAtThisPosition)
                    {
                        if (otherBlock != thisBlock)
                        {
                            //Debug.Log("Found other block at this position; '" + otherBlock.name + "' (my name is '" + thisBlock.name + "')");
                            foundExistingBlockHere = true;
                            break;
                        }
                    }
                }

                //If we didn't find any other blocks at the lower positions, check if the coords are at least inside the grid.
                if (!foundExistingBlockHere)
                {
                    //If this space is outside the grid.
                    //Then foundNextPlace becomes true.
                    if (!BoxUtils.PositionIsInsideXY(oneSpaceDown))
                    {
                        Debug.Log("Hit floor");
                        foundNextPlace = true;
                        break;
                    }

                }

                //But if we did find a block here, no need to check the coords.
                else
                {
                    foundNextPlace = true;
                    break;
                }

            }

            //If we haven't found a new place, move the block one space down and do the check again.
            if (!foundNextPlace)
            {
                movePos = new Vector3(movePos.x, movePos.y - 1, movePos.z);
                instanceObject.transform.position = movePos;
            }

        }

        //We reset the position of this object because it needs to be properly queued for gravity purposes.
        instanceObject.transform.position = initialPos;

        //If the position to move is different than the original pos, there is gravity to update.
        if (movePos != initialPos)
        {
            //Queue gravity.
            QueueGravity(instance, movePos);
        }
        else
        {
            //Update the grid
            FinishPlacement(instance);
            UpdateGrid();
            Debug.Log("MovePos is the same as initialPos");
        }

    }

    //Queue this BoxInstance for gravity with a desired position.
    //The position is calculated in a BoxInstance object.
    public void QueueGravity(BoxInstance instance, Vector3 desiredPos)
    {
        GravityPos gravityPos = new GravityPos();
        gravityPos.instance = instance;
        gravityPos.desiredPos = desiredPos;

        gravityQueue.Add(gravityPos);

        gravityUpdateComplete = false;
    }

    [SerializeField] private float fallSpeed = 0.1f;

    //Whether the gravity update is completely finished.
    //This is true by default. It becomes false when ANY block enters the Gravity Queue.
    private bool gravityUpdateComplete = true;

    private BoxInstance currentInstance;
    private Vector3 desiredPos;

    private void LateUpdate()
    {

        if (gravityUpdateComplete) { return; }

        if (currentInstance != null)
        {
            Vector3 pos = currentInstance.transform.position;
            Vector3 newPos=new Vector3(pos.x,pos.y-(fallSpeed*Time.deltaTime), pos.z);

            if (newPos.y > desiredPos.y)
            {
                currentInstance.transform.position=newPos;
            }
            else
            {
                currentInstance.transform.position=BoxUtils.roundToGrid(pos);
                currentInstance = null;
            }
        }
        else
        {
            if (gravityQueue.Count > 0)
            {
                GravityPos pos = gravityQueue[0];

                currentInstance = pos.instance;
                desiredPos = pos.desiredPos;

                FinishPlacement(currentInstance);

                gravityQueue.Remove(pos);
            }
            else
            {
                //All gravity operations ceased; allow mouse movement and recalculate grid space
                UpdateGrid();

                gravityUpdateComplete = true;
            }
        }

    }
    
    //Finish placing the block.
    public void FinishPlacement(BoxInstance instance)
    {
        Debug.Log("Finalizing placement for '" + instance.gameObject.name+"'");

        MovingBlock movingBlock=instance.GetComponent<MovingBlock>();

        //Tell the MovingBlock component to re-parent its separated children.
        movingBlock.StopDisplayAnchor();

        //The block gameObject returns to layer 6, the PointerValid layer
        instance.gameObject.layer = 6;

        //Update the Z Cache in GridManager of the highest placed Z for this XY coordinate. Might remove.
        GridManager.Instance.UpdateZCache(instance.gameObject);
    }

    private void UpdateGrid()
    {
        GridManager.Instance.RecalculateOccupiedSpaces();
        GridManager.Instance.RecalculateSealedAndUnsealedFreeSpaces();
    }

    struct GravityPos
    {
        public BoxInstance instance;
        public Vector3 desiredPos;

    }

}
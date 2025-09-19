using System;
using System.Threading;
using ThisSideUp.Boxes.Core;
using ThisSideUp.Boxes.Effects;
using UnityEngine;
using UnityEngine.Events;
using static ThisSideUp.Boxes.Core.BoxIDs;


namespace ThisSideUp.Boxes
{
    //The block state. Controls behavior of the block.
    //All blocks start in the WaitingForEnable state; this prevents all behavior and disables the script.
    //A BoxInstance script re-enables the script and changes the blockstate.

    public enum BlockState
    {
        Inventory,          //The box is in the player's inventory.
        Placing,            //The box is being placed and is being moved by the mouse.
        Placed,             //The box is placed.
        WaitingForEnable    //Waiting to be enabled by a BoxInstance script. This disables the script on load if it isn't already.
    }

    public class MovingBlock : MonoBehaviour
    {

        public bool placed { get; private set; }

        public DelayedFollow childFollow;

        private MouseTracker tracker;

        //The current block state, event for when it changes, and method for changing it.
        //See comment above enum for a description of each state.
        public BlockState blockState;
        public UnityEvent OnBlockStateChanged;
        public void ChangeState(BlockState newState)
        {
            this.blockState = newState;
            OnBlockStateChanged.Invoke();
        }

        public void LogStateChange()
        {
            Debug.Log("State changed to " + blockState);
        }

        private void OnTriggerEnter(Collider coll)
        {
            if (blockState != BlockState.Placing) { return; }

            if (coll != null)
            {
                MovingBlock movingBlock = coll.GetComponent<MovingBlock>();
                if (movingBlock != null)
                {
                    //Debug.Log("Collider: " + coll.gameObject.name);

                }
            }

        }

        //Start placing block; decouples DISPLAY ANCHOR from BOX INSTANCE to allow smooth gliding.
        public void StartPlacing()
        {
            childFollow.toFollow = gameObject.transform;
            childFollow.gameObject.transform.parent = null;

            blockState = BlockState.Placing;

            Transform thisTransform=gameObject.transform;
            //if(thisTransform != null) { Debug.Log("baby need smoko!!!!!!!!"); }

            childFollow.StartFollowing(gameObject.transform);

            //Debug.Log("Selected");
        }

        //Stop placing block; makes block "placed" and disables the MovingBlock component.
        public void StopPlacing()
        {
            blockState = BlockState.Placed;
            enabled = false;
        }

        //Stop display anchor; reparents DISPLAY ANCHOR to BOX INSTANCE.
        public void StopDisplayAnchor()
        {
            childFollow.StopFollowing();
            childFollow.gameObject.transform.parent = transform;

        }

        //Converts all enabled BoxColliders into triggers. Enables certain box interactions.
        //Legacy code that might be removed.
        void Start()
        {
            if (blockState == BlockState.WaitingForEnable) { enabled = false; }

            foreach (BoxCollider coll in GetComponents<BoxCollider>())
            {
                if (coll.enabled)
                {
                    coll.isTrigger = true;
                }
            }

            tracker = Camera.main.GetComponent<MouseTracker>();

        }


        // Update is called once per frame
        void Update()
        {
            if (blockState != BlockState.Inventory) { return; }

            //Dev key to select the box. This is the same as selecting it from the inventory.
            //Set MouseTracker's currently selected block to this block.
            //We unparent the child Display Anchor and allow it to glide to the position of the block.
            //if (Input.GetKeyDown(KeyCode.F))
            {
                if (!placed)
                {

                    //if (Input.GetKeyDown(KeyCode.G))
                    //{
                    //    MouseTracker.Instance.SelectBlock(this);
                    //    placed = true;

                    //}

                }
            }
        }
    }
}


using NUnit.Framework.Constraints;
using ThisSideUp.Boxes;
using ThisSideUp.Boxes.Effects;
using UnityEngine;

public class BoxInstance : MonoBehaviour
{

    //A BoxInstance is an empty GameObject that uses the colliders and children of a reference GameObject found in BlockIDs.Instance.blocks.
    //When InstantiateBox() is called, this script obtains the GameObject from its input field (the "Display Prefab"), then does two things:

    //1. Add all child objects of the Display Prefab to THIS object's "Display Anchor." 
    //      The DISPLAY ANCHOR smoothly glides into position, rather than snaps, for smooth movement.
    //      It has no colliders; the BOX INSTANCE does, and it snaps into position for accurate movement.
    //      The DISPLAY ANCHOR is the child of the BOX INSTANCE at all times EXCEPT when being placed - this is so it can glide independent of the parent transform.

    //2. Copy all enabled BoxColliders, and their offsets, to THIS object.
    //      THIS object, the BOX INSTANCE, 

    //The DISPLAY ANCHOR, which is a child of this object, the BOX INSTANCE.
    [SerializeField] Transform displayAnchor;
    [SerializeField] MovingBlock movingBlock;

    public void InstantiateBox(GameObject displayPrefab)
    {
        
        //Add each BoxCollider from the DISPLAY PREFAB to this object, the BOX INSTANCE.
        foreach(BoxCollider coll in displayPrefab.GetComponents<BoxCollider>())
        {
            if (coll.enabled)
            {
                BoxCollider toAdd=gameObject.AddComponent<BoxCollider>();
                toAdd.size = coll.size;
                toAdd.center=coll.center;
                toAdd.isTrigger = true;
            }
        }

        //Add each child of the DISPLAY PREFAB to the DISPLAY ANCHOR, a child of this object, the BOX INSTANCE.
        foreach (Transform child in displayPrefab.transform)
        {
            Debug.Log("Adding Child:" + child.gameObject.name);

            GameObject childToParent = Instantiate(child.gameObject);
            childToParent.transform.parent = displayAnchor.transform;
        }

        //Configure, then enable, the MovingBlock component attached to this object.
        movingBlock.childFollow = displayAnchor.GetComponent<DelayedFollow>();
        movingBlock.blockState = BlockState.Inventory;
        movingBlock.enabled = true;
    }

}

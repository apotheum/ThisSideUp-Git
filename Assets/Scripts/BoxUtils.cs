using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ThisSideUp.Boxes.Core
{
    public class BoxUtils
    {
        public static Vector3 roundToGrid(Vector3 pos)
        {
            Vector3 roundedPoint = new Vector3(
                Mathf.RoundToInt(pos.x),
                Mathf.RoundToInt(pos.y),
                Mathf.RoundToInt(pos.z));

            return roundedPoint;
        }


        //This method obtains the worldspace position of each vertex in a BoxCollider.
        //BoxColliders can have variable width + height, and they can intersect each other.
        public static Vector3[] WorldspaceColliderVertices(BoxCollider coll)
        {
            Vector3[] worldspaceVerts = new Vector3[8];

            Vector3 center = coll.center;
            Vector3 size = coll.size;
            Transform collTransform = coll.transform;

            //To obtain both bounds of the collider, we start from the center and subtract or add HALF the size of the collider.
            Vector3 localMin = center - (size * 0.5f);
            Vector3 localMax = center + (size * 0.5f);

            //LocalMin and localMax contain both X Y and Z coordinates
            worldspaceVerts[0] = collTransform.TransformPoint(new Vector3(localMin.x, localMin.y, localMin.z));
            worldspaceVerts[1] = collTransform.TransformPoint(new Vector3(localMin.x, localMax.y, localMin.z));
            worldspaceVerts[2] = collTransform.TransformPoint(new Vector3(localMax.x, localMin.y, localMin.z));
            worldspaceVerts[3] = collTransform.TransformPoint(new Vector3(localMax.x, localMax.y, localMin.z));

            worldspaceVerts[4] = collTransform.TransformPoint(new Vector3(localMax.x, localMax.y, localMax.z));
            worldspaceVerts[5] = collTransform.TransformPoint(new Vector3(localMax.x, localMin.y, localMax.z));
            worldspaceVerts[6] = collTransform.TransformPoint(new Vector3(localMin.x, localMin.y, localMax.z));
            worldspaceVerts[7] = collTransform.TransformPoint(new Vector3(localMin.x, localMax.y, localMax.z));


            return worldspaceVerts;
        }

        public static bool PositionIsInsideXY(Vector3 pos)
        {
            bool insideGrid = false;

            float widthHeight = GridManager.Instance.gridWidthHeight;

            float x = pos.x;
            float y = pos.y;

            if ((x <= widthHeight) && (x >= 0))
            {
                if ((y <= widthHeight) && (y >= 0))
                {
                    insideGrid = true;
                }
            }

            return insideGrid;
        }

        //Get a reference to all GameObjects with MovingBlock components at a position.
        public static List<GameObject> FindBlocksAtPosition(Vector3 gridPoint, MovingBlock selectedBlock)
        {

            List<GameObject> blocksFound = new List<GameObject>();


            Collider[] collidersHere = Physics.OverlapSphere(gridPoint, 0.01f);

            if (collidersHere.Length > 0)
            {
                foreach (Collider collider in collidersHere)
                {
                    if (collider is BoxCollider)
                    {
                        //If the block we're colliding with isn't us...
                        GameObject thisBlock = collider.gameObject;
                        if (thisBlock != selectedBlock.gameObject)
                        {

                            //And if that block has a MovingBlock component (and is therefore a block...)
                            MovingBlock blockCollide = thisBlock.GetComponent<MovingBlock>();
                            if (blockCollide != null)
                            {

                                //If we haven't found this block before...
                                if (!blocksFound.Contains(blockCollide.gameObject))
                                {
                                    blocksFound.Add(blockCollide.gameObject);
                                }
                            }
                        }
                    }

                }
            }

            return blocksFound;
        }


        //This method obtains every Worldspace Grid position inside the object's colliders.
        public static List<Vector3> GridSpacesInColliders(BoxCollider[] colliders)
        {
            //A List containing every possible grid position INSIDE each collider.
            List<Vector3> gridSpacesInCollider = new List<Vector3>();


            //Calculate the maximum X and Y coordinate of each vertex in any colliders attached to the parent object.
            //This results in a rectangle shape that takes into account all of the colliders;
            //later, we check its extents if they are outside grid space.
            foreach (BoxCollider coll in colliders)
            {
                if (coll.enabled)
                {
                    Vector3[] worldspaceVerts = WorldspaceColliderVertices(coll);

                    //Local min and max of THIS collider.
                    Vector3 colliderLocalMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                    Vector3 colliderLocalMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);


                    foreach (Vector3 pos in worldspaceVerts)
                    {
                        //Update colliderLocalMin and Max to the min and max coordinates of THIS collider.
                        //This allows us to iterate through it later on and find valid grid positions to check other blocks against.
                        if (pos.x > colliderLocalMax.x) { colliderLocalMax.x = pos.x; }
                        if (pos.y > colliderLocalMax.y) { colliderLocalMax.y = pos.y; }
                        if (pos.z > colliderLocalMax.z) { colliderLocalMax.z = pos.z; }

                        if (pos.x < colliderLocalMin.x) { colliderLocalMin.x = pos.x; }
                        if (pos.y < colliderLocalMin.y) { colliderLocalMin.y = pos.y; }
                        if (pos.z < colliderLocalMin.z) { colliderLocalMin.z = pos.z; }
                    }

                    //With the local min and max, we iterate through every possible grid coordinate to get a list.

                    //Identify min and max of this local collider
                    //localMinCornerSphere.transform.position = colliderLocalMin;
                    //localMaxCornerSphere.transform.position = colliderLocalMax;

                    //Pick a point. This point is guaranteed to be INSIDE the box at the corner of the lowest coordinate of the collider.
                    Vector3 firstInsidePoint = colliderLocalMin + new Vector3(0.5f, 0.5f, 0.5f);

                    //Starting from that point, we iterate through each XYZ coordinate up to the maximum.
                    //This lets us identify valid grid positions inside the collider, and thus, the rest of the box.
                    for (float insideX = firstInsidePoint.x; insideX <= colliderLocalMax.x; insideX++)
                    {
                        for (float insideY = firstInsidePoint.y; insideY <= colliderLocalMax.y; insideY++)
                        {
                            for (float insideZ = firstInsidePoint.z; insideZ <= colliderLocalMax.z; insideZ++)
                            {
                                Vector3 insidePoint = new Vector3(insideX, insideY, insideZ);

                                gridSpacesInCollider.Add(insidePoint);
                            }
                        }
                    }
                }
            }

            return gridSpacesInCollider;
        }

    }
}

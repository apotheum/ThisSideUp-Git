using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThisSideUp.Boxes.Core
{
    public class GridManager : MonoBehaviour
    {
        private static GridManager instance;
        public static GridManager Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject instanceObject = new GameObject("Grid Manager Object");
                    instance = instanceObject.AddComponent<GridManager>();
                }

                return instance;
            }
        }


        [SerializeField] private GameObject minCornerSphere;
        [SerializeField] private GameObject maxCornerSphere;

        [SerializeField] private GameObject localMinCornerSphere;
        [SerializeField] private GameObject localMaxCornerSphere;

        [SerializeField] private LayerMask boxColliderLayers;

        private Vector3 lastValidPosition;

        public int gridWidthHeight = 7;

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

        private List<Vector3> insideCollider = new List<Vector3>();

        void OnDrawGizmos()
        {
            if (insideCollider != null)
            {
                Gizmos.color = Color.green;
                foreach (Vector3 point in insideCollider)
                {
                    Gizmos.DrawSphere(point, 0.1f); // Draw a small sphere at each point
                }
            }
        }

        public void FindClampedLocationInGrid(Vector3 pointerLoc, MovingBlock selectedBlock)
        {
            insideCollider.Clear();

            //This method does two things:
            //1. Clamps the location of the parent transform so all of the block's colliders fit inside the grid.
            //2. Finds the next available grid position on the Z AXIS ONLY that isn't occupied. 

            //It makes NO CHECKS for whether ANY of the colliders intersect with other blocks.
            //That is handled elsewhere. Checks for whether the X and Y grid spaces are occupied must also be done elsewhere.

            //This method is called through MouseTracker.cs.
            Vector3 parentPos = selectedBlock.transform.position;

            //parentPos.x = parentPos.x - 0.5f;
            //parentPos.y = parentPos.y - 0.5f;

            //Values by which the box parent should shift.
            //They increase if a collider vert is outside the bounds.
            float shiftX = 0;
            float shiftY = 0;
            float shiftZ = 0; //This is mainly unused for grid purposes; it's for block collision

            //Absolute minimum and maximum worldspace extent of every collider.
            //If the minX and
            float minX = int.MaxValue;
            float minY = int.MaxValue;
            float minZ = int.MaxValue;

            float maxX = int.MinValue;
            float maxY = int.MinValue;
            float maxZ = int.MinValue;


            List<Collider> hitColliders = new List<Collider>();

            //A List containing every possible grid position INSIDE each collider.
            List<Vector3> gridSpacesInCollider = new List<Vector3>();


            //Calculate the maximum X and Y coordinate of each vertex in any colliders attached to the parent object.
            //This results in a rectangle shape that takes into account all of the colliders;
            //later, we check its extents if they are outside grid space.
            foreach (BoxCollider coll in selectedBlock.GetComponents<BoxCollider>())
            {
                if (coll.enabled)
                {
                    Vector3[] worldspaceVerts = WorldspaceColliderVertices(coll);

                    //Local min and max of THIS collider.
                    Vector3 colliderLocalMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
                    Vector3 colliderLocalMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);


                    foreach (Vector3 pos in worldspaceVerts)
                    {
                        //Update the global min and max coordinates to the total coordinates of ALL colliders.
                        //This allows us to check the grid for any illegal positions, then shift the block back inside.
                        if (pos.x > maxX) { maxX = pos.x; }
                        if (pos.y > maxY) { maxY = pos.y; }
                        if (pos.z > maxZ) { maxZ = pos.z; }

                        if (pos.x < minX) { minX = pos.x; }
                        if (pos.y < minY) { minY = pos.y; }
                        if (pos.z < minZ) { minZ = pos.z; }

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
                    localMinCornerSphere.transform.position = colliderLocalMin;
                    localMaxCornerSphere.transform.position = colliderLocalMax;

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
                                //insideCollider.Add(insidePoint);
                            }
                        }
                    }
                }
            }

            minCornerSphere.transform.position = new Vector3(minX, minY, minZ);
            maxCornerSphere.transform.position = new Vector3(maxX, maxY, maxZ);

            //If MinX or MinY are less than 0, the shift value becomes the absolute value.
            //Example: MinX is -3. The absolute value of MinX is therefore 3. We shift the block by 3 so it ends up at 0 again.
            if (minX < 0) { shiftX = Math.Abs(minX) - 0.5f; }
            if (minY < 0) { shiftY = Math.Abs(minY) - 0.5f; }
            if (minZ < 0) { shiftZ = Math.Abs(minZ) - 0.5f; }

            //If MaxX is greater than the grid height (ie. 8 vs. 7, we subtract the value of MaxX from the grid height.
            //This always returns a negative number, so it can be safely added to the position at the end.
            //There is no MaxZ; placing outside the maximum Z height is the only way to end the game.
            if (maxX > gridWidthHeight) { shiftX = gridWidthHeight - maxX + 0.5f; }
            if (maxY > gridWidthHeight) { shiftY = gridWidthHeight - maxY + 0.5f; }


            //Calculate the next valid Z layer.
            float nextZLayer = pointerLoc.z;



            float minCollisionZLayer = float.MaxValue;

            //Check each grid point for any existing collider gameObjects with a MovingBlock component.
            //This locates any existing blocks, placed or otherwise.
            List<GameObject> blocksFound = new List<GameObject>();

            foreach (Vector3 gridPoint in gridSpacesInCollider)
            {
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

                                        //Block thickness is always at least 1.
                                        float blockThickness = 1;


                                        Vector3[] worldspaceVerts = WorldspaceColliderVertices((BoxCollider)collider);
                                        blockThickness = (float)Mathf.RoundToInt(worldspaceVerts[4].z - worldspaceVerts[0].z);

                                        Debug.Log("THICKNESS OF " + blockThickness);

                                        float blockZ = thisBlock.transform.position.z + blockThickness;
                                        Debug.Log("Block Z:" + blockZ);

                                        if (blockZ < minCollisionZLayer) { minCollisionZLayer = blockZ; }
                                    }
                                }
                            }
                        }

                    }
                }
            }

            if (blocksFound.Count > 0)
            {
                nextZLayer = minCollisionZLayer;

                string blocksCollated = "";
                foreach (GameObject blockFound in blocksFound) { blocksCollated = "'" + blockFound.name + "' "; }

                Debug.Log("Min collision layer:" + minCollisionZLayer);

                Debug.Log("Found blocks: " + blocksCollated);
            }


            Vector3 newPos = new Vector3(
                parentPos.x + shiftX,
                parentPos.y + shiftY,
                nextZLayer
            );

            lastValidPosition = newPos;

            if (selectedBlock.transform.position != newPos)
            {
                //Debug.Log("new pos");
            }

            selectedBlock.transform.position = newPos;


        }

        //This method obtains the worldspace position of each vertex in a BoxCollider.
        //BoxColliders can have variable width + height, and they can intersect each other.
        //Each position is checked for whether it is inside the grid.
        public Vector3[] WorldspaceColliderVertices(BoxCollider coll)
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


        public bool ValidMove(Vector3 pos)
        {
            bool valid = false;

            if (
                (pos.x >= 0) && (pos.x <= gridWidthHeight) &&
                (pos.y >= 0) && (pos.y <= gridWidthHeight))
            {
                //if (!grid.ContainsKey(pos))
                //{
                //    valid = true;
                //}
            }

            return valid;
        }

    }

}
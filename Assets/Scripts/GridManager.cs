using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

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

        //Debug objects
        [SerializeField] private GameObject minCornerSphere;
        [SerializeField] private GameObject maxCornerSphere;

        [SerializeField] private GameObject localMinCornerSphere;
        [SerializeField] private GameObject localMaxCornerSphere;

        [SerializeField] private LayerMask boxColliderLayers;

        public int gridWidthHeight = 7;
        public int highestGridZ = 14;

        //Singleton initialization
        private void Awake()
        {
            if (instance == null) { instance = this; }
            else { Destroy(gameObject); return; }
        }
                
        /* Debug Gizmo */
        //Debug purposes
        private List<Vector3> insideCollider = new List<Vector3>();
        void OnDrawGizmos()
        {
            if (insideCollider != null)
            {
                Gizmos.color = Color.green;
                foreach (Vector3 point in insideCollider)
                {
                    Gizmos.DrawSphere(point, 0.1f); 
                }
            }
        }


        /* Owned Spaces */
        //A space is marked "owned" if a block occupies it.
        //Spaces are marked when a block is placed or updates.
        private List<Vector3> occupiedSpaces= new List<Vector3>();

        public UnityEvent UpdateOccupiedSpace=new UnityEvent();

        //Invoke an event that causes every BoxInstance object, who starts listening to it when spawned, to mark their own coordinates in the list above.
        public void RecalculateOccupiedSpaces()
        {
            occupiedSpaces.Clear();
            UpdateOccupiedSpace.Invoke();
        }

        public void MarkOccupied(Vector3 pos)
        {
            Vector3 roundedPos=BoxUtils.roundToGrid(pos);

            if (!occupiedSpaces.Contains(roundedPos))
            {
                occupiedSpaces.Add(roundedPos);
                //insideCollider.Add(roundedPos);

                Debug.Log("Marking "+ roundedPos.x+", "+ roundedPos.y+", "+roundedPos.z +"("+pos.x+","+pos.y+","+pos.z+") as occupied");
            }
        }

        public void ClearOccupied(Vector3 pos)
        {
            Vector3 roundedPos = BoxUtils.roundToGrid(pos);

            if (occupiedSpaces.Contains(roundedPos))
            {
                occupiedSpaces.Remove(roundedPos);
                //insideCollider.Remove(roundedPos);

                Debug.Log("Clearing occupied space " + roundedPos.x + ", " + roundedPos.y + ", " + roundedPos.z + "(" + pos.x + "," + pos.y + "," + pos.z + ")");
            }
        }

        /* Sealed vs. Unsealed Spaces */
        //A position is considered "sealed" when it has no connection to the outside.
        //This can happen on an individual or collective level when an area of the map is blocked on all sides by boxes.
        //A flood fill algorithm calculates the sealed and unsealed spaces, starting at the highest Z coord + 1 and working inwards.

        //Array containing the unsealed free spaces. Updates through RecalculateSealedAndUnsealedFreeSpaces().
        public List<Vector3> unsealedSpaceCache = new List<Vector3>();
        public List<Vector3> sealedSpaceCache = new List<Vector3>();

        //Calculate the unsealed free spaces through flood fill method. The fill continues until hitting a box.
        //Because of this, we also obtain the location of a "sealed" position; the fill will never find a Vector3 surrounded by boxes.
        //This is used to score points and calculate box Z movement.

        //Keep in mind, this does not store its values in the arrays above.
        //Use RecalculateSealedAndUnsealedFreeSpaces to store in array.
        private List<Vector3> GetUnsealedFreeSpaces()
        {
            List<Vector3> freeSpaces=new List<Vector3>();

            Vector3 scanPos = new Vector3(0, 0, highestGridZ + 1);
            //Debug.Log("Starting scan pos: " + scanPos);

            List<Vector3> queue = new List<Vector3>();    //Spaces we need to visit.
            List<Vector3> visited = new List<Vector3>();  //Spaces we've visited before.

            queue.Add(scanPos); //Start scanning by adding the initial pos to the queue.

            int iteration = 1;

            bool keepGoing = true;

            //This while loop continues the flood fill until the queue is empty.
            //It could easily be cleaned up - I don't care!
            while (keepGoing)
            {
                //Snapshot of the current queue. This is what we loop through on this go around.
                List<Vector3> currentQueue = new List<Vector3>();
                foreach (Vector3 p in queue)
                {
                    currentQueue.Add(p);
                }

                //keepGoing could easily be removed but eh
                if (queue.Count == 0)
                {
                    keepGoing = false;
                }

                //The queue is cleared every round because we store a snapshot of it.
                //If this wasn't here, the loop would continue endlessly
                queue.Clear();

                //For every Vector3 pos in the current queue, which on the first round will always have one position,
                //we check all neighboring coordinates for whether they're in the grid, colliding with a box, already visited, etc.
                foreach (Vector3 pos in currentQueue)
                {
                    List<Vector3> neighbors = new List<Vector3>();

                    float x = pos.x;
                    float y = pos.y;
                    float z = pos.z;

                    Vector3 left =      new Vector3(x+1, y, z);
                    Vector3 right =     new Vector3(x-1, y, z);
                    Vector3 top =       new Vector3(x, y+1, z);
                    Vector3 bottom =    new Vector3(x, y-1, z);
                    Vector3 front =     new Vector3(x, y, z+1);
                    Vector3 back =      new Vector3(x, y, z-1);

                    neighbors.Add(left);
                    neighbors.Add(right);
                    neighbors.Add(top);
                    neighbors.Add(bottom);
                    neighbors.Add(front);
                    neighbors.Add(back);

                    //Loop through all six neighbors of this position.
                    //  If a neighbor:
                    //      Isn't occupied by a block
                    //      Isn't already visited
                    //      Isn't already in the queue
                    //      Is within the grid
                    //  Then it gets added to the queue.
                    //The next iteration of the while loop will loop through all the "valid" neighbors we find here.
                    foreach (Vector3 neighbor in neighbors)
                    {
                        //Whether this neighbor is "safe" - ie unsealed.
                        bool safe = false;

                        float neighborX = neighbor.x;
                        float neighborY = neighbor.y;
                        float neighborZ = neighbor.z;

                        //Neighbor validity logic
                        if (BoxUtils.PositionIsInsideXY(neighbor))
                        {
                            if ((neighborZ <= (highestGridZ + 1)) && (neighborZ >= 0))
                            {
                                if (!occupiedSpaces.Contains(neighbor))
                                {
                                    if (!visited.Contains(neighbor))
                                    {
                                        if (!queue.Contains(neighbor))
                                        {
                                            safe = true;
                                        }
                                    }
                                }
                            }

                        }


                        //If safe is true, we know the neighbor coordinate meets the above commented criteria.
                        //It gets added to the queue so that we can check its neighbors - and so on and so on.
                        if (safe)
                        {
                            freeSpaces.Add(neighbor);
                            queue.Add(neighbor);
                        }
                    }

                    //We have now visited this position, so no need to check it again.
                    visited.Add(pos);
                }

                //Iteration count increases, but can remove this
                iteration++;
            }


            return freeSpaces;
        }

        //Calculate the SEALED free spaces. This runs GetUnsealedFreeSpaces as a prerequisite.
        //Passing no arguments into this method runs GetUnsealedFreeSpaces(); normally this is desired.
        //Since RecalculateSealedAndUnsealedFreeSpaces() runs both functions, no need to run the first one twice.
        public List<Vector3> GetSealedFreeSpaces(List<Vector3> unsealedFreeSpaces)
        {
            List<Vector3> sealedSpaces= new List<Vector3>();


            for (float x = 0; x <= gridWidthHeight; x++)
            {
                for(float y = 0; y <= gridWidthHeight; y++)
                {
                    for(float z = 0; z <= (highestGridZ + 1); z++)
                    {
                        Vector3 pos= new Vector3(x, y, z);
                        
                        //If this position is not in unsealedFreeSpaces, it isn't an unsealed free space.
                        //This makes no checks for occupying objects however.
                        if (!unsealedFreeSpaces.Contains(pos))
                        {
                            //If this space is not occupied by a block...
                            if (!occupiedSpaces.Contains(pos))
                            {
                                //...then it is a sealed space.
                                sealedSpaces.Add(pos);
                            }
                        }
                    }
                }
            }

            return sealedSpaces;
        }

        //Same function as above but passing no arguments version
        public List<Vector3> GetSealedFreeSpaces()
        {
            return GetSealedFreeSpaces(GetUnsealedFreeSpaces());
        }

        //Recalculate the unsealed free spaces and store them in each array. This is an expensive ass operation, so it only happens when a block gets placed.
        public void RecalculateSealedAndUnsealedFreeSpaces()
        {
            insideCollider.Clear();

            List<Vector3> unsealedSpaces = GetUnsealedFreeSpaces();
            List<Vector3> sealedSpaces = GetSealedFreeSpaces(unsealedSpaces);

            foreach (Vector3 pos in sealedSpaces)
            {
                //insideCollider.Add(pos);
            }

            unsealedSpaceCache.Clear();
            sealedSpaceCache.Clear();

            foreach(Vector3 pos in unsealedSpaces) { 
                unsealedSpaceCache.Add(pos);
                insideCollider.Add(pos);
            }

            foreach(Vector3 pos in sealedSpaces) { 
                sealedSpaceCache.Add(pos); 
            }

            Debug.Log("Updating free + sealed spaces. Non-sealed spaces:"+ unsealedSpaceCache.Count+"; sealed spaces:"+ sealedSpaceCache.Count);
        }

        /* Grid Clamping and Z Layer Interaction */
        //Main function for mouse and keyboard movement. Blocks are clamped in the grid, moved along the Z axis according to other logic, etc.

        //Highest Z Cache. When a block gets placed, this dictionary updates with the highest Z layer a block was placed on.
        //The cursor object is then clamped to this highest Z layer; this forces the player to place as close as they can
        //while also allowing clever movement for "overlaps" (if a block from somewhere else gets placed nearby.
        private Dictionary<Vector2,float> highestZCache = new Dictionary<Vector2,float>();

        public void UpdateZCache(GameObject placedBlock)
        {
            List<Vector3> worldspaceGridCoords = BoxUtils.GridSpacesInColliders(placedBlock.GetComponents<BoxCollider>());

            foreach (Vector3 pos in worldspaceGridCoords)
            {
                Vector2 xy=new Vector2(pos.x, pos.y);

                float currentZAtXY = 0;
                if (highestZCache.ContainsKey(xy)) { currentZAtXY = highestZCache[xy]; }

                if (pos.z > currentZAtXY)
                {
                    highestZCache[xy] = pos.z;
                    Debug.Log("Highest Z Cache for [" + pos.x + ","+pos.y + "] updated to " + pos.z);
                }

            }
        }

        public void FindClampedLocationInGrid(Vector3 pointerLoc, MovingBlock selectedBlock)
        {
            insideCollider.Clear();

            //This method does two things:
            //1. Clamps the location of the parent transform so all of the block's colliders fit inside the grid.
            //2. Finds the next unsealed grid position on the Z AXIS ONLY that isn't occupied. 

            //This method is called through MouseTracker.cs.
            Vector3 parentPos = pointerLoc;

            //
            // // // GRID CLAMPING // // // 
            //

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

            //Colliders on this block gameObject.
            BoxCollider[] blockColliders = selectedBlock.GetComponents<BoxCollider>();

            //Every grid space inside every collider on this GameObject.
            List<Vector3> gridSpacesInCollider = BoxUtils.GridSpacesInColliders(blockColliders);

            //Calculate the maximum X and Y coordinate of each vertex in any colliders attached to the parent object.
            //This results in a rectangle shape that takes into account all of the colliders;
            //later, we check its extents if they are outside grid space.
            foreach (BoxCollider coll in blockColliders)
            {
                if (coll.enabled)
                {
                    Vector3[] worldspaceVerts = BoxUtils.WorldspaceColliderVertices(coll);

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
                    }
                }
            }

            //Debug purposes for the above method
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

            //The lowest possible valid Z layer. By default, this is 0, but the cursor can sometimes be at a Z level higher than 0,
            //so we default to the player's input.

            //Shifted positions with grid clamping
            float shiftedPosX = parentPos.x + shiftX;
            float shiftedPosY = parentPos.y + shiftY;


            //The pointer loc is clamped to the furthest forward block on the Z layer of this X and Y coordinate.

            float highestCachedZLayer = 0;
            Vector2 xy = new Vector2(parentPos.x, parentPos.y);
            if (highestZCache.ContainsKey(xy)) { highestCachedZLayer = highestZCache[xy]+1; }

            //Debug.Log("HighestCachedZLayer of ("+xy+"): " + highestCachedZLayer);
            //

            //The next Z layer, starting at the pointer location.
            //The pointer loc can move forward depending on if there's a box at that point.
            //Its location is limited by the furthest placed box on that Z layer.
            float nextZLayer = pointerLoc.z;

            if (nextZLayer > highestCachedZLayer) { nextZLayer = highestCachedZLayer; }

            //Debug.Log("NextZLayer:" + nextZLayer);

            //Grid-clamped position without respect for the Z layer.
            //This forces the block to stay inside the grid, but does nothing for the Z layer.
            //The Z layer is calculated based on blocks - up ahead.
            Vector3 initialPosWithoutZ = new Vector3(
                shiftedPosX,
                shiftedPosY,
                nextZLayer 
            );


            //
            // // // Z LAYER INTERACTION // // // 
            //

            //The next "free" Z position the block can fit in.
            //This starts at pointerLoc.z, but increases if there are no free spaces for any of the box's colliders.
            //MIGHT REMOVE
            //float nextAvailableZPos = nextZLayer;

            //MIGHT REMOVE
            Debug.Log("NextAvailableZPos start:" + initialPosWithoutZ);

            //We start at the original location.
            selectedBlock.transform.position = initialPosWithoutZ;

            Vector3 originalLocBeforeZFormat = initialPosWithoutZ;

            Vector3 moveLocZFormat = initialPosWithoutZ;

            Debug.Log("STARTING SCAN AT " + originalLocBeforeZFormat);

            //Start from the z pos. We've already moved it to the cursor.
            bool foundSafePlace = false;

            while (!foundSafePlace)
            {
                //Spaces in the box colliders at this current position
                List<Vector3> gridPointsAtThisLoc = BoxUtils.GridSpacesInColliders(blockColliders);

                bool somethingInTheWay = false;

                //For every grid point in the grid points of every collider,
                foreach(Vector3 gridPoint in gridPointsAtThisLoc)
                {
                    //Add 1 to the Z value of said collider.
                    //Vector3 oneSpaceForward = new Vector3(gridPoint.x, gridPoint.y, gridPoint.z+1);

                    //We obtain a list of blocks occupying the space directly in front of the collider.
                    List<GameObject> otherBlocksHere = BoxUtils.FindBlocksAtPosition(gridPoint, selectedBlock);

                    //If there are any (disincluding our own gameobject) then the space is occupuied; there is something in the way.
                    if(otherBlocksHere.Count > 0)
                    {
                        foreach(GameObject otherBlock in otherBlocksHere)
                        {
                            if (otherBlock != selectedBlock)
                            {
                                somethingInTheWay = true;

                                Debug.Log("Found something in the way at " + gridPoint);
                            }
                        }
                    }
                }

                //If there isn't something in the way of ANY of the colliders moved forward, then we've found a safe place.
                if (!somethingInTheWay)
                {
                    foundSafePlace = true;
                }

                //If there is something in the way, add 1 to the Z position, move the block there, then repeat the check.
                else
                {                    
                    moveLocZFormat=new Vector3(moveLocZFormat.x, moveLocZFormat.y, moveLocZFormat.z+1);
                    selectedBlock.transform.position= moveLocZFormat;
                }
            }

            selectedBlock.transform.position= moveLocZFormat;



            ////Starting from the PointerLoc Z position, moving 1 space at a time, check every Worldspace grid position in each collider
            ////for any other BoxCollider in existence there.
            ////Debug.Log("PointerLoc Z is starting at " + pointerLoc.z);

            ////Every Vector3 position in every collider of the copied object.
            ////These positions get added to the value 'i' in the following loop on the Z axis.
            //List<Vector3> pointsInCopiedColliders = BoxUtils.GridSpacesInColliders(selectedBlock.GetComponents<BoxCollider>());

            //foreach (Vector3 colPoint in pointsInCopiedColliders)
            //{
            //    //insideCollider.Add(colPoint);
            //}

            ////For every Z layer above the pointerLoc...
            //for (float i = nextAvailableZPos; i < 20 /* Arbitrary value of 20 */; i++)
            //{
            //    //If every collider is safe. Default true, but becomes false if we find ANY collider belonging not to us.
            //    bool safe = true;

            //    float iteratingZPos = i;

            //    foreach (Vector3 gridPos in pointsInCopiedColliders)
            //    {
            //        Vector3 posToTest = gridPos;
            //        posToTest.z = gridPos.z + i;

            //        //insideCollider.Add(posToTest);

            //        List<GameObject> blocksAtPosition = BoxUtils.FindBlocksAtPosition(posToTest, selectedBlock);

            //        if (blocksAtPosition.Count > 0)
            //        {
            //            //Debug.Log("Found other block at " + posToTest.x + ", " + posToTest.y + ", " + posToTest.z);
            //            safe = false;
            //            break;
            //        }

            //        if (sealedSpaceCache.Contains(posToTest))
            //        {
            //            Debug.Log("Found sealed space!!");
            //            safe = false;
            //            break;
            //        }


            //    }

            //    if (safe)
            //    {
            //        Vector3 iteratedPos = new Vector3(initialPosWithoutZ.x, initialPosWithoutZ.y, iteratingZPos);
            //        nextAvailableZPos = iteratingZPos;
            //        //Debug.Log("Found safe position at " + iteratedPos);
            //        break;
            //    }

            //}

            ////Finally, we have a Vector3 that's both clamped inside the grid and has found the next free Z position.
            //Vector3 blockSnappedPos = initialPosWithoutZ;
            //blockSnappedPos.z = nextAvailableZPos;

            ////Debug.Log("Ended up with " + blockSnappedPos.x + ", " + blockSnappedPos.y + ", " + blockSnappedPos.z);

            //selectedBlock.transform.position = blockSnappedPos;




        }



    }

}
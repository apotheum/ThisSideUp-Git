using System.Collections.Generic;
using System.Threading;
using ThisSideUp.Boxes;
using ThisSideUp.Boxes.Core;
using UnityEngine;
using static ThisSideUp.Boxes.Core.BoxIDs;

public class BlockSpawner : MonoBehaviour
{

    private GameObject boxInstancePrefab;

    private static BlockSpawner instance;
    public static BlockSpawner Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject instanceObject = new GameObject("Block Spawner Object");
                instance = instanceObject.AddComponent<BlockSpawner>();
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
    private void Start()
    {
        boxInstancePrefab = Resources.Load<GameObject>("Base Block Instance");

        if (boxInstancePrefab != null)
        {
            Debug.Log("Loaded box instance prefab.");
        }
        else
        {
            Debug.Log("Box instance prefab not loaded; aborting spawning procedure!");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            BoxInstance newInstance= SpawnRandomBlock();

            MouseTracker.Instance.SelectBlock(newInstance.GetComponent<MovingBlock>());
        }

    }

    private BoxInstance SpawnRandomBlock()
    {

        BoxInstance spawnedInstance = null;

        if (boxInstancePrefab != null)
        {
            Debug.Log("Attempting to spawn block...");

            //Instantiate the BOX INSTANCE prefab under the map. This does not obey clamping or collision.
            GameObject spawnedBox = Instantiate(boxInstancePrefab);

            //spawnedBox.gameObject.SetActive(false);

            //Pick a random BoxBase struct from list of boxes in BoxIDs.
            //This BoxBase contains the DISPLAY PREFAB, which gets instantiated in BoxInstance.cs.
            //There must be at least one or this will break
            List<BoxBase> boxes = BoxIDs.Instance.boxes;
            BoxBase randomBox = boxes[Random.Range(0, boxes.Count - 1)];

            //randomBox = boxes[2]; //testing purposes

            Debug.Log("I choose the box '" + randomBox.name + "!");

            //The DISPLAY PREFAB of the chosen box. Contains appearance and colliders.
            GameObject displayPrefab = randomBox.appearance;

            //Tell the BoxInstance to instantiate the DISPLAY PREFAB.
            BoxInstance spawnedBoxInstance = spawnedBox.GetComponent<BoxInstance>();

            spawnedBoxInstance.InstantiateBox(displayPrefab);

            //Move the BoxInstance under the map for now.
            Vector3 underMap = new Vector3(0, -10, 0);
            spawnedBox.transform.position = underMap;

            spawnedInstance = spawnedBoxInstance;
        }
        else
        {
            Debug.Log("The box instance prefab is null, so nothing can spawn. Aborting spawn attempt...");
        }

        return spawnedInstance;
    }

}

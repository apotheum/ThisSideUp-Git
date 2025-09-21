using DG.Tweening;
using System;
using System.Collections.Generic;
using ThisSideUp.Boxes.Core;
using UnityEngine;
using UnityEngine.Events;

namespace ThisSideUp.Boxes.Core
{
    public class BlockInventory : MonoBehaviour
    {


        private static BlockInventory instance;
        public static BlockInventory Instance
        {
            get
            {
                if (instance == null)
                {
                    GameObject instanceObject = new GameObject("Block Inventory Object");
                    instance = instanceObject.AddComponent<BlockInventory>();
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

        //Prefab that spawns when AddItem() runs.
        private GameObject itemParentPrefab;

        //Parent object the "ItemParent" prefabs spawn to.
        [SerializeField] private Transform inventoryParent;

        //Values by which objects with a 2x or 3x scale multiply by.
        //Dirty but prevents items from being too big in the inventory.
        //We don't want to scale them all the way however because it's less visually clear.
        [SerializeField] public float twoScale;
        [SerializeField] public float threeScale;


        //Whether we are allowed to select a new item.
        public bool canSelect { get; private set; }
        public UnityEvent AllowSelectionChangeEvent = new UnityEvent();


        // FLIPPING AND SCALING //
        //The inventory doesn't need to be seen all the time.
        //We "flip up" the inventory when we want to see it. Flipping down is the opposite.
        private RectTransform rect;

        //Scale of the manager object.
        //By default, it scales to whatever it started as; this is considered the FLIPPED UP scale.
        //When selection isn't allowed, it scales to FLIPPED DOWN mode, where the scale decreases dramatically.

        [SerializeField] private Vector3 flippedDownScale;
        private Vector3 flippedUpScale;

        private Vector3 desiredScale;

        [SerializeField] private float flipSpeed;

        //DOTween
        private Vector3 flippedUpPos;
        private Vector3 flippedDownPos;
        private float flipDistance = 400;

        private void Start()
        {
            GameManager.Instance.GameResetEvent.AddListener(OnGameReset);
            GameManager.Instance.GameStartEvent.AddListener(OnGameStart);

            rect = GetComponent<RectTransform>();
            flippedUpScale = rect.localScale;

            flippedUpPos = rect.localPosition;
            Vector3 subtractedPos = flippedUpPos;

            subtractedPos.y -= flipDistance;
            flippedDownPos = subtractedPos;

            rect.localPosition = flippedDownPos;

            Debug.Log("FlipUP:" + flippedUpPos + ", FlipDOWN:" + flippedDownPos);

            canSelect = false;


            itemParentPrefab = Resources.Load<GameObject>("Item Parent");

            //RepopulateInventory();

        }

        private void OnGameStart()
        {
            RepopulateInventory();
            ChangeSelectionAllowed(true);
        }

        //Change whether the selection is allowed.
        //Also flips the inventory up or down.
        public void ChangeSelectionAllowed(bool allowed)
        {
            canSelect = allowed;

            DOTween.Kill(rect);

            if (allowed)
            {
                Sequence flipUp = DOTween.Sequence();
                flipUp.Append(rect.DOLocalMoveY(flippedUpPos.y, flipSpeed)).SetEase(Ease.OutCirc);
                flipUp.Play();
            }
            else
            {
                Sequence flipDown = DOTween.Sequence();
                flipDown.Append(rect.DOLocalMoveY(flippedDownPos.y, flipSpeed)).SetEase(Ease.OutCirc);
                flipDown.Play();
            }

            AllowSelectionChangeEvent.Invoke();
        }

        //Show the inventory.
        public void ShowInventory()
        {
            RepopulateInventory();
            ChangeSelectionAllowed(true);
        }


        //Select the item. Creates a DOTween sequence that moves the inventory down.
        //Once the sequence finished, SelectItem() on MouseTracker, unparents the BoxInstance, etc.
        private InventoryItem selectedItem;

        public void SelectItem(InventoryItem selected)
        {
            selectedItem = selected;

            //Disable selection until next block is done placing
            ChangeSelectionAllowed(false);

            //Un-parent the Box Instance from this selectedItem gameObject.
            //It no longer needs to be in the UI anymore and gets passed off to MouseTracker for placement.
            BoxInstance boxInstance = selectedItem.ownedBox;
            GameObject boxObject = boxInstance.gameObject;

            boxObject.transform.parent = null;
            boxObject.transform.position = new Vector3(0, 10, 0);
            boxObject.transform.rotation = Quaternion.identity;
            boxObject.transform.localScale = Vector3.one;

            MouseTracker.Instance.SelectBlock(boxObject.GetComponent<MovingBlock>());

            //Now that we've un-parented the Box Instance from the selectedItem,
            //remove and destroy this selectedItem from the inventory
            inventory.Remove(selected);
            Destroy(selected.gameObject);
        }


        //List of every InventoryItem in our inventory.
        //We can obtain BoxInstance components from the InventoryItem.
        public List<InventoryItem> inventory = new List<InventoryItem>();

        //How many items can we have at a time?
        public int maxItems = 6;

        //Repopulate any inventory spaces with new blocks.
        //This runs once when the game starts and when 
        [SerializeField] private float repopulationCooldown;
        private float repopulateTimer = 0.0f;
        private bool repopulating = false;

        public void RepopulateInventory()
        {
            if (itemParentPrefab == null) { Debug.Log("ItemParent prefab is null"); return; }

            if (inventory.Count < maxItems)
            {
                repopulating = true;
                repopulateTimer = 0.0f;
            }

        }


        private void LateUpdate()
        {
            if (!repopulating) { return; }

            if (repopulateTimer > 0)
            {
                repopulateTimer-= Time.deltaTime;
            }
            else
            {
                if (inventory.Count < maxItems)
                {
                    int itemsNeeded = maxItems - inventory.Count;

                    Debug.Log("We need " + itemsNeeded + " new item(s)");

                    InventoryItem newItem = NewItem();

                    newItem.transform.localRotation = Quaternion.identity;

                    Vector3 itemScale = newItem.transform.localScale;
                    itemScale.x *= rect.localScale.x;
                    itemScale.y *= rect.localScale.y;
                    itemScale.z *= rect.localScale.z;

                    newItem.transform.localScale = itemScale;
                    inventory.Add(newItem);

                    repopulateTimer = repopulationCooldown;
                }
                else
                {
                    repopulating = false;
                }
            }

        }


        //Spawn one new random box. Does not add to inventory
        private InventoryItem NewItem()
        {
            if(itemParentPrefab == null) { Debug.Log("ItemParent prefab is null"); return null; }

            //We instantiate a new ItemParent
            GameObject newParent=Instantiate(itemParentPrefab);
            InventoryItem newItem=newParent.GetComponent<InventoryItem>();

            //Then parent it to the inventory 
            newParent.transform.parent = inventoryParent;
            newParent.transform.localPosition = Vector3.zero;

            //Then a new BoxInstance
            BoxInstance newInstance = BlockSpawner.Instance.SpawnRandomBlock();

            //Then that instance gets assigned to the item
            newItem.SetInstance(newInstance);

            //Then that item gets scaled to fit in the instance
            newItem.UpdateAppearance();

            return newItem;
        }

        void OnGameReset()
        {
            foreach(InventoryItem item in inventory)
            {
                Destroy(item.gameObject);
            }

            inventory.Clear();
        }

    }

}


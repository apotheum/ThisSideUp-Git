using ThisSideUp.Boxes.Core;
using ThisSideUp.Boxes.Effects;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    //The BoxInstance this inventory item represents.
    public BoxInstance ownedBox { get; private set; }

    //Rotation parent - a child of this GameObject.
    //The Rotation parent rotates at an angle, while this object retains 0,0,0 rotation.
    //This is to ensure accurate mouse hovering.
    [SerializeField] private AnchorAnimation anchorAnims;

    [SerializeField] private Button selectButton;

    private RectTransform baseRectTransform;

    private void Awake()
    {

        baseRectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        //Subscribe to AllowSelectionChange in BlockInventory; disables button and changes appearance when canSelect changes.
        BlockInventory.Instance.AllowSelectionChangeEvent.AddListener(UpdateSelectable);

        //Update once on start
        UpdateSelectable();
    }

    void UpdateSelectable()
    {
        bool selectable = BlockInventory.Instance.canSelect;


        selectButton.enabled = selectable;

        if (selectable)
        {
        } else
        {

        }
    }

    public void SetInstance(BoxInstance instance)
    {
        if (instance == null) { Debug.Log("Couldn't set boxInstance because it's null"); return; }

        ownedBox = instance;
    }

    public void OnClick()
    {
        BlockInventory.Instance.SelectItem(this);
        Debug.Log("CLICKED");
    }

    //Scale the child gameObject by the rectangle width of its colliders.
    public void UpdateAppearance()
    {
        if (ownedBox == null) { Debug.Log("Owned box is null"); return; }
        if (anchorAnims == null) { Debug.Log("Rotation parent is null"); return; }

        //Box Colliders of the BoxInstance gameobject.
        BoxCollider[] colliders = ownedBox.GetComponents<BoxCollider>();

        //Min and max coordinates of the colliders - with respect to local space.
        //Used for calculating the volume, then the offset, of each collider to center it in the icon
        Vector3[] minAndMaxCoords=BoxUtils.WorldspaceMinMaxOfColliders(colliders);
        Vector3 minCoord = minAndMaxCoords[0]-ownedBox.transform.position;
        Vector3 maxCoord = minAndMaxCoords[1]-ownedBox.transform.position;

        Debug.Log("MinCoord:" + minCoord + "; MaxCoord:" + maxCoord);

        //Volume of the colliders.
        Vector3 volume = maxCoord - minCoord;
        
        //Center of the volume collider.
        Vector3 centerLoc = volume * 0.5f;
        centerLoc.x = centerLoc.x - 0.5f;
        centerLoc.y = centerLoc.y - 0.5f;
        centerLoc.z = centerLoc.z - 0.5f;

        //Parent the BoxInstance to this object.
        ownedBox.transform.SetParent(anchorAnims.gameObject.transform);
        ownedBox.transform.localPosition = Vector3.zero;

        //The BoxInstance is centered based on the extents of its colliders
        Vector3 centeredPos = ownedBox.transform.localPosition - centerLoc;
        ownedBox.transform.localPosition = centeredPos;

        //Scale of the largest extent of the box.
        //This gets used to subtly scale the box icon.
        float largestValue = int.MinValue;
        if (volume.x > largestValue) { largestValue = volume.x; }
        if (volume.y > largestValue) { largestValue = volume.y; }
        if (volume.z > largestValue) { largestValue = volume.z; }

        Debug.Log("Largest value: " + largestValue);
        float newScale = 1.0f;
        if (largestValue > 1) { newScale = newScale * BlockInventory.Instance.twoScale; }
        if (largestValue > 2) { newScale = newScale * BlockInventory.Instance.threeScale; }

        transform.localScale= (transform.localScale * newScale);

        Vector2 newSize = baseRectTransform.sizeDelta;
        newSize = newSize * Mathf.Max(1,2-newScale);

        Debug.Log("newScale=" + (1 - newScale) + "so Vec2="+newSize);


        baseRectTransform.sizeDelta = newSize;

    }
}

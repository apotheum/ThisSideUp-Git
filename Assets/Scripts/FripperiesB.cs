using UnityEngine;

public class FripperiesB : MonoBehaviour
{

    [SerializeField] private float speed;

    void LateUpdate()
    {
        transform.Rotate(new Vector3(0, speed * Time.deltaTime, 0));
    }
}

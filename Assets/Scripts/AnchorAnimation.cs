using DG.Tweening;
using UnityEngine;

namespace ThisSideUp.Boxes.Effects
{
    public class AnchorAnimation : MonoBehaviour
    {
        [SerializeField] private float yRotationSpeed = 35.0f;
        [SerializeField] private float xRotationSpeed = 35.0f;

        // Update is called once per frame
        void LateUpdate()
        {
            Quaternion currentRot = transform.rotation;
            float delta = Time.deltaTime;
            transform.Rotate(xRotationSpeed * delta, yRotationSpeed * delta, 0);

        }

        public void OnMouseEnter()
        {

        }

        public void OnMouseExit()
        {

        }

    }


}
using UnityEngine;

namespace ThisSideUp.Boxes.Effects
{
    public class DelayedFollow : MonoBehaviour
    {
        public Transform toFollow;

        private bool moving = false;
        private Transform me;

        private static float lerpSpeed = 35.0f;

        //Decouple this child from its parent, then set it as a follow target.
        public void StartFollowing(Transform follow)
        {
            toFollow = follow;

            //if (follow != null) { Debug.Log("Follow isn't null"); }

            me.position = toFollow.position;
            me.rotation = toFollow.rotation;

            moving = true;
        }

        public void StopFollowing()
        {
            me.position = toFollow.position;
            me.rotation = toFollow.rotation;

            moving = false;
            //enabled = false;
        }

        void Awake()
        {

            if (gameObject.transform.parent == null)
            {
                Debug.Log("NULL PARENT.");
                enabled = false;
            }

            me = gameObject.transform;
        }

        //This is some shitass code but itll work
        void LateUpdate()
        {
            if (!moving) { return; }

            Vector3 currentPos = me.position;
            Vector3 desiredPos = toFollow.position;

            Quaternion currentRotation = me.rotation;
            Quaternion desiredRotation = toFollow.rotation;

            Vector3 newPos = Vector3.Lerp(currentPos, desiredPos, Mathf.Clamp01(Time.deltaTime * lerpSpeed));
            Quaternion newRotation = Quaternion.Lerp(currentRotation, desiredRotation, Mathf.Clamp01(Time.deltaTime * lerpSpeed));

            me.position = newPos;
            me.rotation = newRotation;
        }
    }

}

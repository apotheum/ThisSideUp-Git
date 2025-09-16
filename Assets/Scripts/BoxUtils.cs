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
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThisSideUp.Boxes.Core 
{
    //This ScriptableObject is a static list of BoxData structs.
    //BoxData refers to the box GameObject, which contains its colliders and related scripts,
    //as well as its traits, conditions, etc.

    [CreateAssetMenu(fileName = "BoxIDs", menuName = "ScriptableObjects/Box Database", order = 1)]
    public class BoxIDs : ScriptableObject
    {

        private static BoxIDs instance;
        public static BoxIDs Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<BoxIDs>("BoxIDs");

                    if (instance == null)
                    {
                        Debug.LogError("Item ID database failed to initialize; is it in the resources folder?");
                    }
                    else
                    {
                        Debug.Log("Loaded " + instance.boxes.Count + " total items");
                    }
                }

                return instance;
            }
        }



        [System.Serializable]
        public struct BoxBase
        {
            public GameObject appearance;
            public string name;
        }

        public List<BoxBase> boxes;

    }

}

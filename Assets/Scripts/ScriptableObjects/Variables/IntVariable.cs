using UnityEngine;

namespace ScriptableObjects.Variables
{
        [CreateAssetMenu(fileName = "NewIntVar", menuName = "Types/IntVariable")]
        public class IntVariable : ScriptableObject
        {
                public int value;
        }
}

using UnityEngine;

namespace ScriptableObjects.Variables
{
        [CreateAssetMenu(fileName = "NewFloatVar", menuName = "Types/FloatVariable")]
        public class FloatVariable : ScriptableObject
        {
                public float value;
        }
}

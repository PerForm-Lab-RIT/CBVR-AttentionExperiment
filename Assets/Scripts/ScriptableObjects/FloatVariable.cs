using UnityEngine;

namespace ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewFloatVar", menuName = "Types/FloatVariable")]
    public class FloatVariable : ScriptableObject
    {
        public float value;
    }
}

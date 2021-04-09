using UnityEngine;

namespace ScriptableObjects.Variables
{
    [CreateAssetMenu(fileName = "NewStringVar", menuName = "Types/StringVariable")]
    public class StringVariable : ScriptableObject
    {
        public string value;
    }
}
using System;
using UnityEngine;

namespace ScriptableObjects.Variables
{
    [Serializable]
    public class StringReference
    {
        [SerializeField] private bool useConstant = true;
        [SerializeField] private string constant;
        [SerializeField] private StringVariable variable;

        public string Value => useConstant ? constant : variable.value;
    }
}

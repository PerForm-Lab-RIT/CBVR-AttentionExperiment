using System;
using UnityEngine;

namespace ScriptableObjects.Variables
{
    [Serializable]
    public class IntReference
    {
        [SerializeField] private bool useConstant = true;
        [SerializeField] private int constant;
        [SerializeField] private IntVariable variable;
    
        public int Value => useConstant ? constant : variable.value;
    }
}
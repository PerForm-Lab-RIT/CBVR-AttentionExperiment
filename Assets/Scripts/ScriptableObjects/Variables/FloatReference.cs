using System;
using UnityEngine;

namespace ScriptableObjects.Variables
{
    [Serializable]
    public class FloatReference
    {
        [SerializeField] private bool useConstant = true;
        [SerializeField] private float constant;
        [SerializeField] private FloatVariable variable;

        public float Value => useConstant ? constant : variable.value;
    }
}

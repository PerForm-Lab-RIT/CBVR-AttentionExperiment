using System;
using ScriptableObjects;
using UnityEngine;

[Serializable]
public class FloatReference
{
    [SerializeField] private bool useConstant = true;
    [SerializeField] private float constant;
    [SerializeField] private FloatVariable variable;
    
    public float Value => useConstant ? constant : variable.value;
}

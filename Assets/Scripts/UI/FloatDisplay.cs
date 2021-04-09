using System.Globalization;
using ScriptableObjects.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class FloatDisplay : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private FloatReference numRef;
        [SerializeField] private string postfix;
        
        public void UpdateDisplay()
        {
            text.text = $"{numRef.Value:N2}{postfix}";
        }
    }
}

using System.Globalization;
using ScriptableObjects.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class IntCounter : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private IntReference numRef;
        
        public void UpdateCounter()
        {
            text.text = numRef.Value.ToString(CultureInfo.InvariantCulture);
        }
    }
}

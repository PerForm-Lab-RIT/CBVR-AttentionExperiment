using ScriptableObjects.Variables;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class StringDisplay : MonoBehaviour
    {
        [SerializeField] private Text text;
        [SerializeField] private StringReference stringRef;

        public void UpdateDisplay()
        {
            text.text = stringRef.Value;
        }
    }
}

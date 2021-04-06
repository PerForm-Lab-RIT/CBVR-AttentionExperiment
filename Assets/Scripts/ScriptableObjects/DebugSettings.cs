using System;
using UnityEngine;
using UXF;

namespace ScriptableObjects
{
    [CreateAssetMenu(menuName = "Settings/Debug Settings")]
    public class DebugSettings : ScriptableObject
    {
        public bool showGaze;
        public bool showStaircase;

        public void LoadFromUxfJson()
        {
            var debugSettingsDict = Session.instance.settings.GetDict("DebugSettings");
            showGaze = Convert.ToBoolean(debugSettingsDict["ShowGaze"]);
            showStaircase = Convert.ToBoolean(debugSettingsDict["ShowStaircase"]);
        }
    }
}
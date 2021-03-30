// Copyright © 2018 – Property of Tobii AB (publ) - All Rights Reserved

using Tobii.XR.Internal;
using UnityEngine;

namespace Tobii.XR
{
    /// <summary>
    /// Provides eye tracking data to TobiiXR using Pico SDK. 
    /// Pico SDK needs to be downloaded from https://developer.pico-interactive.com/sdk and added to the project separately.
    /// Make sure to follow Pico SDK guide to enable eye tracking.
    ///  
    /// Tested with version 2.8.8. There are small breaking changes between 2.8.7 and 2.8.8 so this version
    /// of the PicoProvider will not work with Pico SDK versions lower than 2.8.8 without some changes. 
    /// </summary>
    [CompilerFlag("TOBIIXR_PICOPROVIDER"), ProviderDisplayName("PicoVR"), SupportedPlatform(XRBuildTargetGroup.Android)]
    public class PicoProvider : IEyeTrackingProvider
    {
        // ReSharper disable once FieldCanBeMadeReadOnly.Local
        private TobiiXR_EyeTrackingData _eyeTrackingDataLocal = new TobiiXR_EyeTrackingData();

        public void GetEyeTrackingDataLocal(TobiiXR_EyeTrackingData data)
        {
            EyeTrackingDataHelper.Copy(_eyeTrackingDataLocal, data);
        }

#if TOBIIXR_PICOPROVIDER
        private Pvr_UnitySDKAPI.EyeTrackingData _eyeTrackingData;

        public Matrix4x4 LocalToWorldMatrix { get { return Pvr_UnitySDKManager.SDK.transform.localToWorldMatrix * Pvr_UnitySDKSensor.Instance.HeadPose.Matrix; } }

        public bool Initialize()
        {
            var result = Pvr_UnitySDKAPI.System.UPvr_setTrackingMode((int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_POSITION | (int)Pvr_UnitySDKAPI.TrackingMode.PVR_TRACKING_MODE_EYE);
            if (!result) Debug.LogWarning("Failed to enable eye tracking");

            return result;
        }

        public void Tick()
        {
            // Sometimes this value is not correctly set when application starts
            if (!Pvr_UnitySDKEyeManager.supportEyeTracking)
            {
                Pvr_UnitySDKEyeManager.Instance.SetEyeTrackingMode();
            }

            bool result = Pvr_UnitySDKAPI.System.UPvr_getEyeTrackingData(ref _eyeTrackingData);

            _eyeTrackingDataLocal.Timestamp = UnityEngine.Time.unscaledTime;

            _eyeTrackingDataLocal.GazeRay = new TobiiXR_GazeRay
            {
                Direction = _eyeTrackingData.combinedEyeGazeVector,
                Origin = _eyeTrackingData.combinedEyeGazePoint,
                IsValid = (_eyeTrackingData.combinedEyePoseStatus & (int)Pvr_UnitySDKAPI.pvrEyePoseStatus.kGazePointValid) != 0 && (_eyeTrackingData.combinedEyePoseStatus & (int)Pvr_UnitySDKAPI.pvrEyePoseStatus.kGazeVectorValid) != 0,
            };

            var leftEyeOpennessIsValid = (_eyeTrackingData.leftEyePoseStatus & (int)Pvr_UnitySDKAPI.pvrEyePoseStatus.kEyeOpennessValid) != 0;
            var rightEyeOpennessIsValid = (_eyeTrackingData.rightEyePoseStatus & (int)Pvr_UnitySDKAPI.pvrEyePoseStatus.kEyeOpennessValid) != 0;

            _eyeTrackingDataLocal.IsLeftEyeBlinking = !leftEyeOpennessIsValid || UnityEngine.Mathf.Approximately(_eyeTrackingData.leftEyeOpenness, 0.0f);
            _eyeTrackingDataLocal.IsRightEyeBlinking = !rightEyeOpennessIsValid || UnityEngine.Mathf.Approximately(_eyeTrackingData.rightEyeOpenness, 0.0f);
        }
#else
        public Matrix4x4 LocalToWorldMatrix { get { return Matrix4x4.identity; } }
        public bool Initialize()
        {
            Debug.LogError(string.Format("Scripting define symbol \"{0}\" not set for {1}.", AssemblyUtils.GetProviderCompilerFlag(this), this.GetType().Name));
            return false;
        }
        public void Tick() { }
#endif

        public void Destroy()
        {
        }
    }
}
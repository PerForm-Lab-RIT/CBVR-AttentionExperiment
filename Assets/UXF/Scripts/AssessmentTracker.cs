using ScriptableObjects.Variables;
using UnityEngine;

namespace UXF
{
    /// <summary>
    /// Attach this component to a gameobject and assign it in the trackedObjects field in an ExperimentSession to automatically record position/rotation of the object at each frame.
    /// </summary>
    
    public class AssessmentTracker : Tracker
    {
        private CalibrationAssessment _target;
        [SerializeField] private IntVariable trialCount;

        private void OnEnable()
        {
            _target = gameObject.GetComponent<CalibrationAssessment>();
            SetupDescriptorAndHeader();
            data = new UXFDataTable(header);
        }

        /// <summary>
        /// Sets measurementDescriptor and customHeader to appropriate values
        /// </summary>
        protected override void SetupDescriptorAndHeader()
        {
            measurementDescriptor = "calibrationAssessment";

            customHeader = new[]
            {
                objectName + "TargetName",
                objectName + "IsHeadFixed",
                objectName + "TargetRadius",
                objectName + "AzimuthWidth",
                objectName + "ElevationHeight",
                objectName + "TargetDistance",
                objectName + "FixationTime",
                objectName + "TargetPositionX",
                objectName + "TargetPositionY",
                objectName + "TargetPositionZ",
                objectName + "TargetLocalPositionX",
                objectName + "TargetLocalPositionY",
                objectName + "TargetLocalPositionZ",
                objectName + "GazeErrorDegrees",
                objectName + "TrialEvaluated"
            };
        }

        /// <summary>
        /// Returns current position and rotation values
        /// </summary>
        /// <returns></returns>
        protected override UXFDataRow GetCurrentValues()
        {
            var dataRow = new UXFDataRow();
            const string format = "0.####";

            // return position, rotation (x, y, z) as an array
            var position = _target.CurrentTargetTransform.position;
            var localPosition = _target.CurrentTargetTransform.localPosition;
            var values =  new(string, object)[]
            {
                (customHeader[0], _target.CurrentTargetTransform.gameObject.name),
                (customHeader[1], _target.IsHeadFixed.ToString()),
                (customHeader[2], _target.targetRadiusInDegrees.ToString(format)),
                (customHeader[3], _target.azimuthWidth.ToString(format)),
                (customHeader[4], _target.elevationHeight.ToString(format)),
                (customHeader[5], _target.targetDistance.ToString(format)),
                (customHeader[6], _target.fixationTime.ToString(format)),
                (customHeader[7], position.x.ToString(format)),
                (customHeader[8], position.y.ToString(format)),
                (customHeader[9], position.z.ToString(format)),
                (customHeader[10], localPosition.x.ToString(format)),
                (customHeader[11], localPosition.y.ToString(format)),
                (customHeader[12], localPosition.z.ToString(format)),
                (customHeader[13], _target.gazeToTargetDist),
                (customHeader[14], trialCount.value)
            };
            
            dataRow.AddRange(values);
            return dataRow;
        }
        
        /// <summary>
        /// Records a new row of data at current time. Can run independently of a running trial.
        /// </summary>
        public override void RecordRow()
        {
            UXFDataRow newRow = GetCurrentValues();
            newRow.Add(("time", Time.time));
            data.AddCompleteRow(newRow);
        }

        public override void StartRecording()
        {
            if (data == null)
                data = new UXFDataTable(header);

            recording = true;
        }
    }
}

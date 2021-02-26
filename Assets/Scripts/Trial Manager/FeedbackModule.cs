using ScriptableObjects;
using UnityEngine;

namespace Trial_Manager
{
    public class FeedbackModule : MonoBehaviour
    {
        [SerializeField] private GameObject innerStimulus;
        [SerializeField] private StimulusSettings innerStimulusSettings;
        [SerializeField] private StimulusSettings outerStimulusSettings;
        [SerializeField] private SoundPlayer soundPlayer;
        [SerializeField] private GameObject correctCircle;
        [SerializeField] private GameObject userCircle;

        public void Start()
        {
            correctCircle.SetActive(false);
            userCircle.SetActive(false);
        }
        
        public void GiveFeedback(bool inputReceived, bool isTrialSuccessful, InputData inputData)
        {
            var innerApertureRadius = Mathf.Tan(innerStimulusSettings.apertureRadiusDegrees * Mathf.PI / 180) *
                                      innerStimulusSettings.stimDepthMeters;

            UpdateAndDisplayCorrectCircle(innerApertureRadius);
            if (inputReceived)
                UpdateAndDisplayUserCircle(innerApertureRadius, inputData);

            if (isTrialSuccessful)
                soundPlayer.PlayWinSound();
            else
                soundPlayer.PlayLoseSound();
        }

        public void HideFeedback()
        {
            correctCircle.SetActive(false);
            userCircle.SetActive(false);
        }

        private void UpdateAndDisplayCorrectCircle(float innerApertureRadius)
        {
            correctCircle.transform.localPosition = innerStimulus.transform.localPosition;
            correctCircle.transform.localScale = new Vector3(2 * innerApertureRadius, 2 * innerApertureRadius, 1.0f);
            correctCircle.transform.localRotation = Quaternion.Euler(0f, 0f, innerStimulusSettings.correctAngle);
            correctCircle.SetActive(true);
        }

        private void UpdateAndDisplayUserCircle(float innerApertureRadius, InputData userInput)
        {
            var userRotation = Mathf.Acos(Vector2.Dot(Vector3.up, userInput.chosenDirection.normalized))
                * 180f / Mathf.PI;
            if (userInput.chosenDirection.x > 0)
                userRotation = -userRotation;
            userCircle.transform.localPosition = new Vector3(userInput.selectionLocation.x,
                userInput.selectionLocation.y, outerStimulusSettings.stimDepthMeters);
            userCircle.transform.localScale = new Vector3(2 * innerApertureRadius, 2 * innerApertureRadius, 1.0f);
            userCircle.transform.localRotation = Quaternion.Euler(0f, 0f, userRotation);
            userCircle.SetActive(true);
        }
    }
}

# Contents
[Getting Started](#getting-started)\
[Choosing an Eye Tracker](#choosing-an-eye-tracker)\
[JSON Settings](#json-settings)\
[Data Output](#data-output)\
[Program Flow](#program-flow)\
[Toggling Debug Visualizations](#toggling-debug-visualizations)

# Getting Started
First, open the project in [Unity 2019.4.18f1](https://download.unity3d.com/download_unity/3310a4d4f880/UnityDownloadAssistant-2019.4.18f1.exe).
After opening the project, go into Play mode to open the UXF UI: 
![UXFUI](docs/uxfui.PNG)

From top to bottom:

*- Experiment settings profile:*
The json settings to use. New json files can be made inside the project folder's 
Assets/StreamingAssets location.

*- Local data save directory:* Where to save the experiment output. It is recommended to put the output
outside of the project folder.

*- Participant ID:* The name/identifier of the participant.

*- Session number:* A sequential number representing a particular session for a participant

*- Session Type:* The type of session to run. The differences between types is highlighted as follows:

    - Training: Enables the attention cue, uses the first set of partition slices (explained in JSON settings section)
    - Testing: Disables the attention cue, uses the second set of partition slices

*- Attention Cue Type:* The type of auditory cue to use. The differences between types is highlighted as follows:
    
    - Neutral: The Attention Cue always plays at the center of the visual field and stays there
    - Feature-based: The Attention Cue pans across the visual field, based on the true direction of the inner
        stimulus and a few JSON settings

*- Enable Directional Staircase:* Toggles the use of the directional staircase

*- Enable Locational Staircase:* Toggles the use of the locational staircase

    NOTE: At least one staircase should ALWAYS be enabled otherwise the experiment won't
    run properly. If both staircases are enabled, they will be interleaved.

*- Feedback Type:* Determines if positive auditory feedback is based on the success of the participant choosing
a correct direction for an inner stimulus or a correct location.

After accepting the data agreement and clicking the 'Begin Session' button, the session will start.

# Choosing an Eye Tracker
In the Hierarchy view, select the EyeTracker GameObject:

![hierarchy](docs/hierarchy.PNG)

Then, in the inspector, change the 'Selection' option in the EyeTracker to the desired
EyeTracker implementation. Make sure that any required 3rd party software is already running (i.e.
Pupil Capture is required before using the PupilLabs eye tracker):

![eyetracker](docs/eyetracker.PNG)

# JSON settings
JSON settings are located in the Assets/StreamingAssets folder of the project. The TEMPLATE.json
file contains all the required settings that need to be set and an example of their expected type of value.
A table explaining each setting is shown below:

| Setting                             | Expects Type | Explanation                                                                                                                                     |
|-------------------------------------|--------------|-------------------------------------------------------------------------------------------------------------------------------------------------|
| NumTrials                           | int          | The number of trials to run in a single session                                                                                                 |
| FixationTimeSeconds                 | float        | How long a participant is required to fixate before starting a trial                                                                            |
| FixationDotRadiusDegrees            | float        |                                                                                                                                                 |
| FixationErrorToleranceRadiusDegrees | float        | How 'off' a participant's gaze is allowed to be when fixating                                                                                   |
| SkyColor                            | List[3]      | The color of the background, in RGB values                                                                                                      |
| StimulusDensity                     | float        |                                                                                                                                                 |
| StimulusDotSizeArcMinutes           | float        |                                                                                                                                                 |
| MinDotLifetimeSeconds               | float        | The minimum amount of time it takes for both stimuli to shuffle their dots around                                                               |
| MaxDotLifetimeSeconds               | float        | The maximum amount of time it takes for both stimuli to shuffle their dots around                                                               |
| OuterStimulusStartMs                | float        | The amount of time after the initial fixation stage to start presenting the outer stimulus                                                      |
| OuterStimulusDurationMs             | float        |                                                                                                                                                 |
| OuterStimulusRadiusDegrees          | float        |                                                                                                                                                 |
| OuterStimulusNoisePercentage        | float        | The percentage of the outer stimulus that contains dots that move in random directions, the rest of the dots will be signal dots                |
| InnerStimulusStartMs                | float        | The amount of time after the initial fixation stage to start presenting the inner stimulus                                                      |
| InnerStimulusDurationMs             | float        |                                                                                                                                                 |
| InnerStimulusRadiusDegrees          | float        |                                                                                                                                                 |
| InnerStimulusNoisePercentage        | float        | The percentage of the inner stimulus that contains dots that move in random directions, the rest of the dots will be signal dots                |
| InnerStimulusSpawnRadius            | float        | How far from the center the inner stimulus is allowed to spawn (in degrees)                                                                     |
| InputStartMs                        | float        | The amount of time after the initial fixation stage to allow input from the user                                                                |
| InputDurationMs                     | float        |                                                                                                                                                 |
| FixationBreakCheckStartMs           | float        | The amount of time after the initial fixation stage to check for a break in fixation when using an Eye Tracker                                  |
| FixationBreakCheckDurationMs        | float        |                                                                                                                                                 |
| AttentionCueStartMs                 | float        | The amount of time after the initial fixation stage to play the auditory attention cue                                                          |
| AttentionCueDurationMs              | float        |                                                                                                                                                 |
| AttentionCueDepth                   | float        | The perceived depth of the attention cue                                                                                                        |
| AttentionCueLengthDegrees           | float        | The amount of visual degrees the path of the feature-based attention should move                                                                |
| PulseFrequency                      | float        | The pulse frequency of the attention cue                                                                                                        |
| SampleRate                          | int          | The sample rate of the attention cue                                                                                                            |
| EnableBuddyDots                     | bool         | Toggles a stimulus generation mode where each dot's velocity is paired with another and moves in the opposite direction                         |
| StimulusSpacingMeters               | float        | Defines the distance between the outer and inner stimulus.  This value should be relatively small.  Only adjust if experiencing any flickering. |
| StimulusDepthMeters                 | float        | Defines the perceived distance of the stimuli                                                                                                   |
| InterTrialDelaySeconds              | float        | The delay between presenting feedback to the participant and the next trial starting                                                            |
| TotalRegionSlices                   | int          | (Must be even!) Defines how many equally-sized 'slices' of the outer aperture are created                                                       |
| FlipRegions                         | bool         | Swaps the slices that get utilized for Training and Testing mode                                                                                |
| CoarseAdjustment                    | bool         | Enables the use of defined selectable angles in the experiment                                                                                  |
| ChoosableAngles                     | List         | The selectable angles (when CoarseAdjustment is enabled). All angles are given in degrees.                                                      |
| AngleErrorToleranceDegrees          | float        | How 'off' the selected direction of a participant's input is allowed to be for positive feedback and staircasing                                |
| PositionErrorToleranceDegrees       | float        | How 'off' the selected location of a participant's input is allowed to be for positive feedback and staircasing                                 |
| CoherenceStaircase                  | List         | The staircase values to use for the inner stimulus' coherence range. All angles are given in degrees.                                           |
| StaircaseIncreaseThreshold          | int          | How many successful trials in a row it takes in order to move up the staircase                                                                  |
| StaircaseDecreaseThreshold          | int          | How many unsuccessful trials in a row it takes in order to move down the staircase                                                              |
| FailOnTimeout                       | bool         | Toggles an option to consider a trial failed if the user doesn't provide input, otherwise the trial will restart if set to false                |

# Data Output
Data will be output to the specified folder defined in the UXF UI. Inside the output folder, the data is organized by the settings used at the top level. Within each settings folder will be another group of folders
representing the participant ID. From there, data is recorded per session number. The majority of data is stored within the trial_results.csv file. Data provided via the UXF UI (such as the feedback or session type)
is located in the participantdetails/participant_details.csv file. Finally, a copy of the json settings used can be found in the settings folder of a given session. An explanation of each of the trial_results columns
is shown below:

**NOTE: All polar coordinate magnitudes are represented in visual degrees from the center of the outer stimulus. The rotational part of a polar coordinate represents the angular distance starting
from the local up vector of the outer stimulus and is measured counter-clockwise from 0 to 360 degrees**

| Result                     | Type   | Explanation                                                                                     |
|----------------------------|--------|-------------------------------------------------------------------------------------------------|
| experiment                 | string | The name of the settings used for this experiment                                               |
| ppid                       | string | The participant id                                                                              |
| session_num                | int    |                                                                                                 |
| trial_num                  | int    |                                                                                                 |
| block_num                  | int    |                                                                                                 |
| trial_num_in_block         | int    |                                                                                                 |
| start_time                 | float  | The start time of the trial, measured starting from the start of the session                    |
| end_time                   | float  | The end time of the trial, measured starting from the start of the session                      |
| correct_angle              | float  | The overall correct angle of the inner stimulus                                                 |
| chosen_angle               | float  | The participant-chosen direction                                                                |
| correct_position_magnitude | float  | How far from the center the center of the inner stimulus is, in visual degrees                  |
| correct_position_angle     | float  |                                                                                                 |
| chosen_position_magnitude  | float  | How far from the center the center of the user input is, in visual degrees                      |
| chosen_position_angle      | float  |                                                                                                 |
| position_error             | float  | How 'off' the center of the chosen position is from the correct position (in visual degrees)    |
| coherence_range            | float  | The variance in the movement angle of the inner stimulus' signal dots (controlled by staircase) |
| position_within_threshold  | bool   | Whether or not the participant was within the defined position error tolerance                  |
| angle_within_threshold     | bool   | Whether or not the participant was within the defined angle error tolerance                     |
| staircase                  | string | The staircase being utilized during a particular trial                                          |

# Program Flow

The majority of the main program flow are the scripts under the "-----MAIN SCRIPTS-----" GameObject. The flow is ultimately driven through
the UXF event system, which you can view by selecting [UXF_Rig] in the Hierarchy then going to the Events tab in the Inspector.

![eyetracker](docs/Events.PNG)

After clicking "Begin Session" in the UI, the OnSessionBegin event will fire. When this happens, a method called
StartSession acts as the entry point of the program. The SessionManager only acts as a setup and jumping off point
to the TrialManager, where the core of the application lies. Take note that a lot of the TrialManager logic utilizes coroutines in order
to perform tasks that require timing (checking fixation, delays between states, etc).

When the Session finishes setting up, BeginTrial is called in the TrialManager. Whenever a trial ends,
the maximum trial capacity is checked. If enough trials have been performed, the session is ended.
Otherwise, a new Trial is created and then started.

# Toggling Debug Visualizations

To enable/disable debug visualizations such as a staircase counter, or gaze visualizer, simply activate or deactivate the "-----DEBUG SCRIPTS-----"
GameObject in the hierarchy.

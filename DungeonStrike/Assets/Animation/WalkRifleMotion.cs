using UnityEngine;
using com.ootii.Actors.AnimationControllers;
using com.ootii.Helpers;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DungeonStrike
{
    /// <summary>
    /// </summary>
    [MotionName("Walk - Rifle")]
    public class WalkRifleMotion : MotionControllerMotion
    {
        /// <summary>
        /// Trigger values for the motion
        /// </summary>
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 1055975000;
        public const int PHASE_START_SHORTCUT_WALK = 1055976000;
        public const int PHASE_START_SHORTCUT_RUN = 1055977000;

        /// <summary>
        /// Determines if we run by default or walk
        /// </summary>
        public bool _DefaultToRun = false;
        public bool DefaultToRun
        {
            get { return _DefaultToRun; }
            set { _DefaultToRun = value; }
        }

        /// <summary>
        /// Degrees per second to rotate the actor in order to face the input direction
        /// </summary>
        public float _RotationSpeed = 180f;
        public float RotationSpeed
        {
            get { return _RotationSpeed; }
            set { _RotationSpeed = value; }
        }

        /// <summary>
        /// Minimum angle before we use the pivot speed
        /// </summary>
        public float _MinPivotAngle = 40f;
        public float MinPivotAngle
        {
            get { return _MinPivotAngle; }
            set { _MinPivotAngle = value; }
        }

        /// <summary>
        /// Degrees per second to rotate the actor when pivoting to face a direction
        /// </summary>
        public float _PivotSpeed = 180f;
        public float PivotSpeed
        {
            get { return _PivotSpeed; }
            set { _PivotSpeed = value; }
        }

        /// <summary>
        /// Delay in seconds before we allow a stop. This is to support pivoting
        /// </summary>
        public float _StopDelay = 0.1f;
        public float StopDelay
        {
            get { return _StopDelay; }
            set { _StopDelay = value; }
        }

        /// <summary>
        /// Number of degrees we'll accelerate and decelerate by
        /// in order to reach the rotation target
        /// </summary>
        public bool _RemoveLateralMovement = true;
        public bool RemoveLateralMovement
        {
            get { return _RemoveLateralMovement; }
            set { _RemoveLateralMovement = value; }
        }

        /// <summary>
        /// Determines if we shortcut the motion and start in a run
        /// </summary>
        private bool mStartInWalk = false;
        public bool StartInWalk
        {
            get { return mStartInWalk; }
            set { mStartInWalk = value; }
        }

        /// <summary>
        /// Determines if we shortcut the motion and start in a run
        /// </summary>
        private bool mStartInRun = false;
        public bool StartInRun
        {
            get { return mStartInRun; }
            set { mStartInRun = value; }
        }

        /// <summary>
        /// Determines if the actor should be running based on input
        /// </summary>
        public bool IsRunActive
        {
            get
            {
                if (mMotionController._InputSource == null) { return _DefaultToRun; }
                return ((_DefaultToRun && !mMotionController._InputSource.IsPressed(_ActionAlias)) || (!_DefaultToRun && mMotionController._InputSource.IsPressed(_ActionAlias)));
            }
        }

        /// <summary>
        /// Track when we stop getting input
        /// </summary>
        private float mInputInactiveStartTime = 0f;

        /// <summary>
        /// Track the magnitude we have from the input
        /// </summary>
        private float mInputMagnitude = 0f;

        /// <summary>
        /// Track the angle we have from the input
        /// </summary>
        private float mInputFromAvatarAngleStart = 0f;

        /// <summary>
        /// Tracks the amount of rotation that was already used
        /// </summary>
        private float mInputFromAvatarAngleUsed = 0f;

        /// <summary>
        /// Default constructor
        /// </summary>
        public WalkRifleMotion()
            : base()
        {
            _Priority = 5;
            _ActionAlias = "Run";
            mIsStartable = true;
            //mIsGroundedExpected = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRifleMotion-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public WalkRifleMotion(MotionController rController)
            : base(rController)
        {
            _Priority = 5;
            _ActionAlias = "Run";
            mIsStartable = true;
            //mIsGroundedExpected = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkRifleMotion-SM"; }
#endif
        }

        /// <summary>
        /// Awake is called after all objects are initialized so you can safely speak to other objects. This is where
        /// reference can be associated.
        /// </summary>
        public override void Awake()
        {
            base.Awake();
        }

        /// <summary>
        /// Tests if this motion should be started. However, the motion
        /// isn't actually started.
        /// </summary>
        /// <returns></returns>
        public override bool TestActivate()
        {
            // If we're not startable, this is easy
            if (!mIsStartable)
            {
                return false;
            }

            // If we're not grounded, this is easy
            if (!mMotionController.IsGrounded)
            {
                return false;
            }

            // If we're not in the traversal state, this is easy
            if (mActorController.State.Stance == EnumControllerStance.COMBAT_MELEE)
            {
                return false;
            }

            // If we're not actually moving, we can stop too
            MotionState lState = mMotionController.State;
            if (lState.InputMagnitudeTrend.Value < 0.03f)
            {
                return false;
            }

            // We're good to move
            return true;
        }

        /// <summary>
        /// Tests if the motion should continue. If it shouldn't, the motion
        /// is typically disabled
        /// </summary>
        /// <returns></returns>
        public override bool TestUpdate()
        {
            // If we just entered this frame, stay
            if (mIsActivatedFrame)
            {
                return true;
            }

            // If we are no longer grounded, stop
            if (!mMotionController.IsGrounded)
            {
                return false;
            }

            // If we're waiting for our first state to kick in, stay active
            if (!mIsAnimatorActive)
            {
                return true;
            }

            MotionState lState = mMotionController.State;
            int lStateID = mMotionLayer._AnimatorStateID;
            int lTransitionID = mMotionLayer._AnimatorTransitionID;

            // If we're not in the normal traveral state, stop

            // If we're in the idle state with no movement, stop
            if (mAge > 0.2f && lStateID == STATE_IdlePose)
            {
                if (lState.InputMagnitudeTrend.Value == 0f)
                {
                    return false;
                }
            }

            // One last check to make sure we're in this state
            if (!IsMotionState(lStateID) && !mStartInRun && !mStartInWalk)
            {
                // This is a bit painful, but make sure we're not in a
                // transition to this sub-state machine
                if (lTransitionID != TRANS_EntryState_IdleToWalk &&
                    lTransitionID != TRANS_EntryState_IdleToRun &&
                    lTransitionID != TRANS_EntryState_IdleTurn20R &&
                    lTransitionID != TRANS_EntryState_IdleTurn90R &&
                    lTransitionID != TRANS_EntryState_IdleTurn180R &&
                    lTransitionID != TRANS_EntryState_IdleTurn20L &&
                    lTransitionID != TRANS_EntryState_IdleTurn90L &&
                    lTransitionID != TRANS_EntryState_RunFwdLoop &&
                    lTransitionID != TRANS_EntryState_WalkFwdLoop)
                {
                    return false;
                }
            }

            // Stay
            return true;
        }

        /// <summary>
        /// Raised when a motion is being interrupted by another motion
        /// </summary>
        /// <param name="rMotion">Motion doing the interruption</param>
        /// <returns>Boolean determining if it can be interrupted</returns>
        public override bool TestInterruption(MotionControllerMotion rMotion)
        {
            if (rMotion is Idle)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Called to start the specific motion. If the motion
        /// were something like 'jump', this would start the jumping process
        /// </summary>
        /// <param name="rPrevMotion">Motion that this motion is taking over from</param>
        public override bool Activate(MotionControllerMotion rPrevMotion)
        {
            if (mStartInRun)
            {
                mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START_SHORTCUT_RUN, true);
            }
            else if (mStartInWalk)
            {
                mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START_SHORTCUT_WALK, true);
            }
            else
            {
                bool lIsRunActivated = _DefaultToRun;

                if (mMotionController._InputSource != null)
                {
                    lIsRunActivated = ((_DefaultToRun && !mMotionController._InputSource.IsPressed(_ActionAlias)) || (!_DefaultToRun && mMotionController._InputSource.IsPressed(_ActionAlias)));
                }

                if (lIsRunActivated)
                {
                    MotionState lState = mMotionController.State;
                    lState.InputMagnitudeTrend.Value = 1f;

                    mMotionController.State = lState;
                }

                mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_START, true);
            }

            // Clear any internal values
            mInputInactiveStartTime = 0f;

            // Store the angle we're starting our movement at. We use this
            // as we pivot to the forward direction
            mInputFromAvatarAngleStart = mMotionController.State.InputFromAvatarAngle;
            mInputFromAvatarAngleUsed = 0f;

            // Flag this motion as active
            return base.Activate(rPrevMotion);
        }

        /// <summary>
        /// Raised when we shut the motion down
        /// </summary>
        public override void Deactivate()
        {
            // Clear out the start
            mStartInRun = false;
            mStartInWalk = false;

            base.Deactivate();
        }

        /// <summary>
        /// Allows the motion to modify the velocity before it is applied.
        ///
        /// NOTE:
        /// Be careful when removing rotations
        /// as some transitions will want rotations even if the state they are transitioning from don't.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        /// <param name="rMovement">Amount of movement caused by root motion this frame</param>
        /// <param name="rRotation">Amount of rotation caused by root motion this frame</param>
        /// <returns></returns>
        public override void UpdateRootMotion(float rDeltaTime, int rUpdateIndex, ref Vector3 rMovement, ref Quaternion rRotation)
        {
            // Don't allow root motion if we're adjusting the forward direction
            int lStateID = mMotionLayer._AnimatorStateID;
            int lTransitionID = mMotionLayer._AnimatorTransitionID;

            // If we're transitioning to the run forward, stop rotation
            if (lTransitionID == TRANS_EntryState_RunFwdLoop || lTransitionID == TRANS_EntryState_WalkFwdLoop)
            {
                rRotation = Quaternion.identity;

                if (_RemoveLateralMovement)
                {
                    rMovement.x = 0f;
                }
            }
            // No rotation whould occur when stopping
            else if (lStateID == STATE_WalkToIdle_RDown || lStateID == STATE_WalkToIdle_LDown)
            {
                rRotation = Quaternion.identity;
            }
            // No rotation should occur when adjusting to the forward position. Here, the animation
            // does have rotation, but we don't want it. Instead we'll do the rotation in code so
            // we get the exact forward.
            else if (lStateID == STATE_IdleTurn20L || lStateID == STATE_IdleTurn20R)
            {
                rRotation = Quaternion.identity;
            }

            // Remove any side-to-side sway
            if (_RemoveLateralMovement && (lStateID == STATE_WalkFwdLoop || lStateID == STATE_IdleToWalk))
            {
                rMovement.x = 0f;
            }

            // Don't allow backwards movement when moving forward. Some animations have this
            if (rMovement.z < 0f)
            {
                rMovement.z = 0f;
            }
        }

        /// <summary>
        /// Updates the motion over time. This is called by the controller
        /// every update cycle so animations and stages can be updated.
        /// </summary>
        /// <param name="rDeltaTime">Time since the last frame (or fixed update call)</param>
        /// <param name="rUpdateIndex">Index of the update to help manage dynamic/fixed updates. [0: Invalid update, >=1: Valid update]</param>
        public override void Update(float rDeltaTime, int rUpdateIndex)
        {
            // Initialize properties
            mRotation = Quaternion.identity;

            // Used to determine if we'll actually use the input values this frame
            bool lUseInput = true;

            // Determines if we need to update the state itself
            bool lUpdateAnimatorState = false;

            // Grab the state info
            MotionState lState = mMotionController.State;
            int lStateMotionPhase = lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].MotionPhase;
            float lStateMotionParameter = lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].MotionParameter;

            //AnimatorStateInfo lStateInfo = lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].StateInfo;
            int lStateID = mMotionLayer.AnimatorStateID;
            float lStateTime = mMotionLayer.AnimatorStateNormalizedTime;

            // In order to swap from walking to running, we're going to modify the state value some.
            bool lIsRunActivated = _DefaultToRun;

            if (mMotionController._InputSource != null)
            {
                lIsRunActivated = ((_DefaultToRun && !mMotionController._InputSource.IsPressed(_ActionAlias)) || (!_DefaultToRun && mMotionController._InputSource.IsPressed(_ActionAlias)));
            }

            if (lIsRunActivated)
            {
                if (lState.InputMagnitudeTrend.Value < 0.51f)
                {
                    lIsRunActivated = false;
                }
            }
            else
            {
                lUpdateAnimatorState = true;
                lState.InputY = lState.InputY * 0.5f;

                if (lState.InputMagnitudeTrend.Value > 0.49f)
                {
                    lState.InputMagnitudeTrend.Replace(0.5f);
                }
            }

            // Update the animator state parameter as the "run" flag
            int lRunActivated = (lIsRunActivated ? 1 : 0);
            //if (lStateMotionParameter != lRunActivated)
            {
                lStateMotionParameter = lRunActivated;

                lUpdateAnimatorState = true;
                lState.AnimatorStates[mMotionLayer._AnimatorLayerIndex].MotionParameter = lRunActivated;
            }


            if (lIsRunActivated && lState.InputMagnitudeTrend.Value < 0.9f)
            {
                //Utilities.Debug.Log.FileWrite("here dt:" + Time.deltaTime + " mag:" + lState.InputMagnitudeTrend.Value.ToString("f3") + " my:" + mMotionController._InputSource.MovementY.ToString("f5") + " uy:" + UnityEngine.Input.GetAxis("WXLeftStickY").ToString("f5"));
            }

            // We may not want to react to the 0 input too quickly. This way we can see
            // if the player is truley stoping or just pivoting...
            //
            // This first check is to see if we process the input immediately
            if (lState.InputMagnitudeTrend.Value >= 0.1f || _StopDelay == 0f || !(lStateID == STATE_WalkFwdLoop || lStateID == STATE_RunFwdLoop))
            {
                if (mInputInactiveStartTime == 0f)
                {
                    mInputMagnitude = lState.InputMagnitudeTrend.Value;
                }

                mInputInactiveStartTime = 0f;
            }
            // If not, we'll delay a bit before changing the magnitude to 0
            else
            {
                if (mInputInactiveStartTime == 0f)
                {
                    mInputInactiveStartTime = Time.time;
                }

                // We use this delay in order to enable the 180 pivot. Without it, the actor
                // comes to a stop and then pivots...which is awkward. However, with it,
                // when we do want to stop (and we're running left/right), the character will keep pivoting.
                if (mInputInactiveStartTime + _StopDelay > Time.time)
                {
                    lUseInput = false;

                    lUpdateAnimatorState = true;
                    lState.InputMagnitudeTrend.Replace(mInputMagnitude);
                }
            }

            // As long as we're not delaying the input, see if we need to pivot
            if (lUseInput)
            {
                // If we are processing input, we can clear the
                // last angular velocity and recalculate it (later)
                mAngularVelocity = Vector3.zero;

                // If we're starting to walk or run, allow the actor rotation
                if (lStateID == STATE_IdleToWalk || lStateID == STATE_IdleToRun)
                {
                    float lPercent = Mathf.Clamp01(lStateTime);
                    mAngularVelocity.y = GetRotationSpeed(mMotionController.State.InputFromAvatarAngle, rDeltaTime) * lPercent;
                }
                // Rotate the avatar if we're walking
                else if (lStateID == STATE_WalkFwdLoop)
                {
                    // If we're not doing a pivot, have the motion rotate the actor
                    if (Mathf.Abs(lState.InputFromAvatarAngle) < 140f)
                    {
                        mAngularVelocity.y = GetRotationSpeed(mMotionController.State.InputFromAvatarAngle, rDeltaTime);
                    }

                    // We set the motion phase here because we want the transition to occur because we want to break the
                    // walk animation into two phases: left down and right down
                    //if (lState.InputMagnitudeTrend.Value < 0.1f
                    //    && (lState.AnimatorStates[mMotionLayer.AnimatorLayerIndex].MotionPhase == 0 ||
                    //        lState.AnimatorStates[mMotionLayer.AnimatorLayerIndex].MotionPhase == PHASE_STOP_LEFT_DOWN ||
                    //        lState.AnimatorStates[mMotionLayer.AnimatorLayerIndex].MotionPhase == PHASE_STOP_RIGHT_DOWN)
                    //    )
                    //{
                    //    float lNormalizedTime = lStateTime % 1f;
                    //    if (lNormalizedTime > 0.6f)
                    //    {
                    //        // Transition exit time = 1.0
                    //        mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_STOP_LEFT_DOWN, true);
                    //    }
                    //    else
                    //    {
                    //        // Transition exit time = 0.5
                    //        mMotionController.SetAnimatorMotionPhase(mMotionLayer.AnimatorLayerIndex, PHASE_STOP_RIGHT_DOWN, true);
                    //    }
                    //}
                }
                // Rotate the avatar if we're running
                else if (lStateID == STATE_RunFwdLoop || lStateID == STATE_RunStop_RDown || lStateID == STATE_RunStop_LDown)
                {
                    if (Mathf.Abs(lState.InputFromAvatarAngle) < 140f)
                    {
                        mAngularVelocity.y = GetRotationSpeed(mMotionController.State.InputFromAvatarAngle, rDeltaTime);
                    }
                }
                // If we're coming out of a idle-pivot-to-walk, allow the actor rotation
                else if (lStateID == STATE_IdleToWalk90L || lStateID == STATE_IdleToWalk90R)
                {
                    if (lStateTime > 0.7f && lStateTime <= 1f)
                    {
                        float lPercent = Mathf.Clamp01((lStateTime - 0.7f) / 0.3f);
                        mAngularVelocity.y = GetRotationSpeed(mMotionController.State.InputFromAvatarAngle, rDeltaTime) * lPercent;
                    }
                }
            }

            // Clear out our start flags
            if (lStateID == STATE_WalkFwdLoop || lStateID == STATE_RunFwdLoop)
            {
                mStartInRun = false;
                mStartInWalk = false;
            }

            // If we're in the idle pose and just turning a little to face in the input direction
            // cleanly, determine the rotation speed and use it to turn the actor.
            // Animations run from 0 to 0.4f
            // [duration] = Exit Time
            if (lStateID == STATE_IdleTurn20L || lStateID == STATE_IdleTurn20R)
            {
                float lPercent = Mathf.Clamp01(lStateTime / 0.4f);
                float lTotalRotation = mInputFromAvatarAngleStart * lPercent;
                float lFrameRotation = lTotalRotation - mInputFromAvatarAngleUsed;

                mRotation = Quaternion.Euler(0f, lFrameRotation, 0f);

                mInputFromAvatarAngleUsed = lTotalRotation;
            }

            // If we need to update the animator state, do it once
            if (lUpdateAnimatorState)
            {
                mMotionController.State = lState;
            }
        }

        /// <summary>
        /// Retrieve the rotation speed we'll use to get the actor to face towards
        /// the direciton of the input
        /// </summary>
        /// <param name="rAngle"></param>
        /// <param name="rDeltaTime"></param>
        /// <returns></returns>
        private float GetRotationSpeed(float rAngle, float rDeltaTime)
        {
            int lPivotState = 0;
            float lAbsAngle = Mathf.Abs(rAngle);

            // Determine if we'll use the pivot speed
            if (_RotationSpeed == 0f && lAbsAngle > 10f)
            {
                lPivotState = 1;
            }
            else if (_MinPivotAngle != 0f && lAbsAngle >= _MinPivotAngle)
            {
                lPivotState = 1;
            }

            // Grab our final rotation speed, but make sure it doesn't exceed the target angle
            float lRotationSpeed = Mathf.Sign(rAngle) * (lPivotState == 0 ? _RotationSpeed : _PivotSpeed);
            if (lRotationSpeed == 0f || Mathf.Abs(lRotationSpeed * rDeltaTime) > lAbsAngle)
            {
                lRotationSpeed = rAngle / rDeltaTime;
            }

            // Return the result
            return lRotationSpeed;
        }

        // **************************************************************************************************
        // Following properties and function only valid while editing
        // **************************************************************************************************

#if UNITY_EDITOR

        /// <summary>
        /// Creates input settings in the Unity Input Manager
        /// </summary>
        public override void CreateInputManagerSettings()
        {
            if (!InputManagerHelper.IsDefined(_ActionAlias))
            {
                InputManagerEntry lEntry = new InputManagerEntry();
                lEntry.Name = _ActionAlias;
                lEntry.PositiveButton = "left shift";
                lEntry.Gravity = 1000;
                lEntry.Dead = 0.001f;
                lEntry.Sensitivity = 1000;
                lEntry.Type = InputManagerEntryType.KEY_MOUSE_BUTTON;
                lEntry.Axis = 0;
                lEntry.JoyNum = 0;

                InputManagerHelper.AddEntry(lEntry, true);

#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

                lEntry = new InputManagerEntry();
                lEntry.Name = _ActionAlias;
                lEntry.PositiveButton = "";
                lEntry.Gravity = 1;
                lEntry.Dead = 0.3f;
                lEntry.Sensitivity = 1;
                lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
                lEntry.Axis = 5;
                lEntry.JoyNum = 0;

                InputManagerHelper.AddEntry(lEntry, true);

#else

                lEntry = new InputManagerEntry();
                lEntry.Name = _ActionAlias;
                lEntry.PositiveButton = "";
                lEntry.Gravity = 1;
                lEntry.Dead = 0.3f;
                lEntry.Sensitivity = 1;
                lEntry.Type = InputManagerEntryType.JOYSTICK_AXIS;
                lEntry.Axis = 9;
                lEntry.JoyNum = 0;

                InputManagerHelper.AddEntry(lEntry, true);

#endif
            }
        }

        /// <summary>
        /// Allow the motion to render it's own GUI
        /// </summary>
        public override bool OnInspectorGUI()
        {
            bool lIsDirty = false;

            bool lNewDefaultToRun = EditorGUILayout.Toggle(new GUIContent("Default to Run", "Determines if the default is to run or walk."), _DefaultToRun);
            if (lNewDefaultToRun != _DefaultToRun)
            {
                lIsDirty = true;
                DefaultToRun = lNewDefaultToRun;
            }

            string lNewActionAlias = EditorGUILayout.TextField(new GUIContent("Run Action Alias", "Action alias that triggers a run or walk (which ever is opposite the default)."), ActionAlias, GUILayout.MinWidth(30));
            if (lNewActionAlias != ActionAlias)
            {
                lIsDirty = true;
                ActionAlias = lNewActionAlias;
            }

            GUILayout.Space(5f);

            float lNewRotationSpeed = EditorGUILayout.FloatField(new GUIContent("Rotation Speed", "Degrees per second to rotate towards the camera forward (when not pivoting). A value of '0' means rotate instantly."), RotationSpeed);
            if (lNewRotationSpeed != RotationSpeed)
            {
                lIsDirty = true;
                RotationSpeed = lNewRotationSpeed;
            }

            float lNewMinPivotAngle = EditorGUILayout.FloatField(new GUIContent("Min Pivot Angle", "Degrees where we use the pivot speed for rotating."), MinPivotAngle);
            if (lNewMinPivotAngle != MinPivotAngle)
            {
                lIsDirty = true;
                MinPivotAngle = lNewMinPivotAngle;
            }

            float lNewPivotSpeed = EditorGUILayout.FloatField(new GUIContent("Pivot Speed", "Degrees per second to rotate when pivoting exceeds the min pivot angle."), PivotSpeed);
            if (lNewPivotSpeed != PivotSpeed)
            {
                lIsDirty = true;
                PivotSpeed = lNewPivotSpeed;
            }

            float lNewStopDelay = EditorGUILayout.FloatField(new GUIContent("Stop Delay", "Delay (in seconds) before we process a stop. This gives us time to test for a pivot."), StopDelay);
            if (lNewStopDelay != StopDelay)
            {
                lIsDirty = true;
                StopDelay = lNewStopDelay;
            }

            GUILayout.Space(5f);

            bool lNewRemoveLateralMovement = EditorGUILayout.Toggle(new GUIContent("Remove Lateral Movement", "Determines if we'll remove sideways movement to reduce swaying."), RemoveLateralMovement);
            if (lNewRemoveLateralMovement != RemoveLateralMovement)
            {
                lIsDirty = true;
                RemoveLateralMovement = lNewRemoveLateralMovement;
            }

            return lIsDirty;
        }

#endif
        #region Auto-Generated
        // ************************************ START AUTO GENERATED ************************************

        /// <summary>
        /// These declarations go inside the class so you can test for which state
        /// and transitions are active. Testing hash values is much faster than strings.
        /// </summary>
        public static int STATE_IdleToWalk = -1;
        public static int STATE_IdleToRun = -1;
        public static int STATE_IdleTurn90L = -1;
        public static int STATE_IdleTurn180L = -1;
        public static int STATE_IdleToWalk90L = -1;
        public static int STATE_IdleToWalk180L = -1;
        public static int STATE_IdleToRun90L = -1;
        public static int STATE_IdleToRun180L = -1;
        public static int STATE_IdleTurn90R = -1;
        public static int STATE_IdleTurn180R = -1;
        public static int STATE_IdleToWalk90R = -1;
        public static int STATE_IdleToWalk180R = -1;
        public static int STATE_IdleToRun90R = -1;
        public static int STATE_IdleToRun180R = -1;
        public static int STATE_IdlePose = -1;
        public static int STATE_WalkFwdLoop = -1;
        public static int STATE_RunFwdLoop = -1;
        public static int STATE_RunPivot180L_RDown = -1;
        public static int STATE_RunPivot180R_LDown = -1;
        public static int STATE_WalkToIdle_RDown = -1;
        public static int STATE_WalkToIdle_LDown = -1;
        public static int STATE_RunStop_RDown = -1;
        public static int STATE_RunStop_LDown = -1;
        public static int STATE_RunPivot180L_LDown = -1;
        public static int STATE_RunPivot180R_RDown = -1;
        public static int STATE_IdleTurn20R = -1;
        public static int STATE_IdleTurn20L = -1;
        public static int STATE_WalkPivot180_L = -1;
        public static int TRANS_AnyState_IdleTurn90L = -1;
        public static int TRANS_EntryState_IdleTurn90L = -1;
        public static int TRANS_AnyState_IdleTurn20L = -1;
        public static int TRANS_EntryState_IdleTurn20L = -1;
        public static int TRANS_AnyState_IdleTurn20R = -1;
        public static int TRANS_EntryState_IdleTurn20R = -1;
        public static int TRANS_AnyState_IdleTurn90R = -1;
        public static int TRANS_EntryState_IdleTurn90R = -1;
        public static int TRANS_AnyState_IdleTurn180R = -1;
        public static int TRANS_EntryState_IdleTurn180R = -1;
        public static int TRANS_AnyState_IdleToRun = -1;
        public static int TRANS_EntryState_IdleToRun = -1;
        public static int TRANS_AnyState_WalkFwdLoop = -1;
        public static int TRANS_EntryState_WalkFwdLoop = -1;
        public static int TRANS_AnyState_RunFwdLoop = -1;
        public static int TRANS_EntryState_RunFwdLoop = -1;
        public static int TRANS_AnyState_IdleToWalk = -1;
        public static int TRANS_EntryState_IdleToWalk = -1;
        public static int TRANS_IdleToWalk_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk_WalkToIdle_LDown = -1;
        public static int TRANS_IdleToWalk_WalkToIdle_RDown = -1;
        public static int TRANS_IdleToRun_RunFwdLoop = -1;
        public static int TRANS_IdleToRun_RunStop_LDown = -1;
        public static int TRANS_IdleToRun_RunStop_RDown = -1;
        public static int TRANS_IdleTurn90L_IdlePose = -1;
        public static int TRANS_IdleTurn90L_IdleToWalk = -1;
        public static int TRANS_IdleTurn90L_IdleToWalk90L = -1;
        public static int TRANS_IdleTurn90L_IdleToRun90L = -1;
        public static int TRANS_IdleTurn90L_IdleToRun = -1;
        public static int TRANS_IdleTurn180L_IdlePose = -1;
        public static int TRANS_IdleTurn180L_IdleToWalk = -1;
        public static int TRANS_IdleTurn180L_IdleToWalk180L = -1;
        public static int TRANS_IdleTurn180L_IdleToRun180L = -1;
        public static int TRANS_IdleTurn180L_IdleToRun = -1;
        public static int TRANS_IdleToWalk90L_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk90L_IdlePose = -1;
        public static int TRANS_IdleToWalk180L_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk180L_IdlePose = -1;
        public static int TRANS_IdleToRun90L_RunFwdLoop = -1;
        public static int TRANS_IdleToRun90L_RunStop_LDown = -1;
        public static int TRANS_IdleToRun180L_RunFwdLoop = -1;
        public static int TRANS_IdleToRun180L_RunStop_LDown = -1;
        public static int TRANS_IdleTurn90R_IdlePose = -1;
        public static int TRANS_IdleTurn90R_IdleToWalk = -1;
        public static int TRANS_IdleTurn90R_IdleToWalk90R = -1;
        public static int TRANS_IdleTurn90R_IdleToRun90R = -1;
        public static int TRANS_IdleTurn90R_IdleToRun = -1;
        public static int TRANS_IdleTurn180R_IdlePose = -1;
        public static int TRANS_IdleTurn180R_IdleToWalk = -1;
        public static int TRANS_IdleTurn180R_IdleToWalk180R = -1;
        public static int TRANS_IdleTurn180R_IdleToRun180R = -1;
        public static int TRANS_IdleTurn180R_IdleToRun = -1;
        public static int TRANS_IdleToWalk90R_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk90R_IdlePose = -1;
        public static int TRANS_IdleToWalk180R_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk180R_IdlePose = -1;
        public static int TRANS_IdleToRun90R_RunStop_LDown = -1;
        public static int TRANS_IdleToRun90R_RunFwdLoop = -1;
        public static int TRANS_IdleToRun180R_RunFwdLoop = -1;
        public static int TRANS_IdleToRun180R_RunStop_LDown = -1;
        public static int TRANS_IdlePose_IdleToWalk180R = -1;
        public static int TRANS_IdlePose_IdleToWalk90R = -1;
        public static int TRANS_IdlePose_IdleToWalk180L = -1;
        public static int TRANS_IdlePose_IdleToWalk90L = -1;
        public static int TRANS_IdlePose_IdleToWalk = -1;
        public static int TRANS_IdlePose_IdleToRun = -1;
        public static int TRANS_IdlePose_IdleToRun90L = -1;
        public static int TRANS_IdlePose_IdleToRun180L = -1;
        public static int TRANS_IdlePose_IdleToRun90R = -1;
        public static int TRANS_IdlePose_IdleToRun180R = -1;
        public static int TRANS_WalkFwdLoop_RunFwdLoop = -1;
        public static int TRANS_WalkFwdLoop_WalkToIdle_RDown = -1;
        public static int TRANS_WalkFwdLoop_WalkToIdle_LDown = -1;
        public static int TRANS_WalkFwdLoop_WalkPivot180_L = -1;
        public static int TRANS_RunFwdLoop_WalkFwdLoop = -1;
        public static int TRANS_RunFwdLoop_RunStop_LDown = -1;
        public static int TRANS_RunFwdLoop_RunPivot180L_RDown = -1;
        public static int TRANS_RunFwdLoop_RunPivot180R_LDown = -1;
        public static int TRANS_RunFwdLoop_RunPivot180L_LDown = -1;
        public static int TRANS_RunFwdLoop_RunPivot180R_RDown = -1;
        public static int TRANS_RunFwdLoop_RunStop_RDown = -1;
        public static int TRANS_RunPivot180L_RDown_RunFwdLoop = -1;
        public static int TRANS_RunPivot180R_LDown_RunFwdLoop = -1;
        public static int TRANS_WalkToIdle_RDown_IdlePose = -1;
        public static int TRANS_WalkToIdle_RDown_WalkFwdLoop = -1;
        public static int TRANS_WalkToIdle_RDown_IdleToWalk = -1;
        public static int TRANS_WalkToIdle_RDown_WalkPivot180_L = -1;
        public static int TRANS_WalkToIdle_RDown_IdleToWalk180R = -1;
        public static int TRANS_WalkToIdle_LDown_IdlePose = -1;
        public static int TRANS_WalkToIdle_LDown_WalkFwdLoop = -1;
        public static int TRANS_WalkToIdle_LDown_WalkPivot180_L = -1;
        public static int TRANS_WalkToIdle_LDown_IdleToWalk180R = -1;
        public static int TRANS_WalkToIdle_LDown_IdleToWalk = -1;
        public static int TRANS_RunStop_RDown_IdlePose = -1;
        public static int TRANS_RunStop_RDown_RunFwdLoop = -1;
        public static int TRANS_RunStop_RDown_RunPivot180R_LDown = -1;
        public static int TRANS_RunStop_LDown_IdlePose = -1;
        public static int TRANS_RunStop_LDown_RunFwdLoop = -1;
        public static int TRANS_RunStop_LDown_RunPivot180R_RDown = -1;
        public static int TRANS_RunPivot180L_LDown_RunFwdLoop = -1;
        public static int TRANS_RunPivot180R_RDown_RunFwdLoop = -1;
        public static int TRANS_IdleTurn20R_IdlePose = -1;
        public static int TRANS_IdleTurn20R_IdleToWalk = -1;
        public static int TRANS_IdleTurn20R_IdleToRun = -1;
        public static int TRANS_IdleTurn20L_IdlePose = -1;
        public static int TRANS_IdleTurn20L_IdleToWalk = -1;
        public static int TRANS_IdleTurn20L_IdleToRun = -1;
        public static int TRANS_WalkPivot180_L_WalkFwdLoop = -1;

        /// <summary>
        /// Determines if we're using auto-generated code
        /// </summary>
        public override bool HasAutoGeneratedCode
        {
            get { return true; }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsInMotionState
        {
            get
            {
                int lStateID = mMotionLayer._AnimatorStateID;
                int lTransitionID = mMotionLayer._AnimatorTransitionID;

                if (lStateID == STATE_IdleToWalk) { return true; }
                if (lStateID == STATE_IdleToRun) { return true; }
                if (lStateID == STATE_IdleTurn90L) { return true; }
                if (lStateID == STATE_IdleTurn180L) { return true; }
                if (lStateID == STATE_IdleToWalk90L) { return true; }
                if (lStateID == STATE_IdleToWalk180L) { return true; }
                if (lStateID == STATE_IdleToRun90L) { return true; }
                if (lStateID == STATE_IdleToRun180L) { return true; }
                if (lStateID == STATE_IdleTurn90R) { return true; }
                if (lStateID == STATE_IdleTurn180R) { return true; }
                if (lStateID == STATE_IdleToWalk90R) { return true; }
                if (lStateID == STATE_IdleToWalk180R) { return true; }
                if (lStateID == STATE_IdleToRun90R) { return true; }
                if (lStateID == STATE_IdleToRun180R) { return true; }
                if (lStateID == STATE_IdlePose) { return true; }
                if (lStateID == STATE_WalkFwdLoop) { return true; }
                if (lStateID == STATE_RunFwdLoop) { return true; }
                if (lStateID == STATE_RunPivot180L_RDown) { return true; }
                if (lStateID == STATE_RunPivot180R_LDown) { return true; }
                if (lStateID == STATE_WalkToIdle_RDown) { return true; }
                if (lStateID == STATE_WalkToIdle_LDown) { return true; }
                if (lStateID == STATE_RunStop_RDown) { return true; }
                if (lStateID == STATE_RunStop_LDown) { return true; }
                if (lStateID == STATE_RunPivot180L_LDown) { return true; }
                if (lStateID == STATE_RunPivot180R_RDown) { return true; }
                if (lStateID == STATE_IdleTurn20R) { return true; }
                if (lStateID == STATE_IdleTurn20L) { return true; }
                if (lStateID == STATE_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn90L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn90L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn20L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn20L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn20R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn20R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn90R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn90R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn180R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn180R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToRun) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToRun) { return true; }
                if (lTransitionID == TRANS_AnyState_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_EntryState_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_AnyState_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_EntryState_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn180R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn180R) { return true; }
                if (lTransitionID == TRANS_IdleToWalk_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToWalk_WalkToIdle_LDown) { return true; }
                if (lTransitionID == TRANS_IdleToWalk_WalkToIdle_RDown) { return true; }
                if (lTransitionID == TRANS_IdleToRun_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToRun_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_IdleToRun_RunStop_RDown) { return true; }
                if (lTransitionID == TRANS_IdleTurn90L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn90L_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_IdleTurn90L_IdleToWalk90L) { return true; }
                if (lTransitionID == TRANS_IdleTurn90L_IdleToRun90L) { return true; }
                if (lTransitionID == TRANS_IdleTurn90L_IdleToRun) { return true; }
                if (lTransitionID == TRANS_IdleTurn180L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn180L_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_IdleTurn180L_IdleToWalk180L) { return true; }
                if (lTransitionID == TRANS_IdleTurn180L_IdleToRun180L) { return true; }
                if (lTransitionID == TRANS_IdleTurn180L_IdleToRun) { return true; }
                if (lTransitionID == TRANS_IdleToWalk90L_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToWalk90L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToWalk180L_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToWalk180L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToRun90L_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToRun90L_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_IdleToRun180L_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToRun180L_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_IdleTurn90R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn90R_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_IdleTurn90R_IdleToWalk90R) { return true; }
                if (lTransitionID == TRANS_IdleTurn90R_IdleToRun90R) { return true; }
                if (lTransitionID == TRANS_IdleTurn90R_IdleToRun) { return true; }
                if (lTransitionID == TRANS_IdleTurn180R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn180R_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_IdleTurn180R_IdleToWalk180R) { return true; }
                if (lTransitionID == TRANS_IdleTurn180R_IdleToRun180R) { return true; }
                if (lTransitionID == TRANS_IdleTurn180R_IdleToRun) { return true; }
                if (lTransitionID == TRANS_IdleToWalk90R_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToWalk90R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToWalk180R_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToWalk180R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleToRun90R_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_IdleToRun90R_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToRun180R_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleToRun180R_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToWalk180R) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToWalk90R) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToWalk180L) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToWalk90L) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToRun) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToRun90L) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToRun180L) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToRun90R) { return true; }
                if (lTransitionID == TRANS_IdlePose_IdleToRun180R) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkToIdle_RDown) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkToIdle_LDown) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkToIdle_LDown) { return true; }
                if (lTransitionID == TRANS_WalkFwdLoop_WalkToIdle_RDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunPivot180L_RDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunPivot180R_LDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunPivot180L_LDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunPivot180R_RDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunStop_RDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunStop_RDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_RunPivot180L_RDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunPivot180R_LDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_IdleToWalk180R) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_IdleToWalk180R) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_IdleToWalk180R) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_IdleToWalk180R) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_RunStop_RDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_RunStop_RDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunStop_RDown_RunPivot180R_LDown) { return true; }
                if (lTransitionID == TRANS_RunStop_RDown_RunPivot180R_LDown) { return true; }
                if (lTransitionID == TRANS_RunStop_LDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_RunStop_LDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunStop_LDown_RunPivot180R_RDown) { return true; }
                if (lTransitionID == TRANS_RunStop_LDown_RunPivot180R_RDown) { return true; }
                if (lTransitionID == TRANS_RunPivot180L_LDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunPivot180R_RDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleTurn20R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn20R_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_IdleTurn20R_IdleToRun) { return true; }
                if (lTransitionID == TRANS_IdleTurn20R_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_IdleTurn20R_IdleToRun) { return true; }
                if (lTransitionID == TRANS_IdleTurn20L_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn20L_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_IdleTurn20L_IdleToRun) { return true; }
                if (lTransitionID == TRANS_IdleTurn20L_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_IdleTurn20L_IdleToRun) { return true; }
                if (lTransitionID == TRANS_WalkPivot180_L_WalkFwdLoop) { return true; }
                return false;
            }
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID)
        {
            if (rStateID == STATE_IdleToWalk) { return true; }
            if (rStateID == STATE_IdleToRun) { return true; }
            if (rStateID == STATE_IdleTurn90L) { return true; }
            if (rStateID == STATE_IdleTurn180L) { return true; }
            if (rStateID == STATE_IdleToWalk90L) { return true; }
            if (rStateID == STATE_IdleToWalk180L) { return true; }
            if (rStateID == STATE_IdleToRun90L) { return true; }
            if (rStateID == STATE_IdleToRun180L) { return true; }
            if (rStateID == STATE_IdleTurn90R) { return true; }
            if (rStateID == STATE_IdleTurn180R) { return true; }
            if (rStateID == STATE_IdleToWalk90R) { return true; }
            if (rStateID == STATE_IdleToWalk180R) { return true; }
            if (rStateID == STATE_IdleToRun90R) { return true; }
            if (rStateID == STATE_IdleToRun180R) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_WalkFwdLoop) { return true; }
            if (rStateID == STATE_RunFwdLoop) { return true; }
            if (rStateID == STATE_RunPivot180L_RDown) { return true; }
            if (rStateID == STATE_RunPivot180R_LDown) { return true; }
            if (rStateID == STATE_WalkToIdle_RDown) { return true; }
            if (rStateID == STATE_WalkToIdle_LDown) { return true; }
            if (rStateID == STATE_RunStop_RDown) { return true; }
            if (rStateID == STATE_RunStop_LDown) { return true; }
            if (rStateID == STATE_RunPivot180L_LDown) { return true; }
            if (rStateID == STATE_RunPivot180R_RDown) { return true; }
            if (rStateID == STATE_IdleTurn20R) { return true; }
            if (rStateID == STATE_IdleTurn20L) { return true; }
            if (rStateID == STATE_WalkPivot180_L) { return true; }
            return false;
        }

        /// <summary>
        /// Used to determine if the actor is in one of the states for this motion
        /// </summary>
        /// <returns></returns>
        public override bool IsMotionState(int rStateID, int rTransitionID)
        {
            if (rStateID == STATE_IdleToWalk) { return true; }
            if (rStateID == STATE_IdleToRun) { return true; }
            if (rStateID == STATE_IdleTurn90L) { return true; }
            if (rStateID == STATE_IdleTurn180L) { return true; }
            if (rStateID == STATE_IdleToWalk90L) { return true; }
            if (rStateID == STATE_IdleToWalk180L) { return true; }
            if (rStateID == STATE_IdleToRun90L) { return true; }
            if (rStateID == STATE_IdleToRun180L) { return true; }
            if (rStateID == STATE_IdleTurn90R) { return true; }
            if (rStateID == STATE_IdleTurn180R) { return true; }
            if (rStateID == STATE_IdleToWalk90R) { return true; }
            if (rStateID == STATE_IdleToWalk180R) { return true; }
            if (rStateID == STATE_IdleToRun90R) { return true; }
            if (rStateID == STATE_IdleToRun180R) { return true; }
            if (rStateID == STATE_IdlePose) { return true; }
            if (rStateID == STATE_WalkFwdLoop) { return true; }
            if (rStateID == STATE_RunFwdLoop) { return true; }
            if (rStateID == STATE_RunPivot180L_RDown) { return true; }
            if (rStateID == STATE_RunPivot180R_LDown) { return true; }
            if (rStateID == STATE_WalkToIdle_RDown) { return true; }
            if (rStateID == STATE_WalkToIdle_LDown) { return true; }
            if (rStateID == STATE_RunStop_RDown) { return true; }
            if (rStateID == STATE_RunStop_LDown) { return true; }
            if (rStateID == STATE_RunPivot180L_LDown) { return true; }
            if (rStateID == STATE_RunPivot180R_RDown) { return true; }
            if (rStateID == STATE_IdleTurn20R) { return true; }
            if (rStateID == STATE_IdleTurn20L) { return true; }
            if (rStateID == STATE_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn90L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn90L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn20L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn20L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn20R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn20R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn90R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn90R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn180R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn180R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToRun) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToRun) { return true; }
            if (rTransitionID == TRANS_AnyState_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_EntryState_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_AnyState_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_EntryState_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn180R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn180R) { return true; }
            if (rTransitionID == TRANS_IdleToWalk_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToWalk_WalkToIdle_LDown) { return true; }
            if (rTransitionID == TRANS_IdleToWalk_WalkToIdle_RDown) { return true; }
            if (rTransitionID == TRANS_IdleToRun_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToRun_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_IdleToRun_RunStop_RDown) { return true; }
            if (rTransitionID == TRANS_IdleTurn90L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn90L_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_IdleTurn90L_IdleToWalk90L) { return true; }
            if (rTransitionID == TRANS_IdleTurn90L_IdleToRun90L) { return true; }
            if (rTransitionID == TRANS_IdleTurn90L_IdleToRun) { return true; }
            if (rTransitionID == TRANS_IdleTurn180L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn180L_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_IdleTurn180L_IdleToWalk180L) { return true; }
            if (rTransitionID == TRANS_IdleTurn180L_IdleToRun180L) { return true; }
            if (rTransitionID == TRANS_IdleTurn180L_IdleToRun) { return true; }
            if (rTransitionID == TRANS_IdleToWalk90L_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToWalk90L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToWalk180L_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToWalk180L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToRun90L_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToRun90L_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_IdleToRun180L_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToRun180L_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_IdleTurn90R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn90R_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_IdleTurn90R_IdleToWalk90R) { return true; }
            if (rTransitionID == TRANS_IdleTurn90R_IdleToRun90R) { return true; }
            if (rTransitionID == TRANS_IdleTurn90R_IdleToRun) { return true; }
            if (rTransitionID == TRANS_IdleTurn180R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn180R_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_IdleTurn180R_IdleToWalk180R) { return true; }
            if (rTransitionID == TRANS_IdleTurn180R_IdleToRun180R) { return true; }
            if (rTransitionID == TRANS_IdleTurn180R_IdleToRun) { return true; }
            if (rTransitionID == TRANS_IdleToWalk90R_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToWalk90R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToWalk180R_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToWalk180R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleToRun90R_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_IdleToRun90R_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToRun180R_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleToRun180R_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToWalk180R) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToWalk90R) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToWalk180L) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToWalk90L) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToRun) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToRun90L) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToRun180L) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToRun90R) { return true; }
            if (rTransitionID == TRANS_IdlePose_IdleToRun180R) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkToIdle_RDown) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkToIdle_LDown) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkToIdle_LDown) { return true; }
            if (rTransitionID == TRANS_WalkFwdLoop_WalkToIdle_RDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunPivot180L_RDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunPivot180R_LDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunPivot180L_LDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunPivot180R_RDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunStop_RDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunStop_RDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_RunPivot180L_RDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunPivot180R_LDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_IdleToWalk180R) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_IdleToWalk180R) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_IdleToWalk180R) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_IdleToWalk180R) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_RunStop_RDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_RunStop_RDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunStop_RDown_RunPivot180R_LDown) { return true; }
            if (rTransitionID == TRANS_RunStop_RDown_RunPivot180R_LDown) { return true; }
            if (rTransitionID == TRANS_RunStop_LDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_RunStop_LDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunStop_LDown_RunPivot180R_RDown) { return true; }
            if (rTransitionID == TRANS_RunStop_LDown_RunPivot180R_RDown) { return true; }
            if (rTransitionID == TRANS_RunPivot180L_LDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunPivot180R_RDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleTurn20R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn20R_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_IdleTurn20R_IdleToRun) { return true; }
            if (rTransitionID == TRANS_IdleTurn20R_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_IdleTurn20R_IdleToRun) { return true; }
            if (rTransitionID == TRANS_IdleTurn20L_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn20L_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_IdleTurn20L_IdleToRun) { return true; }
            if (rTransitionID == TRANS_IdleTurn20L_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_IdleTurn20L_IdleToRun) { return true; }
            if (rTransitionID == TRANS_WalkPivot180_L_WalkFwdLoop) { return true; }
            return false;
        }

        /// <summary>
        /// Preprocess any animator data so the motion can use it later
        /// </summary>
        public override void LoadAnimatorData()
        {
            TRANS_AnyState_IdleTurn90L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRifleMotion-SM.IdleTurn90L");
            TRANS_EntryState_IdleTurn90L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRifleMotion-SM.IdleTurn90L");
            TRANS_AnyState_IdleTurn20L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRifleMotion-SM.IdleTurn20L");
            TRANS_EntryState_IdleTurn20L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRifleMotion-SM.IdleTurn20L");
            TRANS_AnyState_IdleTurn20R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRifleMotion-SM.IdleTurn20R");
            TRANS_EntryState_IdleTurn20R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRifleMotion-SM.IdleTurn20R");
            TRANS_AnyState_IdleTurn90R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRifleMotion-SM.IdleTurn90R");
            TRANS_EntryState_IdleTurn90R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRifleMotion-SM.IdleTurn90R");
            TRANS_AnyState_IdleTurn180R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRifleMotion-SM.IdleTurn180R");
            TRANS_EntryState_IdleTurn180R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRifleMotion-SM.IdleTurn180R");
            TRANS_AnyState_IdleToRun = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRifleMotion-SM.IdleToRun");
            TRANS_EntryState_IdleToRun = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRifleMotion-SM.IdleToRun");
            TRANS_AnyState_WalkFwdLoop = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_EntryState_WalkFwdLoop = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_AnyState_RunFwdLoop = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            TRANS_EntryState_RunFwdLoop = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            TRANS_AnyState_IdleToWalk = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_EntryState_IdleToWalk = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_AnyState_IdleTurn180R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkRifleMotion-SM.IdleTurn180R");
            TRANS_EntryState_IdleTurn180R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkRifleMotion-SM.IdleTurn180R");
            STATE_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_IdleToWalk_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_IdleToWalk_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk -> Base Layer.WalkRifleMotion-SM.WalkToIdle_LDown");
            TRANS_IdleToWalk_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk -> Base Layer.WalkRifleMotion-SM.WalkToIdle_RDown");
            STATE_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun");
            TRANS_IdleToRun_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            TRANS_IdleToRun_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun -> Base Layer.WalkRifleMotion-SM.RunStop_LDown");
            TRANS_IdleToRun_RunStop_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun -> Base Layer.WalkRifleMotion-SM.RunStop_RDown");
            STATE_IdleTurn90L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn90L");
            TRANS_IdleTurn90L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn90L -> Base Layer.WalkRifleMotion-SM.IdlePose");
            TRANS_IdleTurn90L_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn90L -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_IdleTurn90L_IdleToWalk90L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn90L -> Base Layer.WalkRifleMotion-SM.IdleToWalk90L");
            TRANS_IdleTurn90L_IdleToRun90L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn90L -> Base Layer.WalkRifleMotion-SM.IdleToRun90L");
            TRANS_IdleTurn90L_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn90L -> Base Layer.WalkRifleMotion-SM.IdleToRun");
            STATE_IdleTurn180L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn180L");
            TRANS_IdleTurn180L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn180L -> Base Layer.WalkRifleMotion-SM.IdlePose");
            TRANS_IdleTurn180L_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn180L -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_IdleTurn180L_IdleToWalk180L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn180L -> Base Layer.WalkRifleMotion-SM.IdleToWalk180L");
            TRANS_IdleTurn180L_IdleToRun180L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn180L -> Base Layer.WalkRifleMotion-SM.IdleToRun180L");
            TRANS_IdleTurn180L_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn180L -> Base Layer.WalkRifleMotion-SM.IdleToRun");
            STATE_IdleToWalk90L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk90L");
            TRANS_IdleToWalk90L_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk90L -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_IdleToWalk90L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk90L -> Base Layer.WalkRifleMotion-SM.IdlePose");
            STATE_IdleToWalk180L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk180L");
            TRANS_IdleToWalk180L_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk180L -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_IdleToWalk180L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk180L -> Base Layer.WalkRifleMotion-SM.IdlePose");
            STATE_IdleToRun90L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun90L");
            TRANS_IdleToRun90L_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun90L -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            TRANS_IdleToRun90L_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun90L -> Base Layer.WalkRifleMotion-SM.RunStop_LDown");
            STATE_IdleToRun180L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun180L");
            TRANS_IdleToRun180L_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun180L -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            TRANS_IdleToRun180L_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun180L -> Base Layer.WalkRifleMotion-SM.RunStop_LDown");
            STATE_IdleTurn90R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn90R");
            TRANS_IdleTurn90R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn90R -> Base Layer.WalkRifleMotion-SM.IdlePose");
            TRANS_IdleTurn90R_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn90R -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_IdleTurn90R_IdleToWalk90R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn90R -> Base Layer.WalkRifleMotion-SM.IdleToWalk90R");
            TRANS_IdleTurn90R_IdleToRun90R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn90R -> Base Layer.WalkRifleMotion-SM.IdleToRun90R");
            TRANS_IdleTurn90R_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn90R -> Base Layer.WalkRifleMotion-SM.IdleToRun");
            STATE_IdleTurn180R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn180R");
            TRANS_IdleTurn180R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn180R -> Base Layer.WalkRifleMotion-SM.IdlePose");
            TRANS_IdleTurn180R_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn180R -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_IdleTurn180R_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn180R -> Base Layer.WalkRifleMotion-SM.IdleToWalk180R");
            TRANS_IdleTurn180R_IdleToRun180R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn180R -> Base Layer.WalkRifleMotion-SM.IdleToRun180R");
            TRANS_IdleTurn180R_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn180R -> Base Layer.WalkRifleMotion-SM.IdleToRun");
            STATE_IdleToWalk90R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk90R");
            TRANS_IdleToWalk90R_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk90R -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_IdleToWalk90R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk90R -> Base Layer.WalkRifleMotion-SM.IdlePose");
            STATE_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk180R");
            TRANS_IdleToWalk180R_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk180R -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_IdleToWalk180R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToWalk180R -> Base Layer.WalkRifleMotion-SM.IdlePose");
            STATE_IdleToRun90R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun90R");
            TRANS_IdleToRun90R_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun90R -> Base Layer.WalkRifleMotion-SM.RunStop_LDown");
            TRANS_IdleToRun90R_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun90R -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            STATE_IdleToRun180R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun180R");
            TRANS_IdleToRun180R_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun180R -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            TRANS_IdleToRun180R_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleToRun180R -> Base Layer.WalkRifleMotion-SM.RunStop_LDown");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdlePose");
            TRANS_IdlePose_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdlePose -> Base Layer.WalkRifleMotion-SM.IdleToWalk180R");
            TRANS_IdlePose_IdleToWalk90R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdlePose -> Base Layer.WalkRifleMotion-SM.IdleToWalk90R");
            TRANS_IdlePose_IdleToWalk180L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdlePose -> Base Layer.WalkRifleMotion-SM.IdleToWalk180L");
            TRANS_IdlePose_IdleToWalk90L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdlePose -> Base Layer.WalkRifleMotion-SM.IdleToWalk90L");
            TRANS_IdlePose_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdlePose -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_IdlePose_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdlePose -> Base Layer.WalkRifleMotion-SM.IdleToRun");
            TRANS_IdlePose_IdleToRun90L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdlePose -> Base Layer.WalkRifleMotion-SM.IdleToRun90L");
            TRANS_IdlePose_IdleToRun180L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdlePose -> Base Layer.WalkRifleMotion-SM.IdleToRun180L");
            TRANS_IdlePose_IdleToRun90R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdlePose -> Base Layer.WalkRifleMotion-SM.IdleToRun90R");
            TRANS_IdlePose_IdleToRun180R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdlePose -> Base Layer.WalkRifleMotion-SM.IdleToRun180R");
            STATE_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_WalkFwdLoop_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            TRANS_WalkFwdLoop_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkToIdle_RDown");
            TRANS_WalkFwdLoop_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkToIdle_LDown");
            TRANS_WalkFwdLoop_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            TRANS_WalkFwdLoop_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            TRANS_WalkFwdLoop_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkPivot180_L");
            TRANS_WalkFwdLoop_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkPivot180_L");
            TRANS_WalkFwdLoop_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkPivot180_L");
            TRANS_WalkFwdLoop_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkPivot180_L");
            TRANS_WalkFwdLoop_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkPivot180_L");
            TRANS_WalkFwdLoop_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkPivot180_L");
            TRANS_WalkFwdLoop_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkToIdle_LDown");
            TRANS_WalkFwdLoop_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkToIdle_RDown");
            STATE_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            TRANS_RunFwdLoop_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_RunFwdLoop_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunFwdLoop -> Base Layer.WalkRifleMotion-SM.RunStop_LDown");
            TRANS_RunFwdLoop_RunPivot180L_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunFwdLoop -> Base Layer.WalkRifleMotion-SM.RunPivot180L_RDown");
            TRANS_RunFwdLoop_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunFwdLoop -> Base Layer.WalkRifleMotion-SM.RunPivot180R_LDown");
            TRANS_RunFwdLoop_RunPivot180L_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunFwdLoop -> Base Layer.WalkRifleMotion-SM.RunPivot180L_LDown");
            TRANS_RunFwdLoop_RunPivot180R_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunFwdLoop -> Base Layer.WalkRifleMotion-SM.RunPivot180R_RDown");
            TRANS_RunFwdLoop_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_RunFwdLoop_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunFwdLoop -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_RunFwdLoop_RunStop_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunFwdLoop -> Base Layer.WalkRifleMotion-SM.RunStop_RDown");
            TRANS_RunFwdLoop_RunStop_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunFwdLoop -> Base Layer.WalkRifleMotion-SM.RunStop_RDown");
            TRANS_RunFwdLoop_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunFwdLoop -> Base Layer.WalkRifleMotion-SM.RunStop_LDown");
            STATE_RunPivot180L_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunPivot180L_RDown");
            TRANS_RunPivot180L_RDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunPivot180L_RDown -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            STATE_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunPivot180R_LDown");
            TRANS_RunPivot180R_LDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunPivot180R_LDown -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            STATE_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_RDown");
            TRANS_WalkToIdle_RDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_RDown -> Base Layer.WalkRifleMotion-SM.IdlePose");
            TRANS_WalkToIdle_RDown_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_RDown -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_WalkToIdle_RDown_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_RDown -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_WalkToIdle_RDown_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_RDown -> Base Layer.WalkRifleMotion-SM.WalkPivot180_L");
            TRANS_WalkToIdle_RDown_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_RDown -> Base Layer.WalkRifleMotion-SM.WalkPivot180_L");
            TRANS_WalkToIdle_RDown_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_RDown -> Base Layer.WalkRifleMotion-SM.IdleToWalk180R");
            TRANS_WalkToIdle_RDown_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_RDown -> Base Layer.WalkRifleMotion-SM.IdleToWalk180R");
            STATE_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_LDown");
            TRANS_WalkToIdle_LDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_LDown -> Base Layer.WalkRifleMotion-SM.IdlePose");
            TRANS_WalkToIdle_LDown_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_LDown -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
            TRANS_WalkToIdle_LDown_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_LDown -> Base Layer.WalkRifleMotion-SM.WalkPivot180_L");
            TRANS_WalkToIdle_LDown_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_LDown -> Base Layer.WalkRifleMotion-SM.WalkPivot180_L");
            TRANS_WalkToIdle_LDown_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_LDown -> Base Layer.WalkRifleMotion-SM.IdleToWalk180R");
            TRANS_WalkToIdle_LDown_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_LDown -> Base Layer.WalkRifleMotion-SM.IdleToWalk180R");
            TRANS_WalkToIdle_LDown_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkToIdle_LDown -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            STATE_RunStop_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunStop_RDown");
            TRANS_RunStop_RDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunStop_RDown -> Base Layer.WalkRifleMotion-SM.IdlePose");
            TRANS_RunStop_RDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunStop_RDown -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            TRANS_RunStop_RDown_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunStop_RDown -> Base Layer.WalkRifleMotion-SM.RunPivot180R_LDown");
            TRANS_RunStop_RDown_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunStop_RDown -> Base Layer.WalkRifleMotion-SM.RunPivot180R_LDown");
            STATE_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunStop_LDown");
            TRANS_RunStop_LDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunStop_LDown -> Base Layer.WalkRifleMotion-SM.IdlePose");
            TRANS_RunStop_LDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunStop_LDown -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            TRANS_RunStop_LDown_RunPivot180R_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunStop_LDown -> Base Layer.WalkRifleMotion-SM.RunPivot180R_RDown");
            TRANS_RunStop_LDown_RunPivot180R_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunStop_LDown -> Base Layer.WalkRifleMotion-SM.RunPivot180R_RDown");
            STATE_RunPivot180L_LDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunPivot180L_LDown");
            TRANS_RunPivot180L_LDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunPivot180L_LDown -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            STATE_RunPivot180R_RDown = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunPivot180R_RDown");
            TRANS_RunPivot180R_RDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.RunPivot180R_RDown -> Base Layer.WalkRifleMotion-SM.RunFwdLoop");
            STATE_IdleTurn20R = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn20R");
            TRANS_IdleTurn20R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn20R -> Base Layer.WalkRifleMotion-SM.IdlePose");
            TRANS_IdleTurn20R_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn20R -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_IdleTurn20R_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn20R -> Base Layer.WalkRifleMotion-SM.IdleToRun");
            TRANS_IdleTurn20R_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn20R -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_IdleTurn20R_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn20R -> Base Layer.WalkRifleMotion-SM.IdleToRun");
            STATE_IdleTurn20L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn20L");
            TRANS_IdleTurn20L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn20L -> Base Layer.WalkRifleMotion-SM.IdlePose");
            TRANS_IdleTurn20L_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn20L -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_IdleTurn20L_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn20L -> Base Layer.WalkRifleMotion-SM.IdleToRun");
            TRANS_IdleTurn20L_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn20L -> Base Layer.WalkRifleMotion-SM.IdleToWalk");
            TRANS_IdleTurn20L_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.IdleTurn20L -> Base Layer.WalkRifleMotion-SM.IdleToRun");
            STATE_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkPivot180_L");
            TRANS_WalkPivot180_L_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkRifleMotion-SM.WalkPivot180_L -> Base Layer.WalkRifleMotion-SM.WalkFwdLoop");
        }

#if UNITY_EDITOR

        private AnimationClip m15540 = null;
        private AnimationClip m11682 = null;
        private AnimationClip m15518 = null;
        private AnimationClip m15522 = null;
        private AnimationClip m15542 = null;
        private AnimationClip m15546 = null;
        private AnimationClip m15524 = null;
        private AnimationClip m15528 = null;
        private AnimationClip m15544 = null;
        private AnimationClip m15548 = null;
        private AnimationClip m15454 = null;
        private AnimationClip m15538 = null;
        private AnimationClip m11680 = null;
        private AnimationClip m15550 = null;
        private AnimationClip m15552 = null;
        private AnimationClip m11684 = null;
        private AnimationClip m11686 = null;

        /// <summary>
        /// Creates the animator substate machine for this motion.
        /// </summary>
        protected override void CreateStateMachine()
        {
            // Grab the root sm for the layer
            UnityEditor.Animations.AnimatorStateMachine lRootStateMachine = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;
            UnityEditor.Animations.AnimatorStateMachine lSM_17600 = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;
            UnityEditor.Animations.AnimatorStateMachine lRootSubStateMachine = null;

            // If we find the sm with our name, remove it
            for (int i = 0; i < lRootStateMachine.stateMachines.Length; i++)
            {
                // Look for a sm with the matching name
                if (lRootStateMachine.stateMachines[i].stateMachine.name == _EditorAnimatorSMName)
                {
                    lRootSubStateMachine = lRootStateMachine.stateMachines[i].stateMachine;

                    // Allow the user to stop before we remove the sm
                    if (!UnityEditor.EditorUtility.DisplayDialog("Motion Controller", _EditorAnimatorSMName + " already exists. Delete and recreate it?", "Yes", "No"))
                    {
                        return;
                    }

                    // Remove the sm
                    //lRootStateMachine.RemoveStateMachine(lRootStateMachine.stateMachines[i].stateMachine);
                    break;
                }
            }

            UnityEditor.Animations.AnimatorStateMachine lSM_N50002 = lRootSubStateMachine;
            if (lSM_N50002 != null)
            {
                for (int i = lSM_N50002.entryTransitions.Length - 1; i >= 0; i--)
                {
                    lSM_N50002.RemoveEntryTransition(lSM_N50002.entryTransitions[i]);
                }

                for (int i = lSM_N50002.anyStateTransitions.Length - 1; i >= 0; i--)
                {
                    lSM_N50002.RemoveAnyStateTransition(lSM_N50002.anyStateTransitions[i]);
                }

                for (int i = lSM_N50002.states.Length - 1; i >= 0; i--)
                {
                    lSM_N50002.RemoveState(lSM_N50002.states[i].state);
                }

                for (int i = lSM_N50002.stateMachines.Length - 1; i >= 0; i--)
                {
                    lSM_N50002.RemoveStateMachine(lSM_N50002.stateMachines[i].stateMachine);
                }
            }
            else
            {
                lSM_N50002 = lSM_17600.AddStateMachine(_EditorAnimatorSMName, new Vector3(0, 0, 0));
            }

            UnityEditor.Animations.AnimatorState lS_N50004 = lSM_N50002.AddState("IdleToWalk", new Vector3(-420, 216, 0));
            lS_N50004.speed = 1.25f;
            lS_N50004.motion = m15540;

            UnityEditor.Animations.AnimatorState lS_N50006 = lSM_N50002.AddState("IdleToRun", new Vector3(456, 216, 0));
            lS_N50006.speed = 1f;
            lS_N50006.motion = m11682;

            UnityEditor.Animations.AnimatorState lS_N50008 = lSM_N50002.AddState("IdleTurn90L", new Vector3(-228, 60, 0));
            lS_N50008.speed = 1.5f;
            lS_N50008.motion = m15518;

            UnityEditor.Animations.AnimatorState lS_N50010 = lSM_N50002.AddState("IdleTurn180L", new Vector3(-144, 168, 0));
            lS_N50010.speed = 1.5f;
            lS_N50010.motion = m15522;

            UnityEditor.Animations.AnimatorState lS_N50012 = lSM_N50002.AddState("IdleToWalk90L", new Vector3(-420, 276, 0));
            lS_N50012.speed = 1f;
            lS_N50012.motion = m15542;

            UnityEditor.Animations.AnimatorState lS_N50014 = lSM_N50002.AddState("IdleToWalk180L", new Vector3(-420, 336, 0));
            lS_N50014.speed = 1f;
            lS_N50014.motion = m15546;

            UnityEditor.Animations.AnimatorState lS_N50016 = lSM_N50002.AddState("IdleToRun90L", new Vector3(456, 276, 0));
            lS_N50016.speed = 1f;
            lS_N50016.motion = m15542;

            UnityEditor.Animations.AnimatorState lS_N50018 = lSM_N50002.AddState("IdleToRun180L", new Vector3(456, 336, 0));
            lS_N50018.speed = 1f;
            lS_N50018.motion = m15546;

            UnityEditor.Animations.AnimatorState lS_N50020 = lSM_N50002.AddState("IdleTurn90R", new Vector3(288, 48, 0));
            lS_N50020.speed = 1.5f;
            lS_N50020.motion = m15524;

            UnityEditor.Animations.AnimatorState lS_N50022 = lSM_N50002.AddState("IdleTurn180R", new Vector3(192, 168, 0));
            lS_N50022.speed = 1.5f;
            lS_N50022.motion = m15528;

            UnityEditor.Animations.AnimatorState lS_N50024 = lSM_N50002.AddState("IdleToWalk90R", new Vector3(-420, 396, 0));
            lS_N50024.speed = 1f;
            lS_N50024.motion = m15544;

            UnityEditor.Animations.AnimatorState lS_N50026 = lSM_N50002.AddState("IdleToWalk180R", new Vector3(-420, 456, 0));
            lS_N50026.speed = 1f;
            lS_N50026.motion = m15548;

            UnityEditor.Animations.AnimatorState lS_N50028 = lSM_N50002.AddState("IdleToRun90R", new Vector3(456, 396, 0));
            lS_N50028.speed = 1f;
            lS_N50028.motion = m15544;

            UnityEditor.Animations.AnimatorState lS_N50030 = lSM_N50002.AddState("IdleToRun180R", new Vector3(456, 456, 0));
            lS_N50030.speed = 1f;
            lS_N50030.motion = m15548;

            UnityEditor.Animations.AnimatorState lS_N50032 = lSM_N50002.AddState("IdlePose", new Vector3(24, 372, 0));
            lS_N50032.speed = 1f;
            lS_N50032.motion = m15454;

            UnityEditor.Animations.AnimatorState lS_N50034 = lSM_N50002.AddState("WalkFwdLoop", new Vector3(-108, 588, 0));
            lS_N50034.speed = 1f;
            lS_N50034.motion = m15538;

            UnityEditor.Animations.AnimatorState lS_N50036 = lSM_N50002.AddState("RunFwdLoop", new Vector3(228, 588, 0));
            lS_N50036.speed = 1f;
            lS_N50036.motion = m11680;

            UnityEditor.Animations.AnimatorState lS_N50038 = lSM_N50002.AddState("RunPivot180L_RDown", new Vector3(36, 732, 0));
            lS_N50038.speed = 1f;
            lS_N50038.motion = m15522;

            UnityEditor.Animations.AnimatorState lS_N50040 = lSM_N50002.AddState("RunPivot180R_LDown", new Vector3(576, 792, 0));
            lS_N50040.speed = 1f;
            lS_N50040.motion = m15528;

            UnityEditor.Animations.AnimatorState lS_N50042 = lSM_N50002.AddState("WalkToIdle_RDown", new Vector3(-528, 636, 0));
            lS_N50042.speed = 1f;
            lS_N50042.motion = m15550;

            UnityEditor.Animations.AnimatorState lS_N50044 = lSM_N50002.AddState("WalkToIdle_LDown", new Vector3(-564, 588, 0));
            lS_N50044.speed = 1f;
            lS_N50044.motion = m15552;

            UnityEditor.Animations.AnimatorState lS_N50046 = lSM_N50002.AddState("RunStop_RDown", new Vector3(624, 672, 0));
            lS_N50046.speed = 1f;
            lS_N50046.motion = m11684;

            UnityEditor.Animations.AnimatorState lS_N50048 = lSM_N50002.AddState("RunStop_LDown", new Vector3(588, 588, 0));
            lS_N50048.speed = 1f;
            lS_N50048.motion = m11686;

            UnityEditor.Animations.AnimatorState lS_N50050 = lSM_N50002.AddState("RunPivot180L_LDown", new Vector3(120, 792, 0));
            lS_N50050.speed = 1f;
            lS_N50050.motion = m15522;

            UnityEditor.Animations.AnimatorState lS_N50052 = lSM_N50002.AddState("RunPivot180R_RDown", new Vector3(348, 792, 0));
            lS_N50052.speed = 1f;
            lS_N50052.motion = m15528;

            UnityEditor.Animations.AnimatorState lS_N50054 = lSM_N50002.AddState("IdleTurn20R", new Vector3(180, -48, 0));
            lS_N50054.speed = 1.1f;
            lS_N50054.motion = m15524;

            UnityEditor.Animations.AnimatorState lS_N50056 = lSM_N50002.AddState("IdleTurn20L", new Vector3(-120, -48, 0));
            lS_N50056.speed = 1.1f;
            lS_N50056.motion = m15518;

            UnityEditor.Animations.AnimatorState lS_N50058 = lSM_N50002.AddState("WalkPivot180_L", new Vector3(-264, 732, 0));
            lS_N50058.speed = 1f;
            lS_N50058.motion = m15546;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N50060 = lRootStateMachine.AddAnyStateTransition(lS_N50008);
            lT_N50060.hasExitTime = false;
            lT_N50060.hasFixedDuration = true;
            lT_N50060.exitTime = 0f;
            lT_N50060.duration = 0.05f;
            lT_N50060.offset = 0f;
            lT_N50060.mute = false;
            lT_N50060.solo = false;
            lT_N50060.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1.055975E+09f, "L0MotionPhase");
            lT_N50060.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -60f, "InputAngleFromAvatar");
            lT_N50060.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -160f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N50062 = lRootStateMachine.AddAnyStateTransition(lS_N50056);
            lT_N50062.hasExitTime = false;
            lT_N50062.hasFixedDuration = true;
            lT_N50062.exitTime = 0f;
            lT_N50062.duration = 0.05f;
            lT_N50062.offset = 0f;
            lT_N50062.mute = false;
            lT_N50062.solo = false;
            lT_N50062.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1.055975E+09f, "L0MotionPhase");
            lT_N50062.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -10f, "InputAngleFromAvatar");
            lT_N50062.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N50064 = lRootStateMachine.AddAnyStateTransition(lS_N50054);
            lT_N50064.hasExitTime = false;
            lT_N50064.hasFixedDuration = true;
            lT_N50064.exitTime = 0f;
            lT_N50064.duration = 0.05f;
            lT_N50064.offset = 0f;
            lT_N50064.mute = false;
            lT_N50064.solo = false;
            lT_N50064.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1.055975E+09f, "L0MotionPhase");
            lT_N50064.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 10f, "InputAngleFromAvatar");
            lT_N50064.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N50066 = lRootStateMachine.AddAnyStateTransition(lS_N50020);
            lT_N50066.hasExitTime = false;
            lT_N50066.hasFixedDuration = true;
            lT_N50066.exitTime = 0f;
            lT_N50066.duration = 0.05f;
            lT_N50066.offset = 0f;
            lT_N50066.mute = false;
            lT_N50066.solo = false;
            lT_N50066.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1.055975E+09f, "L0MotionPhase");
            lT_N50066.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 60f, "InputAngleFromAvatar");
            lT_N50066.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 160f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N50068 = lRootStateMachine.AddAnyStateTransition(lS_N50022);
            lT_N50068.hasExitTime = false;
            lT_N50068.hasFixedDuration = true;
            lT_N50068.exitTime = 0f;
            lT_N50068.duration = 0.05f;
            lT_N50068.offset = 0f;
            lT_N50068.mute = false;
            lT_N50068.solo = false;
            lT_N50068.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1.055975E+09f, "L0MotionPhase");
            lT_N50068.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N50070 = lRootStateMachine.AddAnyStateTransition(lS_N50006);
            lT_N50070.hasExitTime = false;
            lT_N50070.hasFixedDuration = true;
            lT_N50070.exitTime = 0f;
            lT_N50070.duration = 0.05f;
            lT_N50070.offset = 0f;
            lT_N50070.mute = false;
            lT_N50070.solo = false;
            lT_N50070.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1.055975E+09f, "L0MotionPhase");
            lT_N50070.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lT_N50070.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -10f, "InputAngleFromAvatar");
            lT_N50070.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 10f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N50072 = lRootStateMachine.AddAnyStateTransition(lS_N50034);
            lT_N50072.hasExitTime = false;
            lT_N50072.hasFixedDuration = true;
            lT_N50072.exitTime = 0.9f;
            lT_N50072.duration = 0.1f;
            lT_N50072.offset = 0f;
            lT_N50072.mute = false;
            lT_N50072.solo = false;
            lT_N50072.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1.055976E+09f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N50074 = lRootStateMachine.AddAnyStateTransition(lS_N50036);
            lT_N50074.hasExitTime = false;
            lT_N50074.hasFixedDuration = true;
            lT_N50074.exitTime = 0.9f;
            lT_N50074.duration = 0.1f;
            lT_N50074.offset = 0f;
            lT_N50074.mute = false;
            lT_N50074.solo = false;
            lT_N50074.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1.055977E+09f, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N50076 = lRootStateMachine.AddAnyStateTransition(lS_N50004);
            lT_N50076.hasExitTime = false;
            lT_N50076.hasFixedDuration = true;
            lT_N50076.exitTime = 0f;
            lT_N50076.duration = 0.05f;
            lT_N50076.offset = 0f;
            lT_N50076.mute = false;
            lT_N50076.solo = false;
            lT_N50076.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1.055975E+09f, "L0MotionPhase");
            lT_N50076.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50076.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lT_N50076.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -10f, "InputAngleFromAvatar");
            lT_N50076.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 10f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            UnityEditor.Animations.AnimatorStateTransition lT_N50078 = lRootStateMachine.AddAnyStateTransition(lS_N50022);
            lT_N50078.hasExitTime = false;
            lT_N50078.hasFixedDuration = true;
            lT_N50078.exitTime = 0f;
            lT_N50078.duration = 0.05f;
            lT_N50078.offset = 0f;
            lT_N50078.mute = false;
            lT_N50078.solo = false;
            lT_N50078.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1.055975E+09f, "L0MotionPhase");
            lT_N50078.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50080 = lS_N50004.AddTransition(lS_N50034);
            lT_N50080.hasExitTime = true;
            lT_N50080.hasFixedDuration = true;
            lT_N50080.exitTime = 1f;
            lT_N50080.duration = 0f;
            lT_N50080.offset = 0f;
            lT_N50080.mute = false;
            lT_N50080.solo = false;
            lT_N50080.canTransitionToSelf = true;
            lT_N50080.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50082 = lS_N50004.AddTransition(lS_N50044);
            lT_N50082.hasExitTime = true;
            lT_N50082.hasFixedDuration = true;
            lT_N50082.exitTime = 1f;
            lT_N50082.duration = 0f;
            lT_N50082.offset = 0f;
            lT_N50082.mute = false;
            lT_N50082.solo = false;
            lT_N50082.canTransitionToSelf = true;
            lT_N50082.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50084 = lS_N50004.AddTransition(lS_N50042);
            lT_N50084.hasExitTime = true;
            lT_N50084.hasFixedDuration = true;
            lT_N50084.exitTime = 0.4614975f;
            lT_N50084.duration = 0.1f;
            lT_N50084.offset = 0.1621253f;
            lT_N50084.mute = false;
            lT_N50084.solo = false;
            lT_N50084.canTransitionToSelf = true;
            lT_N50084.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50086 = lS_N50006.AddTransition(lS_N50036);
            lT_N50086.hasExitTime = true;
            lT_N50086.hasFixedDuration = true;
            lT_N50086.exitTime = 1f;
            lT_N50086.duration = 0.1f;
            lT_N50086.offset = 0f;
            lT_N50086.mute = false;
            lT_N50086.solo = false;
            lT_N50086.canTransitionToSelf = true;
            lT_N50086.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50088 = lS_N50006.AddTransition(lS_N50048);
            lT_N50088.hasExitTime = true;
            lT_N50088.hasFixedDuration = true;
            lT_N50088.exitTime = 1f;
            lT_N50088.duration = 0f;
            lT_N50088.offset = 0f;
            lT_N50088.mute = false;
            lT_N50088.solo = false;
            lT_N50088.canTransitionToSelf = true;
            lT_N50088.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50090 = lS_N50006.AddTransition(lS_N50046);
            lT_N50090.hasExitTime = true;
            lT_N50090.hasFixedDuration = true;
            lT_N50090.exitTime = 0.4f;
            lT_N50090.duration = 0.1f;
            lT_N50090.offset = 0f;
            lT_N50090.mute = false;
            lT_N50090.solo = false;
            lT_N50090.canTransitionToSelf = true;
            lT_N50090.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50092 = lS_N50008.AddTransition(lS_N50032);
            lT_N50092.hasExitTime = true;
            lT_N50092.hasFixedDuration = true;
            lT_N50092.exitTime = 1f;
            lT_N50092.duration = 0f;
            lT_N50092.offset = 0f;
            lT_N50092.mute = false;
            lT_N50092.solo = false;
            lT_N50092.canTransitionToSelf = true;
            lT_N50092.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50094 = lS_N50008.AddTransition(lS_N50004);
            lT_N50094.hasExitTime = true;
            lT_N50094.hasFixedDuration = true;
            lT_N50094.exitTime = 1f;
            lT_N50094.duration = 0f;
            lT_N50094.offset = 0f;
            lT_N50094.mute = false;
            lT_N50094.solo = false;
            lT_N50094.canTransitionToSelf = true;
            lT_N50094.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50094.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50096 = lS_N50008.AddTransition(lS_N50012);
            lT_N50096.hasExitTime = true;
            lT_N50096.hasFixedDuration = true;
            lT_N50096.exitTime = 0.2f;
            lT_N50096.duration = 0.1f;
            lT_N50096.offset = 0.1354761f;
            lT_N50096.mute = false;
            lT_N50096.solo = false;
            lT_N50096.canTransitionToSelf = true;
            lT_N50096.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50096.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50098 = lS_N50008.AddTransition(lS_N50016);
            lT_N50098.hasExitTime = true;
            lT_N50098.hasFixedDuration = true;
            lT_N50098.exitTime = 0.2f;
            lT_N50098.duration = 0.09999999f;
            lT_N50098.offset = 0.1202882f;
            lT_N50098.mute = false;
            lT_N50098.solo = false;
            lT_N50098.canTransitionToSelf = true;
            lT_N50098.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50100 = lS_N50008.AddTransition(lS_N50006);
            lT_N50100.hasExitTime = true;
            lT_N50100.hasFixedDuration = false;
            lT_N50100.exitTime = 1f;
            lT_N50100.duration = 0f;
            lT_N50100.offset = 0f;
            lT_N50100.mute = false;
            lT_N50100.solo = false;
            lT_N50100.canTransitionToSelf = true;
            lT_N50100.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50102 = lS_N50010.AddTransition(lS_N50032);
            lT_N50102.hasExitTime = true;
            lT_N50102.hasFixedDuration = true;
            lT_N50102.exitTime = 1f;
            lT_N50102.duration = 0f;
            lT_N50102.offset = 0f;
            lT_N50102.mute = false;
            lT_N50102.solo = false;
            lT_N50102.canTransitionToSelf = true;
            lT_N50102.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50104 = lS_N50010.AddTransition(lS_N50004);
            lT_N50104.hasExitTime = true;
            lT_N50104.hasFixedDuration = true;
            lT_N50104.exitTime = 1f;
            lT_N50104.duration = 0f;
            lT_N50104.offset = 0f;
            lT_N50104.mute = false;
            lT_N50104.solo = false;
            lT_N50104.canTransitionToSelf = true;
            lT_N50104.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50104.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50106 = lS_N50010.AddTransition(lS_N50014);
            lT_N50106.hasExitTime = true;
            lT_N50106.hasFixedDuration = true;
            lT_N50106.exitTime = 0.2f;
            lT_N50106.duration = 0.2f;
            lT_N50106.offset = 0.07468655f;
            lT_N50106.mute = false;
            lT_N50106.solo = false;
            lT_N50106.canTransitionToSelf = true;
            lT_N50106.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50106.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50108 = lS_N50010.AddTransition(lS_N50018);
            lT_N50108.hasExitTime = true;
            lT_N50108.hasFixedDuration = true;
            lT_N50108.exitTime = 0.2f;
            lT_N50108.duration = 0.2f;
            lT_N50108.offset = 0.09689496f;
            lT_N50108.mute = false;
            lT_N50108.solo = false;
            lT_N50108.canTransitionToSelf = true;
            lT_N50108.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50110 = lS_N50010.AddTransition(lS_N50006);
            lT_N50110.hasExitTime = true;
            lT_N50110.hasFixedDuration = false;
            lT_N50110.exitTime = 1f;
            lT_N50110.duration = 0f;
            lT_N50110.offset = 0f;
            lT_N50110.mute = false;
            lT_N50110.solo = false;
            lT_N50110.canTransitionToSelf = true;
            lT_N50110.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50112 = lS_N50012.AddTransition(lS_N50034);
            lT_N50112.hasExitTime = true;
            lT_N50112.hasFixedDuration = true;
            lT_N50112.exitTime = 1f;
            lT_N50112.duration = 0f;
            lT_N50112.offset = 0f;
            lT_N50112.mute = false;
            lT_N50112.solo = false;
            lT_N50112.canTransitionToSelf = true;
            lT_N50112.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50114 = lS_N50012.AddTransition(lS_N50032);
            lT_N50114.hasExitTime = true;
            lT_N50114.hasFixedDuration = true;
            lT_N50114.exitTime = 1f;
            lT_N50114.duration = 0.15f;
            lT_N50114.offset = 0f;
            lT_N50114.mute = false;
            lT_N50114.solo = false;
            lT_N50114.canTransitionToSelf = true;
            lT_N50114.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50116 = lS_N50014.AddTransition(lS_N50034);
            lT_N50116.hasExitTime = true;
            lT_N50116.hasFixedDuration = true;
            lT_N50116.exitTime = 1f;
            lT_N50116.duration = 0f;
            lT_N50116.offset = 0f;
            lT_N50116.mute = false;
            lT_N50116.solo = false;
            lT_N50116.canTransitionToSelf = true;
            lT_N50116.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50118 = lS_N50014.AddTransition(lS_N50032);
            lT_N50118.hasExitTime = true;
            lT_N50118.hasFixedDuration = true;
            lT_N50118.exitTime = 1f;
            lT_N50118.duration = 0.15f;
            lT_N50118.offset = 0f;
            lT_N50118.mute = false;
            lT_N50118.solo = false;
            lT_N50118.canTransitionToSelf = true;
            lT_N50118.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50120 = lS_N50016.AddTransition(lS_N50036);
            lT_N50120.hasExitTime = true;
            lT_N50120.hasFixedDuration = true;
            lT_N50120.exitTime = 1f;
            lT_N50120.duration = 0.1f;
            lT_N50120.offset = 0f;
            lT_N50120.mute = false;
            lT_N50120.solo = false;
            lT_N50120.canTransitionToSelf = true;
            lT_N50120.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50122 = lS_N50016.AddTransition(lS_N50048);
            lT_N50122.hasExitTime = true;
            lT_N50122.hasFixedDuration = false;
            lT_N50122.exitTime = 1f;
            lT_N50122.duration = 0f;
            lT_N50122.offset = 0f;
            lT_N50122.mute = false;
            lT_N50122.solo = false;
            lT_N50122.canTransitionToSelf = true;
            lT_N50122.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50124 = lS_N50018.AddTransition(lS_N50036);
            lT_N50124.hasExitTime = true;
            lT_N50124.hasFixedDuration = true;
            lT_N50124.exitTime = 1f;
            lT_N50124.duration = 0.1f;
            lT_N50124.offset = 0f;
            lT_N50124.mute = false;
            lT_N50124.solo = false;
            lT_N50124.canTransitionToSelf = true;
            lT_N50124.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50126 = lS_N50018.AddTransition(lS_N50048);
            lT_N50126.hasExitTime = true;
            lT_N50126.hasFixedDuration = false;
            lT_N50126.exitTime = 1f;
            lT_N50126.duration = 0f;
            lT_N50126.offset = 0f;
            lT_N50126.mute = false;
            lT_N50126.solo = false;
            lT_N50126.canTransitionToSelf = true;
            lT_N50126.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50128 = lS_N50020.AddTransition(lS_N50032);
            lT_N50128.hasExitTime = true;
            lT_N50128.hasFixedDuration = true;
            lT_N50128.exitTime = 1f;
            lT_N50128.duration = 0f;
            lT_N50128.offset = 0f;
            lT_N50128.mute = false;
            lT_N50128.solo = false;
            lT_N50128.canTransitionToSelf = true;
            lT_N50128.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50130 = lS_N50020.AddTransition(lS_N50004);
            lT_N50130.hasExitTime = true;
            lT_N50130.hasFixedDuration = true;
            lT_N50130.exitTime = 1f;
            lT_N50130.duration = 0f;
            lT_N50130.offset = 0f;
            lT_N50130.mute = false;
            lT_N50130.solo = false;
            lT_N50130.canTransitionToSelf = true;
            lT_N50130.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50130.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50132 = lS_N50020.AddTransition(lS_N50024);
            lT_N50132.hasExitTime = true;
            lT_N50132.hasFixedDuration = true;
            lT_N50132.exitTime = 0.2f;
            lT_N50132.duration = 0.09999999f;
            lT_N50132.offset = 0.1567315f;
            lT_N50132.mute = false;
            lT_N50132.solo = false;
            lT_N50132.canTransitionToSelf = true;
            lT_N50132.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50132.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50134 = lS_N50020.AddTransition(lS_N50028);
            lT_N50134.hasExitTime = true;
            lT_N50134.hasFixedDuration = true;
            lT_N50134.exitTime = 0.2f;
            lT_N50134.duration = 0.09999999f;
            lT_N50134.offset = 0.09090913f;
            lT_N50134.mute = false;
            lT_N50134.solo = false;
            lT_N50134.canTransitionToSelf = true;
            lT_N50134.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50136 = lS_N50020.AddTransition(lS_N50006);
            lT_N50136.hasExitTime = true;
            lT_N50136.hasFixedDuration = false;
            lT_N50136.exitTime = 1f;
            lT_N50136.duration = 0f;
            lT_N50136.offset = 0f;
            lT_N50136.mute = false;
            lT_N50136.solo = false;
            lT_N50136.canTransitionToSelf = true;
            lT_N50136.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50138 = lS_N50022.AddTransition(lS_N50032);
            lT_N50138.hasExitTime = true;
            lT_N50138.hasFixedDuration = true;
            lT_N50138.exitTime = 1f;
            lT_N50138.duration = 0f;
            lT_N50138.offset = 0f;
            lT_N50138.mute = false;
            lT_N50138.solo = false;
            lT_N50138.canTransitionToSelf = true;
            lT_N50138.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50140 = lS_N50022.AddTransition(lS_N50004);
            lT_N50140.hasExitTime = true;
            lT_N50140.hasFixedDuration = true;
            lT_N50140.exitTime = 1f;
            lT_N50140.duration = 0f;
            lT_N50140.offset = 0f;
            lT_N50140.mute = false;
            lT_N50140.solo = false;
            lT_N50140.canTransitionToSelf = true;
            lT_N50140.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50140.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50142 = lS_N50022.AddTransition(lS_N50026);
            lT_N50142.hasExitTime = true;
            lT_N50142.hasFixedDuration = true;
            lT_N50142.exitTime = 0.2f;
            lT_N50142.duration = 0.2f;
            lT_N50142.offset = 0.1132088f;
            lT_N50142.mute = false;
            lT_N50142.solo = false;
            lT_N50142.canTransitionToSelf = true;
            lT_N50142.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50142.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50144 = lS_N50022.AddTransition(lS_N50030);
            lT_N50144.hasExitTime = true;
            lT_N50144.hasFixedDuration = true;
            lT_N50144.exitTime = 0.2f;
            lT_N50144.duration = 0.2f;
            lT_N50144.offset = 0.0738495f;
            lT_N50144.mute = false;
            lT_N50144.solo = false;
            lT_N50144.canTransitionToSelf = true;
            lT_N50144.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50146 = lS_N50022.AddTransition(lS_N50006);
            lT_N50146.hasExitTime = true;
            lT_N50146.hasFixedDuration = false;
            lT_N50146.exitTime = 1f;
            lT_N50146.duration = 0f;
            lT_N50146.offset = 0f;
            lT_N50146.mute = false;
            lT_N50146.solo = false;
            lT_N50146.canTransitionToSelf = true;
            lT_N50146.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50148 = lS_N50024.AddTransition(lS_N50034);
            lT_N50148.hasExitTime = true;
            lT_N50148.hasFixedDuration = true;
            lT_N50148.exitTime = 1f;
            lT_N50148.duration = 0f;
            lT_N50148.offset = 0f;
            lT_N50148.mute = false;
            lT_N50148.solo = false;
            lT_N50148.canTransitionToSelf = true;
            lT_N50148.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50150 = lS_N50024.AddTransition(lS_N50032);
            lT_N50150.hasExitTime = true;
            lT_N50150.hasFixedDuration = true;
            lT_N50150.exitTime = 1f;
            lT_N50150.duration = 0.15f;
            lT_N50150.offset = 0f;
            lT_N50150.mute = false;
            lT_N50150.solo = false;
            lT_N50150.canTransitionToSelf = true;
            lT_N50150.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50152 = lS_N50026.AddTransition(lS_N50034);
            lT_N50152.hasExitTime = true;
            lT_N50152.hasFixedDuration = true;
            lT_N50152.exitTime = 1f;
            lT_N50152.duration = 0f;
            lT_N50152.offset = 0f;
            lT_N50152.mute = false;
            lT_N50152.solo = false;
            lT_N50152.canTransitionToSelf = true;
            lT_N50152.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50154 = lS_N50026.AddTransition(lS_N50032);
            lT_N50154.hasExitTime = true;
            lT_N50154.hasFixedDuration = true;
            lT_N50154.exitTime = 1f;
            lT_N50154.duration = 0.15f;
            lT_N50154.offset = 0f;
            lT_N50154.mute = false;
            lT_N50154.solo = false;
            lT_N50154.canTransitionToSelf = true;
            lT_N50154.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50156 = lS_N50028.AddTransition(lS_N50048);
            lT_N50156.hasExitTime = true;
            lT_N50156.hasFixedDuration = false;
            lT_N50156.exitTime = 1f;
            lT_N50156.duration = 0f;
            lT_N50156.offset = 0f;
            lT_N50156.mute = false;
            lT_N50156.solo = false;
            lT_N50156.canTransitionToSelf = true;
            lT_N50156.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50158 = lS_N50028.AddTransition(lS_N50036);
            lT_N50158.hasExitTime = true;
            lT_N50158.hasFixedDuration = true;
            lT_N50158.exitTime = 1f;
            lT_N50158.duration = 0.1f;
            lT_N50158.offset = 0f;
            lT_N50158.mute = false;
            lT_N50158.solo = false;
            lT_N50158.canTransitionToSelf = true;
            lT_N50158.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50160 = lS_N50030.AddTransition(lS_N50036);
            lT_N50160.hasExitTime = true;
            lT_N50160.hasFixedDuration = true;
            lT_N50160.exitTime = 1f;
            lT_N50160.duration = 0.1f;
            lT_N50160.offset = 0f;
            lT_N50160.mute = false;
            lT_N50160.solo = false;
            lT_N50160.canTransitionToSelf = true;
            lT_N50160.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50162 = lS_N50030.AddTransition(lS_N50048);
            lT_N50162.hasExitTime = true;
            lT_N50162.hasFixedDuration = false;
            lT_N50162.exitTime = 1f;
            lT_N50162.duration = 0f;
            lT_N50162.offset = 0f;
            lT_N50162.mute = false;
            lT_N50162.solo = false;
            lT_N50162.canTransitionToSelf = true;
            lT_N50162.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50164 = lS_N50032.AddTransition(lS_N50026);
            lT_N50164.hasExitTime = false;
            lT_N50164.hasFixedDuration = true;
            lT_N50164.exitTime = 0f;
            lT_N50164.duration = 0f;
            lT_N50164.offset = 0f;
            lT_N50164.mute = false;
            lT_N50164.solo = false;
            lT_N50164.canTransitionToSelf = true;
            lT_N50164.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_N50164.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50164.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lT_N50164.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50166 = lS_N50032.AddTransition(lS_N50024);
            lT_N50166.hasExitTime = false;
            lT_N50166.hasFixedDuration = true;
            lT_N50166.exitTime = 0f;
            lT_N50166.duration = 0f;
            lT_N50166.offset = 0f;
            lT_N50166.mute = false;
            lT_N50166.solo = false;
            lT_N50166.canTransitionToSelf = true;
            lT_N50166.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_N50166.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50166.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lT_N50166.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 60f, "InputAngleFromAvatar");
            lT_N50166.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 160f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50168 = lS_N50032.AddTransition(lS_N50014);
            lT_N50168.hasExitTime = false;
            lT_N50168.hasFixedDuration = true;
            lT_N50168.exitTime = 0f;
            lT_N50168.duration = 0f;
            lT_N50168.offset = 0f;
            lT_N50168.mute = false;
            lT_N50168.solo = false;
            lT_N50168.canTransitionToSelf = true;
            lT_N50168.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_N50168.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50168.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lT_N50168.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50170 = lS_N50032.AddTransition(lS_N50012);
            lT_N50170.hasExitTime = false;
            lT_N50170.hasFixedDuration = true;
            lT_N50170.exitTime = 0f;
            lT_N50170.duration = 0f;
            lT_N50170.offset = 0f;
            lT_N50170.mute = false;
            lT_N50170.solo = false;
            lT_N50170.canTransitionToSelf = true;
            lT_N50170.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_N50170.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50170.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lT_N50170.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -60f, "InputAngleFromAvatar");
            lT_N50170.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -160f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50172 = lS_N50032.AddTransition(lS_N50004);
            lT_N50172.hasExitTime = false;
            lT_N50172.hasFixedDuration = true;
            lT_N50172.exitTime = 0f;
            lT_N50172.duration = 0f;
            lT_N50172.offset = 0f;
            lT_N50172.mute = false;
            lT_N50172.solo = false;
            lT_N50172.canTransitionToSelf = true;
            lT_N50172.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_N50172.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50172.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lT_N50172.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");
            lT_N50172.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50174 = lS_N50032.AddTransition(lS_N50006);
            lT_N50174.hasExitTime = false;
            lT_N50174.hasFixedDuration = false;
            lT_N50174.exitTime = 0f;
            lT_N50174.duration = 0f;
            lT_N50174.offset = 0f;
            lT_N50174.mute = false;
            lT_N50174.solo = false;
            lT_N50174.canTransitionToSelf = true;
            lT_N50174.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lT_N50174.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");
            lT_N50174.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50176 = lS_N50032.AddTransition(lS_N50016);
            lT_N50176.hasExitTime = false;
            lT_N50176.hasFixedDuration = false;
            lT_N50176.exitTime = 0f;
            lT_N50176.duration = 0f;
            lT_N50176.offset = 0f;
            lT_N50176.mute = false;
            lT_N50176.solo = false;
            lT_N50176.canTransitionToSelf = true;
            lT_N50176.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lT_N50176.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -60f, "InputAngleFromAvatar");
            lT_N50176.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -160f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50178 = lS_N50032.AddTransition(lS_N50018);
            lT_N50178.hasExitTime = false;
            lT_N50178.hasFixedDuration = false;
            lT_N50178.exitTime = 0f;
            lT_N50178.duration = 0f;
            lT_N50178.offset = 0f;
            lT_N50178.mute = false;
            lT_N50178.solo = false;
            lT_N50178.canTransitionToSelf = true;
            lT_N50178.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lT_N50178.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50180 = lS_N50032.AddTransition(lS_N50028);
            lT_N50180.hasExitTime = false;
            lT_N50180.hasFixedDuration = false;
            lT_N50180.exitTime = 0f;
            lT_N50180.duration = 0f;
            lT_N50180.offset = 0f;
            lT_N50180.mute = false;
            lT_N50180.solo = false;
            lT_N50180.canTransitionToSelf = true;
            lT_N50180.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lT_N50180.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 60f, "InputAngleFromAvatar");
            lT_N50180.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 160f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50182 = lS_N50032.AddTransition(lS_N50030);
            lT_N50182.hasExitTime = true;
            lT_N50182.hasFixedDuration = false;
            lT_N50182.exitTime = 0f;
            lT_N50182.duration = 0f;
            lT_N50182.offset = 0f;
            lT_N50182.mute = false;
            lT_N50182.solo = false;
            lT_N50182.canTransitionToSelf = true;
            lT_N50182.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lT_N50182.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50184 = lS_N50034.AddTransition(lS_N50036);
            lT_N50184.hasExitTime = true;
            lT_N50184.hasFixedDuration = false;
            lT_N50184.exitTime = 0.3f;
            lT_N50184.duration = 0.2f;
            lT_N50184.offset = 0.2510554f;
            lT_N50184.mute = false;
            lT_N50184.solo = false;
            lT_N50184.canTransitionToSelf = true;
            lT_N50184.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");
            lT_N50184.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50186 = lS_N50034.AddTransition(lS_N50042);
            lT_N50186.hasExitTime = true;
            lT_N50186.hasFixedDuration = true;
            lT_N50186.exitTime = 0.5039032f;
            lT_N50186.duration = 0f;
            lT_N50186.offset = 0f;
            lT_N50186.mute = false;
            lT_N50186.solo = false;
            lT_N50186.canTransitionToSelf = true;
            lT_N50186.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50188 = lS_N50034.AddTransition(lS_N50044);
            lT_N50188.hasExitTime = true;
            lT_N50188.hasFixedDuration = true;
            lT_N50188.exitTime = 1f;
            lT_N50188.duration = 0f;
            lT_N50188.offset = 0f;
            lT_N50188.mute = false;
            lT_N50188.solo = false;
            lT_N50188.canTransitionToSelf = true;
            lT_N50188.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50190 = lS_N50034.AddTransition(lS_N50036);
            lT_N50190.hasExitTime = true;
            lT_N50190.hasFixedDuration = false;
            lT_N50190.exitTime = 0.6f;
            lT_N50190.duration = 0.2f;
            lT_N50190.offset = 0.6110921f;
            lT_N50190.mute = false;
            lT_N50190.solo = false;
            lT_N50190.canTransitionToSelf = true;
            lT_N50190.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");
            lT_N50190.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50192 = lS_N50034.AddTransition(lS_N50036);
            lT_N50192.hasExitTime = true;
            lT_N50192.hasFixedDuration = false;
            lT_N50192.exitTime = 0.9f;
            lT_N50192.duration = 0.2f;
            lT_N50192.offset = 0.8995329f;
            lT_N50192.mute = false;
            lT_N50192.solo = false;
            lT_N50192.canTransitionToSelf = true;
            lT_N50192.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");
            lT_N50192.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50194 = lS_N50034.AddTransition(lS_N50058);
            lT_N50194.hasExitTime = true;
            lT_N50194.hasFixedDuration = true;
            lT_N50194.exitTime = 0.5f;
            lT_N50194.duration = 0.1f;
            lT_N50194.offset = 0f;
            lT_N50194.mute = false;
            lT_N50194.solo = false;
            lT_N50194.canTransitionToSelf = true;
            lT_N50194.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50196 = lS_N50034.AddTransition(lS_N50058);
            lT_N50196.hasExitTime = true;
            lT_N50196.hasFixedDuration = true;
            lT_N50196.exitTime = 0.05f;
            lT_N50196.duration = 0.1f;
            lT_N50196.offset = 0f;
            lT_N50196.mute = false;
            lT_N50196.solo = false;
            lT_N50196.canTransitionToSelf = true;
            lT_N50196.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50198 = lS_N50034.AddTransition(lS_N50058);
            lT_N50198.hasExitTime = true;
            lT_N50198.hasFixedDuration = true;
            lT_N50198.exitTime = 0.95f;
            lT_N50198.duration = 0.1f;
            lT_N50198.offset = 0f;
            lT_N50198.mute = false;
            lT_N50198.solo = false;
            lT_N50198.canTransitionToSelf = true;
            lT_N50198.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50200 = lS_N50034.AddTransition(lS_N50058);
            lT_N50200.hasExitTime = true;
            lT_N50200.hasFixedDuration = true;
            lT_N50200.exitTime = 0.5f;
            lT_N50200.duration = 0.1f;
            lT_N50200.offset = 0f;
            lT_N50200.mute = false;
            lT_N50200.solo = false;
            lT_N50200.canTransitionToSelf = true;
            lT_N50200.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50202 = lS_N50034.AddTransition(lS_N50058);
            lT_N50202.hasExitTime = true;
            lT_N50202.hasFixedDuration = true;
            lT_N50202.exitTime = 0.05f;
            lT_N50202.duration = 0.1f;
            lT_N50202.offset = 0f;
            lT_N50202.mute = false;
            lT_N50202.solo = false;
            lT_N50202.canTransitionToSelf = true;
            lT_N50202.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50204 = lS_N50034.AddTransition(lS_N50058);
            lT_N50204.hasExitTime = true;
            lT_N50204.hasFixedDuration = true;
            lT_N50204.exitTime = 0.95f;
            lT_N50204.duration = 0.1f;
            lT_N50204.offset = 0f;
            lT_N50204.mute = false;
            lT_N50204.solo = false;
            lT_N50204.canTransitionToSelf = true;
            lT_N50204.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50206 = lS_N50034.AddTransition(lS_N50044);
            lT_N50206.hasExitTime = true;
            lT_N50206.hasFixedDuration = true;
            lT_N50206.exitTime = 0.25f;
            lT_N50206.duration = 0.1f;
            lT_N50206.offset = 0.1131489f;
            lT_N50206.mute = false;
            lT_N50206.solo = false;
            lT_N50206.canTransitionToSelf = true;
            lT_N50206.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50208 = lS_N50034.AddTransition(lS_N50042);
            lT_N50208.hasExitTime = true;
            lT_N50208.hasFixedDuration = true;
            lT_N50208.exitTime = 0.75f;
            lT_N50208.duration = 0.1f;
            lT_N50208.offset = 0.1338524f;
            lT_N50208.mute = false;
            lT_N50208.solo = false;
            lT_N50208.canTransitionToSelf = true;
            lT_N50208.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50210 = lS_N50036.AddTransition(lS_N50034);
            lT_N50210.hasExitTime = true;
            lT_N50210.hasFixedDuration = false;
            lT_N50210.exitTime = 0.3f;
            lT_N50210.duration = 0.2f;
            lT_N50210.offset = 0.3139983f;
            lT_N50210.mute = false;
            lT_N50210.solo = false;
            lT_N50210.canTransitionToSelf = true;
            lT_N50210.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_N50210.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.4f, "InputMagnitude");
            lT_N50210.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50212 = lS_N50036.AddTransition(lS_N50048);
            lT_N50212.hasExitTime = true;
            lT_N50212.hasFixedDuration = false;
            lT_N50212.exitTime = 0.1f;
            lT_N50212.duration = 0.1f;
            lT_N50212.offset = 0.03113134f;
            lT_N50212.mute = false;
            lT_N50212.solo = false;
            lT_N50212.canTransitionToSelf = true;
            lT_N50212.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.5f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50214 = lS_N50036.AddTransition(lS_N50038);
            lT_N50214.hasExitTime = true;
            lT_N50214.hasFixedDuration = false;
            lT_N50214.exitTime = 0.53f;
            lT_N50214.duration = 0f;
            lT_N50214.offset = 0f;
            lT_N50214.mute = false;
            lT_N50214.solo = false;
            lT_N50214.canTransitionToSelf = true;
            lT_N50214.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lT_N50214.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50216 = lS_N50036.AddTransition(lS_N50040);
            lT_N50216.hasExitTime = true;
            lT_N50216.hasFixedDuration = false;
            lT_N50216.exitTime = 1f;
            lT_N50216.duration = 0f;
            lT_N50216.offset = 0f;
            lT_N50216.mute = false;
            lT_N50216.solo = false;
            lT_N50216.canTransitionToSelf = true;
            lT_N50216.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lT_N50216.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50218 = lS_N50036.AddTransition(lS_N50050);
            lT_N50218.hasExitTime = true;
            lT_N50218.hasFixedDuration = false;
            lT_N50218.exitTime = 1f;
            lT_N50218.duration = 0f;
            lT_N50218.offset = 0f;
            lT_N50218.mute = false;
            lT_N50218.solo = false;
            lT_N50218.canTransitionToSelf = true;
            lT_N50218.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lT_N50218.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50220 = lS_N50036.AddTransition(lS_N50052);
            lT_N50220.hasExitTime = true;
            lT_N50220.hasFixedDuration = false;
            lT_N50220.exitTime = 0.53f;
            lT_N50220.duration = 0f;
            lT_N50220.offset = 0f;
            lT_N50220.mute = false;
            lT_N50220.solo = false;
            lT_N50220.canTransitionToSelf = true;
            lT_N50220.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lT_N50220.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50222 = lS_N50036.AddTransition(lS_N50034);
            lT_N50222.hasExitTime = true;
            lT_N50222.hasFixedDuration = false;
            lT_N50222.exitTime = 0.6f;
            lT_N50222.duration = 0.2f;
            lT_N50222.offset = 0.6967632f;
            lT_N50222.mute = false;
            lT_N50222.solo = false;
            lT_N50222.canTransitionToSelf = true;
            lT_N50222.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_N50222.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.4f, "InputMagnitude");
            lT_N50222.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50224 = lS_N50036.AddTransition(lS_N50034);
            lT_N50224.hasExitTime = true;
            lT_N50224.hasFixedDuration = false;
            lT_N50224.exitTime = 0.9f;
            lT_N50224.duration = 0.2f;
            lT_N50224.offset = 0.9236227f;
            lT_N50224.mute = false;
            lT_N50224.solo = false;
            lT_N50224.canTransitionToSelf = true;
            lT_N50224.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lT_N50224.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.4f, "InputMagnitude");
            lT_N50224.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50226 = lS_N50036.AddTransition(lS_N50046);
            lT_N50226.hasExitTime = true;
            lT_N50226.hasFixedDuration = true;
            lT_N50226.exitTime = 0.55f;
            lT_N50226.duration = 0.1f;
            lT_N50226.offset = 0.01f;
            lT_N50226.mute = false;
            lT_N50226.solo = false;
            lT_N50226.canTransitionToSelf = true;
            lT_N50226.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.5f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50228 = lS_N50036.AddTransition(lS_N50046);
            lT_N50228.hasExitTime = true;
            lT_N50228.hasFixedDuration = true;
            lT_N50228.exitTime = 0.8f;
            lT_N50228.duration = 0.1f;
            lT_N50228.offset = 0.2348485f;
            lT_N50228.mute = false;
            lT_N50228.solo = false;
            lT_N50228.canTransitionToSelf = true;
            lT_N50228.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.5f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50230 = lS_N50036.AddTransition(lS_N50048);
            lT_N50230.hasExitTime = true;
            lT_N50230.hasFixedDuration = true;
            lT_N50230.exitTime = 0.3142294f;
            lT_N50230.duration = 0.1f;
            lT_N50230.offset = 0.181153f;
            lT_N50230.mute = false;
            lT_N50230.solo = false;
            lT_N50230.canTransitionToSelf = true;
            lT_N50230.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.5f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50232 = lS_N50038.AddTransition(lS_N50036);
            lT_N50232.hasExitTime = true;
            lT_N50232.hasFixedDuration = true;
            lT_N50232.exitTime = 1f;
            lT_N50232.duration = 0.1f;
            lT_N50232.offset = 0f;
            lT_N50232.mute = false;
            lT_N50232.solo = false;
            lT_N50232.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N50234 = lS_N50040.AddTransition(lS_N50036);
            lT_N50234.hasExitTime = true;
            lT_N50234.hasFixedDuration = true;
            lT_N50234.exitTime = 1f;
            lT_N50234.duration = 0.1f;
            lT_N50234.offset = 0f;
            lT_N50234.mute = false;
            lT_N50234.solo = false;
            lT_N50234.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N50236 = lS_N50042.AddTransition(lS_N50032);
            lT_N50236.hasExitTime = true;
            lT_N50236.hasFixedDuration = false;
            lT_N50236.exitTime = 1f;
            lT_N50236.duration = 0f;
            lT_N50236.offset = 0f;
            lT_N50236.mute = false;
            lT_N50236.solo = false;
            lT_N50236.canTransitionToSelf = true;
            lT_N50236.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50238 = lS_N50042.AddTransition(lS_N50034);
            lT_N50238.hasExitTime = true;
            lT_N50238.hasFixedDuration = false;
            lT_N50238.exitTime = 0.5057101f;
            lT_N50238.duration = 0.04744329f;
            lT_N50238.offset = 0f;
            lT_N50238.mute = false;
            lT_N50238.solo = false;
            lT_N50238.canTransitionToSelf = true;
            lT_N50238.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50238.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lT_N50238.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50240 = lS_N50042.AddTransition(lS_N50004);
            lT_N50240.hasExitTime = true;
            lT_N50240.hasFixedDuration = true;
            lT_N50240.exitTime = 1f;
            lT_N50240.duration = 0f;
            lT_N50240.offset = 0f;
            lT_N50240.mute = false;
            lT_N50240.solo = false;
            lT_N50240.canTransitionToSelf = true;
            lT_N50240.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50240.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -140f, "InputAngleFromAvatar");
            lT_N50240.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50242 = lS_N50042.AddTransition(lS_N50058);
            lT_N50242.hasExitTime = true;
            lT_N50242.hasFixedDuration = true;
            lT_N50242.exitTime = 0.65f;
            lT_N50242.duration = 0.05f;
            lT_N50242.offset = 0f;
            lT_N50242.mute = false;
            lT_N50242.solo = false;
            lT_N50242.canTransitionToSelf = true;
            lT_N50242.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50242.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50244 = lS_N50042.AddTransition(lS_N50058);
            lT_N50244.hasExitTime = true;
            lT_N50244.hasFixedDuration = true;
            lT_N50244.exitTime = 0.65f;
            lT_N50244.duration = 0.05000001f;
            lT_N50244.offset = 0f;
            lT_N50244.mute = false;
            lT_N50244.solo = false;
            lT_N50244.canTransitionToSelf = true;
            lT_N50244.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50244.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50246 = lS_N50042.AddTransition(lS_N50026);
            lT_N50246.hasExitTime = true;
            lT_N50246.hasFixedDuration = true;
            lT_N50246.exitTime = 1f;
            lT_N50246.duration = 0f;
            lT_N50246.offset = 0f;
            lT_N50246.mute = false;
            lT_N50246.solo = false;
            lT_N50246.canTransitionToSelf = true;
            lT_N50246.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50246.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50248 = lS_N50042.AddTransition(lS_N50026);
            lT_N50248.hasExitTime = true;
            lT_N50248.hasFixedDuration = true;
            lT_N50248.exitTime = 1f;
            lT_N50248.duration = 0f;
            lT_N50248.offset = 0f;
            lT_N50248.mute = false;
            lT_N50248.solo = false;
            lT_N50248.canTransitionToSelf = true;
            lT_N50248.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50248.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50250 = lS_N50044.AddTransition(lS_N50032);
            lT_N50250.hasExitTime = true;
            lT_N50250.hasFixedDuration = false;
            lT_N50250.exitTime = 1f;
            lT_N50250.duration = 0f;
            lT_N50250.offset = 0f;
            lT_N50250.mute = false;
            lT_N50250.solo = false;
            lT_N50250.canTransitionToSelf = true;
            lT_N50250.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50252 = lS_N50044.AddTransition(lS_N50034);
            lT_N50252.hasExitTime = true;
            lT_N50252.hasFixedDuration = false;
            lT_N50252.exitTime = 0.5250819f;
            lT_N50252.duration = 0.04743088f;
            lT_N50252.offset = 0.5119725f;
            lT_N50252.mute = false;
            lT_N50252.solo = false;
            lT_N50252.canTransitionToSelf = true;
            lT_N50252.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50252.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lT_N50252.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50254 = lS_N50044.AddTransition(lS_N50058);
            lT_N50254.hasExitTime = true;
            lT_N50254.hasFixedDuration = true;
            lT_N50254.exitTime = 0.65f;
            lT_N50254.duration = 0.05f;
            lT_N50254.offset = 0f;
            lT_N50254.mute = false;
            lT_N50254.solo = false;
            lT_N50254.canTransitionToSelf = true;
            lT_N50254.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50254.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50256 = lS_N50044.AddTransition(lS_N50058);
            lT_N50256.hasExitTime = true;
            lT_N50256.hasFixedDuration = true;
            lT_N50256.exitTime = 0.65f;
            lT_N50256.duration = 0.05000007f;
            lT_N50256.offset = 0f;
            lT_N50256.mute = false;
            lT_N50256.solo = false;
            lT_N50256.canTransitionToSelf = true;
            lT_N50256.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50256.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50258 = lS_N50044.AddTransition(lS_N50026);
            lT_N50258.hasExitTime = true;
            lT_N50258.hasFixedDuration = true;
            lT_N50258.exitTime = 1f;
            lT_N50258.duration = 0f;
            lT_N50258.offset = 0f;
            lT_N50258.mute = false;
            lT_N50258.solo = false;
            lT_N50258.canTransitionToSelf = true;
            lT_N50258.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50258.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50260 = lS_N50044.AddTransition(lS_N50026);
            lT_N50260.hasExitTime = true;
            lT_N50260.hasFixedDuration = true;
            lT_N50260.exitTime = 1f;
            lT_N50260.duration = 0f;
            lT_N50260.offset = 0f;
            lT_N50260.mute = false;
            lT_N50260.solo = false;
            lT_N50260.canTransitionToSelf = true;
            lT_N50260.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50260.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50262 = lS_N50044.AddTransition(lS_N50004);
            lT_N50262.hasExitTime = true;
            lT_N50262.hasFixedDuration = true;
            lT_N50262.exitTime = 1f;
            lT_N50262.duration = 0f;
            lT_N50262.offset = 0f;
            lT_N50262.mute = false;
            lT_N50262.solo = false;
            lT_N50262.canTransitionToSelf = true;
            lT_N50262.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50262.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -140f, "InputAngleFromAvatar");
            lT_N50262.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50264 = lS_N50046.AddTransition(lS_N50032);
            lT_N50264.hasExitTime = true;
            lT_N50264.hasFixedDuration = false;
            lT_N50264.exitTime = 0.8687333f;
            lT_N50264.duration = 0.1f;
            lT_N50264.offset = 0f;
            lT_N50264.mute = false;
            lT_N50264.solo = false;
            lT_N50264.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N50266 = lS_N50046.AddTransition(lS_N50036);
            lT_N50266.hasExitTime = true;
            lT_N50266.hasFixedDuration = false;
            lT_N50266.exitTime = 0.4475232f;
            lT_N50266.duration = 0.1973684f;
            lT_N50266.offset = 0f;
            lT_N50266.mute = false;
            lT_N50266.solo = false;
            lT_N50266.canTransitionToSelf = true;
            lT_N50266.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lT_N50266.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lT_N50266.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50268 = lS_N50046.AddTransition(lS_N50040);
            lT_N50268.hasExitTime = true;
            lT_N50268.hasFixedDuration = false;
            lT_N50268.exitTime = 0.3613918f;
            lT_N50268.duration = 0.1f;
            lT_N50268.offset = 0f;
            lT_N50268.mute = false;
            lT_N50268.solo = false;
            lT_N50268.canTransitionToSelf = true;
            lT_N50268.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lT_N50268.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50270 = lS_N50046.AddTransition(lS_N50040);
            lT_N50270.hasExitTime = true;
            lT_N50270.hasFixedDuration = false;
            lT_N50270.exitTime = 0.3613918f;
            lT_N50270.duration = 0.1f;
            lT_N50270.offset = 0f;
            lT_N50270.mute = false;
            lT_N50270.solo = false;
            lT_N50270.canTransitionToSelf = true;
            lT_N50270.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lT_N50270.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50272 = lS_N50048.AddTransition(lS_N50032);
            lT_N50272.hasExitTime = true;
            lT_N50272.hasFixedDuration = false;
            lT_N50272.exitTime = 0.7665359f;
            lT_N50272.duration = 0.1f;
            lT_N50272.offset = 0f;
            lT_N50272.mute = false;
            lT_N50272.solo = false;
            lT_N50272.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N50274 = lS_N50048.AddTransition(lS_N50036);
            lT_N50274.hasExitTime = true;
            lT_N50274.hasFixedDuration = false;
            lT_N50274.exitTime = 0.4860786f;
            lT_N50274.duration = 0.1217646f;
            lT_N50274.offset = 0f;
            lT_N50274.mute = false;
            lT_N50274.solo = false;
            lT_N50274.canTransitionToSelf = true;
            lT_N50274.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lT_N50274.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lT_N50274.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50276 = lS_N50048.AddTransition(lS_N50052);
            lT_N50276.hasExitTime = true;
            lT_N50276.hasFixedDuration = false;
            lT_N50276.exitTime = 0.3564995f;
            lT_N50276.duration = 0.1f;
            lT_N50276.offset = 0.03969417f;
            lT_N50276.mute = false;
            lT_N50276.solo = false;
            lT_N50276.canTransitionToSelf = true;
            lT_N50276.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lT_N50276.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50278 = lS_N50048.AddTransition(lS_N50052);
            lT_N50278.hasExitTime = true;
            lT_N50278.hasFixedDuration = false;
            lT_N50278.exitTime = 0.3564995f;
            lT_N50278.duration = 0.1f;
            lT_N50278.offset = 0.03969417f;
            lT_N50278.mute = false;
            lT_N50278.solo = false;
            lT_N50278.canTransitionToSelf = true;
            lT_N50278.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lT_N50278.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lT_N50280 = lS_N50050.AddTransition(lS_N50036);
            lT_N50280.hasExitTime = true;
            lT_N50280.hasFixedDuration = true;
            lT_N50280.exitTime = 1f;
            lT_N50280.duration = 0.1f;
            lT_N50280.offset = 0f;
            lT_N50280.mute = false;
            lT_N50280.solo = false;
            lT_N50280.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N50282 = lS_N50052.AddTransition(lS_N50036);
            lT_N50282.hasExitTime = true;
            lT_N50282.hasFixedDuration = true;
            lT_N50282.exitTime = 1f;
            lT_N50282.duration = 0.1f;
            lT_N50282.offset = 0f;
            lT_N50282.mute = false;
            lT_N50282.solo = false;
            lT_N50282.canTransitionToSelf = true;

            UnityEditor.Animations.AnimatorStateTransition lT_N50284 = lS_N50054.AddTransition(lS_N50032);
            lT_N50284.hasExitTime = true;
            lT_N50284.hasFixedDuration = true;
            lT_N50284.exitTime = 0.4f;
            lT_N50284.duration = 0.4f;
            lT_N50284.offset = 0f;
            lT_N50284.mute = false;
            lT_N50284.solo = false;
            lT_N50284.canTransitionToSelf = true;
            lT_N50284.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50286 = lS_N50054.AddTransition(lS_N50004);
            lT_N50286.hasExitTime = true;
            lT_N50286.hasFixedDuration = true;
            lT_N50286.exitTime = 1f;
            lT_N50286.duration = 0f;
            lT_N50286.offset = 0f;
            lT_N50286.mute = false;
            lT_N50286.solo = false;
            lT_N50286.canTransitionToSelf = true;
            lT_N50286.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50286.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50288 = lS_N50054.AddTransition(lS_N50006);
            lT_N50288.hasExitTime = true;
            lT_N50288.hasFixedDuration = false;
            lT_N50288.exitTime = 1f;
            lT_N50288.duration = 0f;
            lT_N50288.offset = 0f;
            lT_N50288.mute = false;
            lT_N50288.solo = false;
            lT_N50288.canTransitionToSelf = true;
            lT_N50288.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50290 = lS_N50054.AddTransition(lS_N50004);
            lT_N50290.hasExitTime = true;
            lT_N50290.hasFixedDuration = true;
            lT_N50290.exitTime = 0.5f;
            lT_N50290.duration = 0.1f;
            lT_N50290.offset = 0f;
            lT_N50290.mute = false;
            lT_N50290.solo = false;
            lT_N50290.canTransitionToSelf = true;
            lT_N50290.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50290.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50292 = lS_N50054.AddTransition(lS_N50006);
            lT_N50292.hasExitTime = true;
            lT_N50292.hasFixedDuration = true;
            lT_N50292.exitTime = 0.5f;
            lT_N50292.duration = 0.1f;
            lT_N50292.offset = 0f;
            lT_N50292.mute = false;
            lT_N50292.solo = false;
            lT_N50292.canTransitionToSelf = true;
            lT_N50292.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50294 = lS_N50056.AddTransition(lS_N50032);
            lT_N50294.hasExitTime = true;
            lT_N50294.hasFixedDuration = true;
            lT_N50294.exitTime = 0.4f;
            lT_N50294.duration = 0.4f;
            lT_N50294.offset = 0f;
            lT_N50294.mute = false;
            lT_N50294.solo = false;
            lT_N50294.canTransitionToSelf = true;
            lT_N50294.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50296 = lS_N50056.AddTransition(lS_N50004);
            lT_N50296.hasExitTime = true;
            lT_N50296.hasFixedDuration = true;
            lT_N50296.exitTime = 1f;
            lT_N50296.duration = 0f;
            lT_N50296.offset = 0f;
            lT_N50296.mute = false;
            lT_N50296.solo = false;
            lT_N50296.canTransitionToSelf = true;
            lT_N50296.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50296.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50298 = lS_N50056.AddTransition(lS_N50006);
            lT_N50298.hasExitTime = true;
            lT_N50298.hasFixedDuration = false;
            lT_N50298.exitTime = 1f;
            lT_N50298.duration = 0f;
            lT_N50298.offset = 0f;
            lT_N50298.mute = false;
            lT_N50298.solo = false;
            lT_N50298.canTransitionToSelf = true;
            lT_N50298.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50300 = lS_N50056.AddTransition(lS_N50004);
            lT_N50300.hasExitTime = true;
            lT_N50300.hasFixedDuration = true;
            lT_N50300.exitTime = 0.5f;
            lT_N50300.duration = 0.1f;
            lT_N50300.offset = 0f;
            lT_N50300.mute = false;
            lT_N50300.solo = false;
            lT_N50300.canTransitionToSelf = true;
            lT_N50300.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lT_N50300.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50302 = lS_N50056.AddTransition(lS_N50006);
            lT_N50302.hasExitTime = true;
            lT_N50302.hasFixedDuration = true;
            lT_N50302.exitTime = 0.5f;
            lT_N50302.duration = 0.1f;
            lT_N50302.offset = 0f;
            lT_N50302.mute = false;
            lT_N50302.solo = false;
            lT_N50302.canTransitionToSelf = true;
            lT_N50302.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            UnityEditor.Animations.AnimatorStateTransition lT_N50304 = lS_N50058.AddTransition(lS_N50034);
            lT_N50304.hasExitTime = true;
            lT_N50304.hasFixedDuration = true;
            lT_N50304.exitTime = 1f;
            lT_N50304.duration = 0f;
            lT_N50304.offset = 0f;
            lT_N50304.mute = false;
            lT_N50304.solo = false;
            lT_N50304.canTransitionToSelf = true;

        }

        /// <summary>
        /// Gathers the animations so we can use them when creating the sub-state machine.
        /// </summary>
        public override void FindAnimations()
        {
            m15540 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStart.anim", "Rifle_WalkFwdStart");
            m11682 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro_Sprint.fbx/Rifle_SprintStart.anim", "Rifle_SprintStart");
            m15518 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_TurnL_90.anim", "Rifle_TurnL_90");
            m15522 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_TurnL_180.anim", "Rifle_TurnL_180");
            m15542 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStart90_L.anim", "Rifle_WalkFwdStart90_L");
            m15546 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStart180_L.anim", "Rifle_WalkFwdStart180_L");
            m15524 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_TurnR_90.anim", "Rifle_TurnR_90");
            m15528 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_TurnR_180.anim", "Rifle_TurnR_180");
            m15544 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStart90_R.anim", "Rifle_WalkFwdStart90_R");
            m15548 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStart180_R.anim", "Rifle_WalkFwdStart180_R");
            m15454 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_Idle.anim", "Rifle_Idle");
            m15538 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdLoop.anim", "Rifle_WalkFwdLoop");
            m11680 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro_Sprint.fbx/Rifle_SprintLoop.anim", "Rifle_SprintLoop");
            m15550 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStop_LU.anim", "Rifle_WalkFwdStop_LU");
            m15552 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStop_RU.anim", "Rifle_WalkFwdStop_RU");
            m11684 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro_Sprint.fbx/Rifle_SprintStop_LU.anim", "Rifle_SprintStop_LU");
            m11686 = FindAnimationClip("Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro_Sprint.fbx/Rifle_SprintStop_RU.anim", "Rifle_SprintStop_RU");

            // Add the remaining functionality
            base.FindAnimations();
        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            m15540 = CreateAnimationField("IdleToWalk.Rifle_WalkFwdStart", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStart.anim", "Rifle_WalkFwdStart", m15540);
            m11682 = CreateAnimationField("IdleToRun.Rifle_SprintStart", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro_Sprint.fbx/Rifle_SprintStart.anim", "Rifle_SprintStart", m11682);
            m15518 = CreateAnimationField("IdleTurn90L.Rifle_TurnL_90", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_TurnL_90.anim", "Rifle_TurnL_90", m15518);
            m15522 = CreateAnimationField("IdleTurn180L.Rifle_TurnL_180", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_TurnL_180.anim", "Rifle_TurnL_180", m15522);
            m15542 = CreateAnimationField("IdleToWalk90L.Rifle_WalkFwdStart90_L", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStart90_L.anim", "Rifle_WalkFwdStart90_L", m15542);
            m15546 = CreateAnimationField("IdleToWalk180L.Rifle_WalkFwdStart180_L", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStart180_L.anim", "Rifle_WalkFwdStart180_L", m15546);
            m15524 = CreateAnimationField("IdleTurn90R.Rifle_TurnR_90", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_TurnR_90.anim", "Rifle_TurnR_90", m15524);
            m15528 = CreateAnimationField("IdleTurn180R.Rifle_TurnR_180", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_TurnR_180.anim", "Rifle_TurnR_180", m15528);
            m15544 = CreateAnimationField("IdleToWalk90R.Rifle_WalkFwdStart90_R", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStart90_R.anim", "Rifle_WalkFwdStart90_R", m15544);
            m15548 = CreateAnimationField("IdleToWalk180R.Rifle_WalkFwdStart180_R", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStart180_R.anim", "Rifle_WalkFwdStart180_R", m15548);
            m15454 = CreateAnimationField("IdlePose.Rifle_Idle", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_Idle.anim", "Rifle_Idle", m15454);
            m15538 = CreateAnimationField("WalkFwdLoop.Rifle_WalkFwdLoop", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdLoop.anim", "Rifle_WalkFwdLoop", m15538);
            m11680 = CreateAnimationField("RunFwdLoop.Rifle_SprintLoop", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro_Sprint.fbx/Rifle_SprintLoop.anim", "Rifle_SprintLoop", m11680);
            m15550 = CreateAnimationField("WalkToIdle_RDown.Rifle_WalkFwdStop_LU", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStop_LU.anim", "Rifle_WalkFwdStop_LU", m15550);
            m15552 = CreateAnimationField("WalkToIdle_LDown.Rifle_WalkFwdStop_RU", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro.fbx/Rifle_WalkFwdStop_RU.anim", "Rifle_WalkFwdStop_RU", m15552);
            m11684 = CreateAnimationField("RunStop_RDown.Rifle_SprintStop_LU", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro_Sprint.fbx/Rifle_SprintStop_LU.anim", "Rifle_SprintStop_LU", m11684);
            m11686 = CreateAnimationField("RunStop_LDown.Rifle_SprintStop_RU", "Assets/ThirdParty/RifleAnimsetPro/Animations/RifleAnimsetPro_Sprint.fbx/Rifle_SprintStop_RU.anim", "Rifle_SprintStop_RU", m11686);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion

    }
}

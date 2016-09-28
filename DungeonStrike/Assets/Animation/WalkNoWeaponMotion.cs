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
    [MotionName("Walk - No Weapon")]
    public class WalkNoWeaponMotion : MotionControllerMotion
    {
        /// <summary>
        /// Trigger values for the motion
        /// </summary>
        public const int PHASE_UNKNOWN = 0;
        public const int PHASE_START = 1055972000;
        public const int PHASE_START_SHORTCUT_WALK = 1055973000;
        public const int PHASE_START_SHORTCUT_RUN = 1055974000;

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
        public WalkNoWeaponMotion() : base()
        {
            _Priority = 5;
            _ActionAlias = "Run";
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkNoWeaponMotion-SM"; }
#endif
        }

        /// <summary>
        /// Controller constructor
        /// </summary>
        /// <param name="rController">Controller the motion belongs to</param>
        public WalkNoWeaponMotion(MotionController rController) : base(rController)
        {
            _Priority = 5;
            _ActionAlias = "Run";
            mIsStartable = true;

#if UNITY_EDITOR
            if (_EditorAnimatorSMName.Length == 0) { _EditorAnimatorSMName = "WalkNoWeaponMotion-SM"; }
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
        public static int TRANS_EntryState_IdleTurn90L = -1;
        public static int TRANS_AnyState_IdleTurn90L = -1;
        public static int TRANS_EntryState_IdleTurn20L = -1;
        public static int TRANS_AnyState_IdleTurn20L = -1;
        public static int TRANS_EntryState_IdleTurn20R = -1;
        public static int TRANS_AnyState_IdleTurn20R = -1;
        public static int TRANS_EntryState_IdleTurn90R = -1;
        public static int TRANS_AnyState_IdleTurn90R = -1;
        public static int TRANS_EntryState_IdleTurn180R = -1;
        public static int TRANS_AnyState_IdleTurn180R = -1;
        public static int TRANS_EntryState_IdleToRun = -1;
        public static int TRANS_AnyState_IdleToRun = -1;
        public static int TRANS_EntryState_WalkFwdLoop = -1;
        public static int TRANS_AnyState_WalkFwdLoop = -1;
        public static int TRANS_EntryState_RunFwdLoop = -1;
        public static int TRANS_AnyState_RunFwdLoop = -1;
        public static int TRANS_EntryState_IdleToWalk = -1;
        public static int TRANS_AnyState_IdleToWalk = -1;
        public static int STATE_IdleToWalk = -1;
        public static int TRANS_IdleToWalk_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk_WalkToIdle_LDown = -1;
        public static int TRANS_IdleToWalk_WalkToIdle_RDown = -1;
        public static int STATE_IdleToRun = -1;
        public static int TRANS_IdleToRun_RunFwdLoop = -1;
        public static int TRANS_IdleToRun_RunStop_LDown = -1;
        public static int TRANS_IdleToRun_RunStop_RDown = -1;
        public static int STATE_IdleTurn90L = -1;
        public static int TRANS_IdleTurn90L_IdlePose = -1;
        public static int TRANS_IdleTurn90L_IdleToWalk = -1;
        public static int TRANS_IdleTurn90L_IdleToWalk90L = -1;
        public static int TRANS_IdleTurn90L_IdleToRun90L = -1;
        public static int TRANS_IdleTurn90L_IdleToRun = -1;
        public static int STATE_IdleTurn180L = -1;
        public static int TRANS_IdleTurn180L_IdlePose = -1;
        public static int TRANS_IdleTurn180L_IdleToWalk = -1;
        public static int TRANS_IdleTurn180L_IdleToWalk180L = -1;
        public static int TRANS_IdleTurn180L_IdleToRun180L = -1;
        public static int TRANS_IdleTurn180L_IdleToRun = -1;
        public static int STATE_IdleToWalk90L = -1;
        public static int TRANS_IdleToWalk90L_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk90L_IdlePose = -1;
        public static int STATE_IdleToWalk180L = -1;
        public static int TRANS_IdleToWalk180L_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk180L_IdlePose = -1;
        public static int STATE_IdleToRun90L = -1;
        public static int TRANS_IdleToRun90L_RunFwdLoop = -1;
        public static int TRANS_IdleToRun90L_RunStop_LDown = -1;
        public static int STATE_IdleToRun180L = -1;
        public static int TRANS_IdleToRun180L_RunFwdLoop = -1;
        public static int TRANS_IdleToRun180L_RunStop_LDown = -1;
        public static int STATE_IdleTurn90R = -1;
        public static int TRANS_IdleTurn90R_IdlePose = -1;
        public static int TRANS_IdleTurn90R_IdleToWalk = -1;
        public static int TRANS_IdleTurn90R_IdleToWalk90R = -1;
        public static int TRANS_IdleTurn90R_IdleToRun90R = -1;
        public static int TRANS_IdleTurn90R_IdleToRun = -1;
        public static int STATE_IdleTurn180R = -1;
        public static int TRANS_IdleTurn180R_IdlePose = -1;
        public static int TRANS_IdleTurn180R_IdleToWalk = -1;
        public static int TRANS_IdleTurn180R_IdleToWalk180R = -1;
        public static int TRANS_IdleTurn180R_IdleToRun180R = -1;
        public static int TRANS_IdleTurn180R_IdleToRun = -1;
        public static int STATE_IdleToWalk90R = -1;
        public static int TRANS_IdleToWalk90R_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk90R_IdlePose = -1;
        public static int STATE_IdleToWalk180R = -1;
        public static int TRANS_IdleToWalk180R_WalkFwdLoop = -1;
        public static int TRANS_IdleToWalk180R_IdlePose = -1;
        public static int STATE_IdleToRun90R = -1;
        public static int TRANS_IdleToRun90R_RunStop_LDown = -1;
        public static int TRANS_IdleToRun90R_RunFwdLoop = -1;
        public static int STATE_IdleToRun180R = -1;
        public static int TRANS_IdleToRun180R_RunFwdLoop = -1;
        public static int TRANS_IdleToRun180R_RunStop_LDown = -1;
        public static int STATE_IdlePose = -1;
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
        public static int STATE_WalkFwdLoop = -1;
        public static int TRANS_WalkFwdLoop_RunFwdLoop = -1;
        public static int TRANS_WalkFwdLoop_WalkToIdle_RDown = -1;
        public static int TRANS_WalkFwdLoop_WalkToIdle_LDown = -1;
        public static int TRANS_WalkFwdLoop_WalkPivot180_L = -1;
        public static int STATE_RunFwdLoop = -1;
        public static int TRANS_RunFwdLoop_WalkFwdLoop = -1;
        public static int TRANS_RunFwdLoop_RunStop_LDown = -1;
        public static int TRANS_RunFwdLoop_RunPivot180L_RDown = -1;
        public static int TRANS_RunFwdLoop_RunPivot180R_LDown = -1;
        public static int TRANS_RunFwdLoop_RunPivot180L_LDown = -1;
        public static int TRANS_RunFwdLoop_RunPivot180R_RDown = -1;
        public static int TRANS_RunFwdLoop_RunStop_RDown = -1;
        public static int STATE_RunPivot180L_RDown = -1;
        public static int TRANS_RunPivot180L_RDown_RunFwdLoop = -1;
        public static int STATE_RunPivot180R_LDown = -1;
        public static int TRANS_RunPivot180R_LDown_RunFwdLoop = -1;
        public static int STATE_WalkToIdle_RDown = -1;
        public static int TRANS_WalkToIdle_RDown_IdlePose = -1;
        public static int TRANS_WalkToIdle_RDown_WalkFwdLoop = -1;
        public static int TRANS_WalkToIdle_RDown_IdleToWalk = -1;
        public static int TRANS_WalkToIdle_RDown_WalkPivot180_L = -1;
        public static int TRANS_WalkToIdle_RDown_IdleToWalk180R = -1;
        public static int STATE_WalkToIdle_LDown = -1;
        public static int TRANS_WalkToIdle_LDown_IdlePose = -1;
        public static int TRANS_WalkToIdle_LDown_WalkFwdLoop = -1;
        public static int TRANS_WalkToIdle_LDown_WalkPivot180_L = -1;
        public static int TRANS_WalkToIdle_LDown_IdleToWalk180R = -1;
        public static int TRANS_WalkToIdle_LDown_IdleToWalk = -1;
        public static int STATE_RunStop_RDown = -1;
        public static int TRANS_RunStop_RDown_IdlePose = -1;
        public static int TRANS_RunStop_RDown_RunFwdLoop = -1;
        public static int TRANS_RunStop_RDown_RunPivot180R_LDown = -1;
        public static int STATE_RunStop_LDown = -1;
        public static int TRANS_RunStop_LDown_IdlePose = -1;
        public static int TRANS_RunStop_LDown_RunFwdLoop = -1;
        public static int TRANS_RunStop_LDown_RunPivot180R_RDown = -1;
        public static int STATE_RunPivot180L_LDown = -1;
        public static int TRANS_RunPivot180L_LDown_RunFwdLoop = -1;
        public static int STATE_RunPivot180R_RDown = -1;
        public static int TRANS_RunPivot180R_RDown_RunFwdLoop = -1;
        public static int STATE_IdleTurn20R = -1;
        public static int TRANS_IdleTurn20R_IdlePose = -1;
        public static int TRANS_IdleTurn20R_IdleToWalk = -1;
        public static int TRANS_IdleTurn20R_IdleToRun = -1;
        public static int STATE_IdleTurn20L = -1;
        public static int TRANS_IdleTurn20L_IdlePose = -1;
        public static int TRANS_IdleTurn20L_IdleToWalk = -1;
        public static int TRANS_IdleTurn20L_IdleToRun = -1;
        public static int STATE_WalkPivot180_L = -1;
        public static int TRANS_WalkPivot180_L_WalkFwdLoop = -1;

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
                if (lTransitionID == TRANS_EntryState_IdleTurn90L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn90L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn20L) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn20L) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn20R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn20R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn90R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn90R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleTurn180R) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleTurn180R) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToRun) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToRun) { return true; }
                if (lTransitionID == TRANS_EntryState_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_AnyState_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_EntryState_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_AnyState_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_EntryState_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_AnyState_IdleToWalk) { return true; }
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
                if (lTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunStop_LDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunPivot180L_RDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunPivot180R_LDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunPivot180L_LDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunPivot180R_RDown) { return true; }
                if (lTransitionID == TRANS_RunFwdLoop_RunStop_RDown) { return true; }
                if (lTransitionID == TRANS_RunPivot180L_RDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunPivot180R_LDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_RDown_IdleToWalk180R) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_WalkFwdLoop) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_WalkPivot180_L) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_IdleToWalk180R) { return true; }
                if (lTransitionID == TRANS_WalkToIdle_LDown_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_RunStop_RDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_RunStop_RDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunStop_RDown_RunPivot180R_LDown) { return true; }
                if (lTransitionID == TRANS_RunStop_LDown_IdlePose) { return true; }
                if (lTransitionID == TRANS_RunStop_LDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunStop_LDown_RunPivot180R_RDown) { return true; }
                if (lTransitionID == TRANS_RunPivot180L_LDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_RunPivot180R_RDown_RunFwdLoop) { return true; }
                if (lTransitionID == TRANS_IdleTurn20R_IdlePose) { return true; }
                if (lTransitionID == TRANS_IdleTurn20R_IdleToWalk) { return true; }
                if (lTransitionID == TRANS_IdleTurn20R_IdleToRun) { return true; }
                if (lTransitionID == TRANS_IdleTurn20L_IdlePose) { return true; }
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
            if (rTransitionID == TRANS_EntryState_IdleTurn90L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn90L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn20L) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn20L) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn20R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn20R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn90R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn90R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleTurn180R) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleTurn180R) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToRun) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToRun) { return true; }
            if (rTransitionID == TRANS_EntryState_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_AnyState_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_EntryState_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_AnyState_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_EntryState_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_AnyState_IdleToWalk) { return true; }
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
            if (rTransitionID == TRANS_WalkFwdLoop_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunStop_LDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunPivot180L_RDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunPivot180R_LDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunPivot180L_LDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunPivot180R_RDown) { return true; }
            if (rTransitionID == TRANS_RunFwdLoop_RunStop_RDown) { return true; }
            if (rTransitionID == TRANS_RunPivot180L_RDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunPivot180R_LDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_RDown_IdleToWalk180R) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_WalkFwdLoop) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_WalkPivot180_L) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_IdleToWalk180R) { return true; }
            if (rTransitionID == TRANS_WalkToIdle_LDown_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_RunStop_RDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_RunStop_RDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunStop_RDown_RunPivot180R_LDown) { return true; }
            if (rTransitionID == TRANS_RunStop_LDown_IdlePose) { return true; }
            if (rTransitionID == TRANS_RunStop_LDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunStop_LDown_RunPivot180R_RDown) { return true; }
            if (rTransitionID == TRANS_RunPivot180L_LDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_RunPivot180R_RDown_RunFwdLoop) { return true; }
            if (rTransitionID == TRANS_IdleTurn20R_IdlePose) { return true; }
            if (rTransitionID == TRANS_IdleTurn20R_IdleToWalk) { return true; }
            if (rTransitionID == TRANS_IdleTurn20R_IdleToRun) { return true; }
            if (rTransitionID == TRANS_IdleTurn20L_IdlePose) { return true; }
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
            /// <summary>
            /// These assignments go inside the 'LoadAnimatorData' function so that we can
            /// extract and assign the hash values for this run. These are typically used for debugging.
            /// </summary>
            TRANS_EntryState_IdleTurn90L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkNoWeaponMotion-SM.IdleTurn90L");
            TRANS_AnyState_IdleTurn90L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkNoWeaponMotion-SM.IdleTurn90L");
            TRANS_EntryState_IdleTurn20L = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkNoWeaponMotion-SM.IdleTurn20L");
            TRANS_AnyState_IdleTurn20L = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkNoWeaponMotion-SM.IdleTurn20L");
            TRANS_EntryState_IdleTurn20R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkNoWeaponMotion-SM.IdleTurn20R");
            TRANS_AnyState_IdleTurn20R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkNoWeaponMotion-SM.IdleTurn20R");
            TRANS_EntryState_IdleTurn90R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkNoWeaponMotion-SM.IdleTurn90R");
            TRANS_AnyState_IdleTurn90R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkNoWeaponMotion-SM.IdleTurn90R");
            TRANS_EntryState_IdleTurn180R = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkNoWeaponMotion-SM.IdleTurn180R");
            TRANS_AnyState_IdleTurn180R = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkNoWeaponMotion-SM.IdleTurn180R");
            TRANS_EntryState_IdleToRun = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun");
            TRANS_AnyState_IdleToRun = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun");
            TRANS_EntryState_WalkFwdLoop = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop");
            TRANS_AnyState_WalkFwdLoop = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop");
            TRANS_EntryState_RunFwdLoop = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            TRANS_AnyState_RunFwdLoop = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            TRANS_EntryState_IdleToWalk = mMotionController.AddAnimatorName("Entry -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk");
            TRANS_AnyState_IdleToWalk = mMotionController.AddAnimatorName("AnyState -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk");
            STATE_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk");
            TRANS_IdleToWalk_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk -> Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop");
            TRANS_IdleToWalk_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk -> Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_LDown");
            TRANS_IdleToWalk_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk -> Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_RDown");
            STATE_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun");
            TRANS_IdleToRun_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            TRANS_IdleToRun_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun -> Base Layer.WalkNoWeaponMotion-SM.RunStop_LDown");
            TRANS_IdleToRun_RunStop_RDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun -> Base Layer.WalkNoWeaponMotion-SM.RunStop_RDown");
            STATE_IdleTurn90L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn90L");
            TRANS_IdleTurn90L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn90L -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            TRANS_IdleTurn90L_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn90L -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk");
            TRANS_IdleTurn90L_IdleToWalk90L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn90L -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk90L");
            TRANS_IdleTurn90L_IdleToRun90L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn90L -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun90L");
            TRANS_IdleTurn90L_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn90L -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun");
            STATE_IdleTurn180L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn180L");
            TRANS_IdleTurn180L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn180L -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            TRANS_IdleTurn180L_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn180L -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk");
            TRANS_IdleTurn180L_IdleToWalk180L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn180L -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk180L");
            TRANS_IdleTurn180L_IdleToRun180L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn180L -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun180L");
            TRANS_IdleTurn180L_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn180L -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun");
            STATE_IdleToWalk90L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk90L");
            TRANS_IdleToWalk90L_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk90L -> Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop");
            TRANS_IdleToWalk90L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk90L -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            STATE_IdleToWalk180L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk180L");
            TRANS_IdleToWalk180L_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk180L -> Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop");
            TRANS_IdleToWalk180L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk180L -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            STATE_IdleToRun90L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun90L");
            TRANS_IdleToRun90L_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun90L -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            TRANS_IdleToRun90L_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun90L -> Base Layer.WalkNoWeaponMotion-SM.RunStop_LDown");
            STATE_IdleToRun180L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun180L");
            TRANS_IdleToRun180L_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun180L -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            TRANS_IdleToRun180L_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun180L -> Base Layer.WalkNoWeaponMotion-SM.RunStop_LDown");
            STATE_IdleTurn90R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn90R");
            TRANS_IdleTurn90R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn90R -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            TRANS_IdleTurn90R_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn90R -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk");
            TRANS_IdleTurn90R_IdleToWalk90R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn90R -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk90R");
            TRANS_IdleTurn90R_IdleToRun90R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn90R -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun90R");
            TRANS_IdleTurn90R_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn90R -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun");
            STATE_IdleTurn180R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn180R");
            TRANS_IdleTurn180R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn180R -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            TRANS_IdleTurn180R_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn180R -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk");
            TRANS_IdleTurn180R_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn180R -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk180R");
            TRANS_IdleTurn180R_IdleToRun180R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn180R -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun180R");
            TRANS_IdleTurn180R_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn180R -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun");
            STATE_IdleToWalk90R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk90R");
            TRANS_IdleToWalk90R_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk90R -> Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop");
            TRANS_IdleToWalk90R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk90R -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            STATE_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk180R");
            TRANS_IdleToWalk180R_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk180R -> Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop");
            TRANS_IdleToWalk180R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToWalk180R -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            STATE_IdleToRun90R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun90R");
            TRANS_IdleToRun90R_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun90R -> Base Layer.WalkNoWeaponMotion-SM.RunStop_LDown");
            TRANS_IdleToRun90R_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun90R -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            STATE_IdleToRun180R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun180R");
            TRANS_IdleToRun180R_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun180R -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            TRANS_IdleToRun180R_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleToRun180R -> Base Layer.WalkNoWeaponMotion-SM.RunStop_LDown");
            STATE_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            TRANS_IdlePose_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdlePose -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk180R");
            TRANS_IdlePose_IdleToWalk90R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdlePose -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk90R");
            TRANS_IdlePose_IdleToWalk180L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdlePose -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk180L");
            TRANS_IdlePose_IdleToWalk90L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdlePose -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk90L");
            TRANS_IdlePose_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdlePose -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk");
            TRANS_IdlePose_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdlePose -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun");
            TRANS_IdlePose_IdleToRun90L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdlePose -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun90L");
            TRANS_IdlePose_IdleToRun180L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdlePose -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun180L");
            TRANS_IdlePose_IdleToRun90R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdlePose -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun90R");
            TRANS_IdlePose_IdleToRun180R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdlePose -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun180R");
            STATE_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop");
            TRANS_WalkFwdLoop_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            TRANS_WalkFwdLoop_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop -> Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_RDown");
            TRANS_WalkFwdLoop_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop -> Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_LDown");
            TRANS_WalkFwdLoop_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop -> Base Layer.WalkNoWeaponMotion-SM.WalkPivot180_L");
            STATE_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            TRANS_RunFwdLoop_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop -> Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop");
            TRANS_RunFwdLoop_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop -> Base Layer.WalkNoWeaponMotion-SM.RunStop_LDown");
            TRANS_RunFwdLoop_RunPivot180L_RDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop -> Base Layer.WalkNoWeaponMotion-SM.RunPivot180L_RDown");
            TRANS_RunFwdLoop_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop -> Base Layer.WalkNoWeaponMotion-SM.RunPivot180R_LDown");
            TRANS_RunFwdLoop_RunPivot180L_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop -> Base Layer.WalkNoWeaponMotion-SM.RunPivot180L_LDown");
            TRANS_RunFwdLoop_RunPivot180R_RDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop -> Base Layer.WalkNoWeaponMotion-SM.RunPivot180R_RDown");
            TRANS_RunFwdLoop_RunStop_RDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop -> Base Layer.WalkNoWeaponMotion-SM.RunStop_RDown");
            STATE_RunPivot180L_RDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunPivot180L_RDown");
            TRANS_RunPivot180L_RDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunPivot180L_RDown -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            STATE_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunPivot180R_LDown");
            TRANS_RunPivot180R_LDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunPivot180R_LDown -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            STATE_WalkToIdle_RDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_RDown");
            TRANS_WalkToIdle_RDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_RDown -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            TRANS_WalkToIdle_RDown_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_RDown -> Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop");
            TRANS_WalkToIdle_RDown_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_RDown -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk");
            TRANS_WalkToIdle_RDown_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_RDown -> Base Layer.WalkNoWeaponMotion-SM.WalkPivot180_L");
            TRANS_WalkToIdle_RDown_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_RDown -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk180R");
            STATE_WalkToIdle_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_LDown");
            TRANS_WalkToIdle_LDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_LDown -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            TRANS_WalkToIdle_LDown_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_LDown -> Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop");
            TRANS_WalkToIdle_LDown_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_LDown -> Base Layer.WalkNoWeaponMotion-SM.WalkPivot180_L");
            TRANS_WalkToIdle_LDown_IdleToWalk180R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_LDown -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk180R");
            TRANS_WalkToIdle_LDown_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkToIdle_LDown -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk");
            STATE_RunStop_RDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunStop_RDown");
            TRANS_RunStop_RDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunStop_RDown -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            TRANS_RunStop_RDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunStop_RDown -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            TRANS_RunStop_RDown_RunPivot180R_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunStop_RDown -> Base Layer.WalkNoWeaponMotion-SM.RunPivot180R_LDown");
            STATE_RunStop_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunStop_LDown");
            TRANS_RunStop_LDown_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunStop_LDown -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            TRANS_RunStop_LDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunStop_LDown -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            TRANS_RunStop_LDown_RunPivot180R_RDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunStop_LDown -> Base Layer.WalkNoWeaponMotion-SM.RunPivot180R_RDown");
            STATE_RunPivot180L_LDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunPivot180L_LDown");
            TRANS_RunPivot180L_LDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunPivot180L_LDown -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            STATE_RunPivot180R_RDown = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunPivot180R_RDown");
            TRANS_RunPivot180R_RDown_RunFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.RunPivot180R_RDown -> Base Layer.WalkNoWeaponMotion-SM.RunFwdLoop");
            STATE_IdleTurn20R = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn20R");
            TRANS_IdleTurn20R_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn20R -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            TRANS_IdleTurn20R_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn20R -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk");
            TRANS_IdleTurn20R_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn20R -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun");
            STATE_IdleTurn20L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn20L");
            TRANS_IdleTurn20L_IdlePose = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn20L -> Base Layer.WalkNoWeaponMotion-SM.IdlePose");
            TRANS_IdleTurn20L_IdleToWalk = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn20L -> Base Layer.WalkNoWeaponMotion-SM.IdleToWalk");
            TRANS_IdleTurn20L_IdleToRun = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.IdleTurn20L -> Base Layer.WalkNoWeaponMotion-SM.IdleToRun");
            STATE_WalkPivot180_L = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkPivot180_L");
            TRANS_WalkPivot180_L_WalkFwdLoop = mMotionController.AddAnimatorName("Base Layer.WalkNoWeaponMotion-SM.WalkPivot180_L -> Base Layer.WalkNoWeaponMotion-SM.WalkFwdLoop");
        }

#if UNITY_EDITOR

        private AnimationClip mWalkFwdStart = null;
        private AnimationClip mRunFwdStart = null;
        private AnimationClip mTurnLt90_Loop = null;
        private AnimationClip mTurnLt180 = null;
        private AnimationClip mWalkFwdStart90_L = null;
        private AnimationClip mWalkFwdStart180_L = null;
        private AnimationClip mRunFwdStart90_L = null;
        private AnimationClip mRunFwdStart180_L = null;
        private AnimationClip mTurnRt90_Loop = null;
        private AnimationClip mTurnRt180 = null;
        private AnimationClip mWalkFwdStart90_R = null;
        private AnimationClip mWalkFwdStart180_R = null;
        private AnimationClip mRunFwdStart90_R = null;
        private AnimationClip mRunFwdStart180_R = null;
        private AnimationClip mIdle = null;
        private AnimationClip mWalkFwdLoop = null;
        private AnimationClip mRunFwdLoop = null;
        private AnimationClip mRunFwdTurn180_L_LU = null;
        private AnimationClip mRunFwdTurn180_R_RU = null;
        private AnimationClip mWalkFwdStop_LU = null;
        private AnimationClip mWalkFwdStop_RU = null;
        private AnimationClip mRunFwdStop_LU = null;
        private AnimationClip mRunFwdStop_RU = null;
        private AnimationClip mRunFwdTurn180_L_RU = null;
        private AnimationClip mRunFwdTurn180_R_LU = null;

        /// <summary>
        /// Creates the animator substate machine for this motion.
        /// </summary>
        protected override void CreateStateMachine()
        {
            // Grab the root sm for the layer
            UnityEditor.Animations.AnimatorStateMachine lRootStateMachine = _EditorAnimatorController.layers[mMotionLayer.AnimatorLayerIndex].stateMachine;

            // If we find the sm with our name, remove it
            for (int i = 0; i < lRootStateMachine.stateMachines.Length; i++)
            {
                // Look for a sm with the matching name
                if (lRootStateMachine.stateMachines[i].stateMachine.name == _EditorAnimatorSMName)
                {
                    // Allow the user to stop before we remove the sm
                    if (!UnityEditor.EditorUtility.DisplayDialog("Motion Controller", _EditorAnimatorSMName + " already exists. Delete and recreate it?", "Yes", "No"))
                    {
                        return;
                    }

                    // Remove the sm
                    lRootStateMachine.RemoveStateMachine(lRootStateMachine.stateMachines[i].stateMachine);
                }
            }

            UnityEditor.Animations.AnimatorStateMachine lMotionStateMachine = lRootStateMachine.AddStateMachine(_EditorAnimatorSMName);

            // Attach the behaviour if needed
            if (_EditorAttachBehaviour)
            {
                MotionControllerBehaviour lBehaviour = lMotionStateMachine.AddStateMachineBehaviour(typeof(MotionControllerBehaviour)) as MotionControllerBehaviour;
                lBehaviour._MotionKey = (_Key.Length > 0 ? _Key : this.GetType().FullName);
            }

            UnityEditor.Animations.AnimatorState lIdleToWalk = lMotionStateMachine.AddState("IdleToWalk", new Vector3(-420, 216, 0));
            lIdleToWalk.motion = mWalkFwdStart;
            lIdleToWalk.speed = 1.25f;

            UnityEditor.Animations.AnimatorState lIdleToRun = lMotionStateMachine.AddState("IdleToRun", new Vector3(456, 216, 0));
            lIdleToRun.motion = mRunFwdStart;
            lIdleToRun.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleTurn90L = lMotionStateMachine.AddState("IdleTurn90L", new Vector3(-228, 60, 0));
            lIdleTurn90L.motion = mTurnLt90_Loop;
            lIdleTurn90L.speed = 1.5f;

            UnityEditor.Animations.AnimatorState lIdleTurn180L = lMotionStateMachine.AddState("IdleTurn180L", new Vector3(-144, 168, 0));
            lIdleTurn180L.motion = mTurnLt180;
            lIdleTurn180L.speed = 1.5f;

            UnityEditor.Animations.AnimatorState lIdleToWalk90L = lMotionStateMachine.AddState("IdleToWalk90L", new Vector3(-420, 276, 0));
            lIdleToWalk90L.motion = mWalkFwdStart90_L;
            lIdleToWalk90L.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleToWalk180L = lMotionStateMachine.AddState("IdleToWalk180L", new Vector3(-420, 336, 0));
            lIdleToWalk180L.motion = mWalkFwdStart180_L;
            lIdleToWalk180L.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleToRun90L = lMotionStateMachine.AddState("IdleToRun90L", new Vector3(456, 276, 0));
            lIdleToRun90L.motion = mRunFwdStart90_L;
            lIdleToRun90L.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleToRun180L = lMotionStateMachine.AddState("IdleToRun180L", new Vector3(456, 336, 0));
            lIdleToRun180L.motion = mRunFwdStart180_L;
            lIdleToRun180L.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleTurn90R = lMotionStateMachine.AddState("IdleTurn90R", new Vector3(288, 48, 0));
            lIdleTurn90R.motion = mTurnRt90_Loop;
            lIdleTurn90R.speed = 1.5f;

            UnityEditor.Animations.AnimatorState lIdleTurn180R = lMotionStateMachine.AddState("IdleTurn180R", new Vector3(192, 168, 0));
            lIdleTurn180R.motion = mTurnRt180;
            lIdleTurn180R.speed = 1.5f;

            UnityEditor.Animations.AnimatorState lIdleToWalk90R = lMotionStateMachine.AddState("IdleToWalk90R", new Vector3(-420, 396, 0));
            lIdleToWalk90R.motion = mWalkFwdStart90_R;
            lIdleToWalk90R.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleToWalk180R = lMotionStateMachine.AddState("IdleToWalk180R", new Vector3(-420, 456, 0));
            lIdleToWalk180R.motion = mWalkFwdStart180_R;
            lIdleToWalk180R.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleToRun90R = lMotionStateMachine.AddState("IdleToRun90R", new Vector3(456, 396, 0));
            lIdleToRun90R.motion = mRunFwdStart90_R;
            lIdleToRun90R.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleToRun180R = lMotionStateMachine.AddState("IdleToRun180R", new Vector3(456, 456, 0));
            lIdleToRun180R.motion = mRunFwdStart180_R;
            lIdleToRun180R.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdlePose = lMotionStateMachine.AddState("IdlePose", new Vector3(24, 372, 0));
            lIdlePose.motion = mIdle;
            lIdlePose.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkFwdLoop = lMotionStateMachine.AddState("WalkFwdLoop", new Vector3(-108, 588, 0));
            lWalkFwdLoop.motion = mWalkFwdLoop;
            lWalkFwdLoop.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunFwdLoop = lMotionStateMachine.AddState("RunFwdLoop", new Vector3(228, 588, 0));
            lRunFwdLoop.motion = mRunFwdLoop;
            lRunFwdLoop.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunPivot180L_RDown = lMotionStateMachine.AddState("RunPivot180L_RDown", new Vector3(36, 732, 0));
            lRunPivot180L_RDown.motion = mRunFwdTurn180_L_LU;
            lRunPivot180L_RDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunPivot180R_LDown = lMotionStateMachine.AddState("RunPivot180R_LDown", new Vector3(576, 792, 0));
            lRunPivot180R_LDown.motion = mRunFwdTurn180_R_RU;
            lRunPivot180R_LDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkToIdle_RDown = lMotionStateMachine.AddState("WalkToIdle_RDown", new Vector3(-528, 636, 0));
            lWalkToIdle_RDown.motion = mWalkFwdStop_LU;
            lWalkToIdle_RDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lWalkToIdle_LDown = lMotionStateMachine.AddState("WalkToIdle_LDown", new Vector3(-564, 588, 0));
            lWalkToIdle_LDown.motion = mWalkFwdStop_RU;
            lWalkToIdle_LDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunStop_RDown = lMotionStateMachine.AddState("RunStop_RDown", new Vector3(624, 672, 0));
            lRunStop_RDown.motion = mRunFwdStop_LU;
            lRunStop_RDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunStop_LDown = lMotionStateMachine.AddState("RunStop_LDown", new Vector3(588, 588, 0));
            lRunStop_LDown.motion = mRunFwdStop_RU;
            lRunStop_LDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunPivot180L_LDown = lMotionStateMachine.AddState("RunPivot180L_LDown", new Vector3(120, 792, 0));
            lRunPivot180L_LDown.motion = mRunFwdTurn180_L_RU;
            lRunPivot180L_LDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lRunPivot180R_RDown = lMotionStateMachine.AddState("RunPivot180R_RDown", new Vector3(348, 792, 0));
            lRunPivot180R_RDown.motion = mRunFwdTurn180_R_LU;
            lRunPivot180R_RDown.speed = 1f;

            UnityEditor.Animations.AnimatorState lIdleTurn20R = lMotionStateMachine.AddState("IdleTurn20R", new Vector3(180, -48, 0));
            lIdleTurn20R.motion = mTurnRt90_Loop;
            lIdleTurn20R.speed = 1.1f;

            UnityEditor.Animations.AnimatorState lIdleTurn20L = lMotionStateMachine.AddState("IdleTurn20L", new Vector3(-120, -48, 0));
            lIdleTurn20L.motion = mTurnLt90_Loop;
            lIdleTurn20L.speed = 1.1f;

            UnityEditor.Animations.AnimatorState lWalkPivot180_L = lMotionStateMachine.AddState("WalkPivot180_L", new Vector3(-264, 732, 0));
            lWalkPivot180_L.motion = mWalkFwdStart180_L;
            lWalkPivot180_L.speed = 1f;

            UnityEditor.Animations.AnimatorStateTransition lAnyStateTransition = null;

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn90L);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.05f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, PHASE_START, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -60f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -160f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn20L);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.05f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, PHASE_START, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -10f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn20R);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.05f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, PHASE_START, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 10f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn90R);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.05f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, PHASE_START, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 60f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 160f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn180R);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.05f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, PHASE_START, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleToRun);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.05f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, PHASE_START, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -10f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 10f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lWalkFwdLoop);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, PHASE_START_SHORTCUT_WALK, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lRunFwdLoop);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0.9f;
            lAnyStateTransition.duration = 0.1f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, PHASE_START_SHORTCUT_RUN, "L0MotionPhase");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleToWalk);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.05f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, PHASE_START, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -10f, "InputAngleFromAvatar");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 10f, "InputAngleFromAvatar");

            // Create the transition from the any state. Note that 'AnyState' transitions have to be added to the root
            lAnyStateTransition = lRootStateMachine.AddAnyStateTransition(lIdleTurn180R);
            lAnyStateTransition.hasExitTime = false;
            lAnyStateTransition.hasFixedDuration = true;
            lAnyStateTransition.exitTime = 0f;
            lAnyStateTransition.duration = 0.05f;
            lAnyStateTransition.offset = 0f;
            lAnyStateTransition.mute = false;
            lAnyStateTransition.solo = false;
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, PHASE_START, "L0MotionPhase");
            lAnyStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");

            UnityEditor.Animations.AnimatorStateTransition lStateTransition = null;

            lStateTransition = lIdleToWalk.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToWalk.AddTransition(lWalkToIdle_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToWalk.AddTransition(lWalkToIdle_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.4614975f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.1621253f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToRun.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun.AddTransition(lRunStop_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.4f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleTurn90L.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleTurn90L.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn90L.AddTransition(lIdleToWalk90L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.2f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.1354761f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn90L.AddTransition(lIdleToRun90L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.2f;
            lStateTransition.duration = 0.09999999f;
            lStateTransition.offset = 0.1202882f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn90L.AddTransition(lIdleToRun);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn180L.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleTurn180L.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn180L.AddTransition(lIdleToWalk180L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.2f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0.07468655f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn180L.AddTransition(lIdleToRun180L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.2f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0.09689496f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn180L.AddTransition(lIdleToRun);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            lStateTransition = lIdleToWalk90L.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToWalk90L.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.15f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToWalk180L.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToWalk180L.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.15f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToRun90L.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun90L.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun180L.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun180L.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleTurn90R.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleTurn90R.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn90R.AddTransition(lIdleToWalk90R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.2f;
            lStateTransition.duration = 0.09999999f;
            lStateTransition.offset = 0.1567315f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn90R.AddTransition(lIdleToRun90R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.2f;
            lStateTransition.duration = 0.09999999f;
            lStateTransition.offset = 0.09090913f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn90R.AddTransition(lIdleToRun);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn180R.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleTurn180R.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn180R.AddTransition(lIdleToWalk180R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.2f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0.1132088f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn180R.AddTransition(lIdleToRun180R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.2f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0.0738495f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn180R.AddTransition(lIdleToRun);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            lStateTransition = lIdleToWalk90R.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToWalk90R.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.15f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToWalk180R.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToWalk180R.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.15f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleToRun90R.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun90R.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun180R.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lIdleToRun180R.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.9f, "InputMagnitude");

            lStateTransition = lIdlePose.AddTransition(lIdleToWalk180R);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToWalk90R);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToWalk180L);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToWalk90L);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToRun);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToRun90L);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToRun180L);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToRun90R);
            lStateTransition.hasExitTime = false;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 160f, "InputAngleFromAvatar");

            lStateTransition = lIdlePose.AddTransition(lIdleToRun180R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 160f, "InputAngleFromAvatar");

            lStateTransition = lWalkFwdLoop.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0.2510554f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkToIdle_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5039032f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkToIdle_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lWalkFwdLoop.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.6f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0.6110921f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lWalkFwdLoop.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0.8995329f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 1f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.9f, "InputMagnitude");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkPivot180_L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkPivot180_L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.05f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkPivot180_L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.95f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkPivot180_L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkPivot180_L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.05f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkPivot180_L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.95f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkToIdle_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.25f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.1131489f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lWalkFwdLoop.AddTransition(lWalkToIdle_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.75f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.1338524f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0.3139983f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.4f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.1f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.03113134f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.5f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lRunPivot180L_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.53f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lRunFwdLoop.AddTransition(lRunPivot180R_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lRunFwdLoop.AddTransition(lRunPivot180L_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lRunFwdLoop.AddTransition(lRunPivot180R_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.53f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lRunFwdLoop.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.6f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0.6967632f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.4f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.9f;
            lStateTransition.duration = 0.2f;
            lStateTransition.offset = 0.9236227f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Equals, 0f, "L0MotionParameter");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.4f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lRunStop_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.55f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.01f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.5f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lRunStop_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.8f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.2348485f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.5f, "InputMagnitude");

            lStateTransition = lRunFwdLoop.AddTransition(lRunStop_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.3142294f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.181153f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.5f, "InputMagnitude");

            lStateTransition = lRunPivot180L_RDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lRunPivot180R_LDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lWalkToIdle_RDown.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lWalkToIdle_RDown.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.5057101f;
            lStateTransition.duration = 0.04744329f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_RDown.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -140f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 140f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_RDown.AddTransition(lWalkPivot180_L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.65f;
            lStateTransition.duration = 0.05f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_RDown.AddTransition(lWalkPivot180_L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.65f;
            lStateTransition.duration = 0.05000001f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_RDown.AddTransition(lIdleToWalk180R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_RDown.AddTransition(lIdleToWalk180R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_LDown.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lWalkToIdle_LDown.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.5250819f;
            lStateTransition.duration = 0.04743088f;
            lStateTransition.offset = 0.5119725f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_LDown.AddTransition(lWalkPivot180_L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.65f;
            lStateTransition.duration = 0.05f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_LDown.AddTransition(lWalkPivot180_L);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.65f;
            lStateTransition.duration = 0.05000007f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_LDown.AddTransition(lIdleToWalk180R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_LDown.AddTransition(lIdleToWalk180R);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lWalkToIdle_LDown.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -140f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 140f, "InputAngleFromAvatar");

            lStateTransition = lRunStop_RDown.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.8687333f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lRunStop_RDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.4475232f;
            lStateTransition.duration = 0.1973684f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            lStateTransition = lRunStop_RDown.AddTransition(lRunPivot180R_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3613918f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lRunStop_RDown.AddTransition(lRunPivot180R_LDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3613918f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lRunStop_LDown.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.7665359f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lRunStop_LDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.4860786f;
            lStateTransition.duration = 0.1217646f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, -60f, "InputAngleFromAvatar");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 60f, "InputAngleFromAvatar");

            lStateTransition = lRunStop_LDown.AddTransition(lRunPivot180R_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3564995f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.03969417f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, -140f, "InputAngleFromAvatar");

            lStateTransition = lRunStop_LDown.AddTransition(lRunPivot180R_RDown);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 0.3564995f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0.03969417f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.3f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 140f, "InputAngleFromAvatar");

            lStateTransition = lRunPivot180L_LDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lRunPivot180R_RDown.AddTransition(lRunFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

            lStateTransition = lIdleTurn20R.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.4f;
            lStateTransition.duration = 0.4f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleTurn20R.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn20R.AddTransition(lIdleToRun);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn20R.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn20R.AddTransition(lIdleToRun);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn20L.AddTransition(lIdlePose);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.4f;
            lStateTransition.duration = 0.4f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.1f, "InputMagnitude");

            lStateTransition = lIdleTurn20L.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn20L.AddTransition(lIdleToRun);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = false;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn20L.AddTransition(lIdleToWalk);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.1f, "InputMagnitude");
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Less, 0.6f, "InputMagnitude");

            lStateTransition = lIdleTurn20L.AddTransition(lIdleToRun);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 0.5f;
            lStateTransition.duration = 0.1f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;
            lStateTransition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.Greater, 0.6f, "InputMagnitude");

            lStateTransition = lWalkPivot180_L.AddTransition(lWalkFwdLoop);
            lStateTransition.hasExitTime = true;
            lStateTransition.hasFixedDuration = true;
            lStateTransition.exitTime = 1f;
            lStateTransition.duration = 0f;
            lStateTransition.offset = 0f;
            lStateTransition.mute = false;
            lStateTransition.solo = false;

        }

        /// <summary>
        /// Used to show the settings that allow us to generate the animator setup.
        /// </summary>
        public override void OnSettingsGUI()
        {
            UnityEditor.EditorGUILayout.IntField(new GUIContent("Phase ID", "Phase ID used to transition to the state."), PHASE_START);
            mWalkFwdStart = CreateAnimationField("IdleToWalk", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/WalkFwdStart.anim", "WalkFwdStart", mWalkFwdStart);
            mRunFwdStart = CreateAnimationField("IdleToRun", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/RunFwdStart.anim", "RunFwdStart", mRunFwdStart);
            mTurnLt90_Loop = CreateAnimationField("IdleTurn90L", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/TurnLt90_Loop.anim", "TurnLt90_Loop", mTurnLt90_Loop);
            mTurnLt180 = CreateAnimationField("IdleTurn180L", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/TurnLt180.anim", "TurnLt180", mTurnLt180);
            mWalkFwdStart90_L = CreateAnimationField("IdleToWalk90L", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/WalkFwdStart90_L.anim", "WalkFwdStart90_L", mWalkFwdStart90_L);
            mWalkFwdStart180_L = CreateAnimationField("IdleToWalk180L", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/WalkFwdStart180_L.anim", "WalkFwdStart180_L", mWalkFwdStart180_L);
            mRunFwdStart90_L = CreateAnimationField("IdleToRun90L", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/RunFwdStart90_L.anim", "RunFwdStart90_L", mRunFwdStart90_L);
            mRunFwdStart180_L = CreateAnimationField("IdleToRun180L", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/RunFwdStart180_L.anim", "RunFwdStart180_L", mRunFwdStart180_L);
            mTurnRt90_Loop = CreateAnimationField("IdleTurn90R", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/TurnRt90_Loop.anim", "TurnRt90_Loop", mTurnRt90_Loop);
            mTurnRt180 = CreateAnimationField("IdleTurn180R", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/TurnRt180.anim", "TurnRt180", mTurnRt180);
            mWalkFwdStart90_R = CreateAnimationField("IdleToWalk90R", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/WalkFwdStart90_R.anim", "WalkFwdStart90_R", mWalkFwdStart90_R);
            mWalkFwdStart180_R = CreateAnimationField("IdleToWalk180R", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/WalkFwdStart180_R.anim", "WalkFwdStart180_R", mWalkFwdStart180_R);
            mRunFwdStart90_R = CreateAnimationField("IdleToRun90R", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/RunFwdStart90_R.anim", "RunFwdStart90_R", mRunFwdStart90_R);
            mRunFwdStart180_R = CreateAnimationField("IdleToRun180R", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/RunFwdStart180_R.anim", "RunFwdStart180_R", mRunFwdStart180_R);
            mIdle = CreateAnimationField("IdlePose", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/Idle.anim", "Idle", mIdle);
            mWalkFwdLoop = CreateAnimationField("WalkFwdLoop", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/WalkFwdLoop.anim", "WalkFwdLoop", mWalkFwdLoop);
            mRunFwdLoop = CreateAnimationField("RunFwdLoop", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/RunFwdLoop.anim", "RunFwdLoop", mRunFwdLoop);
            mRunFwdTurn180_L_LU = CreateAnimationField("RunPivot180L_RDown", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/RunFwdTurn180_L_LU.anim", "RunFwdTurn180_L_LU", mRunFwdTurn180_L_LU);
            mRunFwdTurn180_R_RU = CreateAnimationField("RunPivot180R_LDown", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/RunFwdTurn180_R_RU.anim", "RunFwdTurn180_R_RU", mRunFwdTurn180_R_RU);
            mWalkFwdStop_LU = CreateAnimationField("WalkToIdle_RDown", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/WalkFwdStop_LU.anim", "WalkFwdStop_LU", mWalkFwdStop_LU);
            mWalkFwdStop_RU = CreateAnimationField("WalkToIdle_LDown", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/WalkFwdStop_RU.anim", "WalkFwdStop_RU", mWalkFwdStop_RU);
            mRunFwdStop_LU = CreateAnimationField("RunStop_RDown", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/RunFwdStop_LU.anim", "RunFwdStop_LU", mRunFwdStop_LU);
            mRunFwdStop_RU = CreateAnimationField("RunStop_LDown", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/RunFwdStop_RU.anim", "RunFwdStop_RU", mRunFwdStop_RU);
            mRunFwdTurn180_L_RU = CreateAnimationField("RunPivot180L_LDown", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/RunFwdTurn180_L_RU.anim", "RunFwdTurn180_L_RU", mRunFwdTurn180_L_RU);
            mRunFwdTurn180_R_LU = CreateAnimationField("RunPivot180R_RDown", "Assets/ThirdParty/MovementAnimsetPro/Animations/MovementAnimsetPro.fbx/RunFwdTurn180_R_LU.anim", "RunFwdTurn180_R_LU", mRunFwdTurn180_R_LU);

            // Add the remaining functionality
            base.OnSettingsGUI();
        }

#endif

        // ************************************ END AUTO GENERATED ************************************
        #endregion
    }
}

using System;
using com.ootii.Input;
using UnityEngine;

namespace DungeonStrike
{
    public class EmptyInputSource : MonoBehaviour, IInputSource
    {
        float IInputSource.InputFromAvatarAngle
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        float IInputSource.InputFromCameraAngle
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        bool IInputSource.IsEnabled
        {
            get
            {
                return false;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        bool IInputSource.IsViewingActivated
        {
            get
            {
                return false;
            }
        }

        float IInputSource.MovementSqr
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        float IInputSource.MovementX
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        float IInputSource.MovementY
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        float IInputSource.ViewX
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        float IInputSource.ViewY
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        float IInputSource.GetValue(string rAction)
        {
            throw new NotImplementedException();
        }

        float IInputSource.GetValue(int rKey)
        {
            throw new NotImplementedException();
        }

        bool IInputSource.IsJustPressed(string rAction)
        {
            return false;
        }

        bool IInputSource.IsJustPressed(int rKey)
        {
            return false;
        }

        bool IInputSource.IsJustPressed(KeyCode rKey)
        {
            return false;
        }

        bool IInputSource.IsJustReleased(string rAction)
        {
            return false;
        }

        bool IInputSource.IsJustReleased(int rKey)
        {
            return false;
        }

        bool IInputSource.IsJustReleased(KeyCode rKey)
        {
            return false;
        }

        bool IInputSource.IsPressed(int rKey)
        {
            return false;
        }

        bool IInputSource.IsPressed(string rAction)
        {
            return false;
        }

        bool IInputSource.IsPressed(KeyCode rKey)
        {
            return false;
        }

        bool IInputSource.IsReleased(int rKey)
        {
            return false;
        }

        bool IInputSource.IsReleased(string rAction)
        {
            return false;
        }

        bool IInputSource.IsReleased(KeyCode rKey)
        {
            return false;
        }
    }
}
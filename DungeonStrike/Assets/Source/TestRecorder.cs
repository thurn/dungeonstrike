using UnityEngine;

namespace DungeonStrike
{
    public class TestRecorder : MonoBehaviour
    {
        private bool _recording;
        private int _numUpdates;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _recording = !_recording;

                if (_recording)
                {
                    Debug.Log("Started Recording");
                    //Application.targetFrameRate = 5;
                }
            }
        }

        void FixedUpdate()
        {
            if (_recording)
            {
                _numUpdates++;
                if (_numUpdates % 50 == 0)
                {
                    Debug.Log("Update " + _numUpdates + " time " + Time.deltaTime);
                    Application.CaptureScreenshot("Screenshot" + _numUpdates + ".png");
                }
            }
        }
    }
}
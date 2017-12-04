using UnityEngine;
using UnityEngine.VR;
using System;
using System.Collections.Generic;
using System.Linq;
using VRTK;

namespace headmotion
{
    struct hdm
    {
        public float timestamp;
        public Quaternion orientation;
        public Vector3 eulerAngles;

        public hdm(float timestamp, Quaternion orientation)
        {
            this.timestamp = timestamp;
            this.orientation = orientation;

            eulerAngles = orientation.eulerAngles;
            eulerAngles.x = detection.WrapAngle(eulerAngles.x);
            eulerAngles.y = detection.WrapAngle(eulerAngles.y);
        }
    }

    public class VRGesture : MonoBehaviour
    {
        [SerializeField]
        float recognitionInterval = 0.5f;

        private Camera _cam;

        public event Action YesHandler;
        public event Action<int, float> ShakeHandler;

        LinkedList<hdm> hdms = new LinkedList<hdm>();
        float waitTime = 0f;
        
        protected void Awake() {
            VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
        }

        protected virtual void OnDestroy() {
            VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
        }

        protected virtual void OnEnable() {
            _cam = VRTK_SDKManager.instance.loadedSetup.headsetSDK.GetHeadsetCamera().GetComponent<Camera>();
        }

        void Update()
        {
            // Record orientation
//            Quaternion q = InputTracking.GetLocalRotation(VRNode.Head);
//            Quaternion q = InputTracking.GetLocalRotation(VRNode.Head);
            Quaternion q = _cam.transform.rotation;

            hdms.AddLast(new hdm(Time.time, q));
            if (hdms.Count >= 120)
            {
                hdms.RemoveFirst();
            }
            if (waitTime > 0)
            {
                waitTime -= Time.deltaTime;
            }
            else
            {
                RecognizeYes();
                RecognizeShake();
            }
        }

        public void GetGraphEntries(out float[] timestamps, out Quaternion[] orientations)
        {
            int size = hdms.Count;
            timestamps = new float[size];
            orientations = new Quaternion[size];

            int index = 0;
            foreach (var hdm in hdms)
            {
                timestamps[index] = hdm.timestamp;
                orientations[index] = hdm.orientation;
                index++;
            }
        }

        IEnumerable<hdm> Range(float startTime, float endTime)
        {
            return hdms.Where(hdm => (hdm.timestamp < Time.time - startTime &&
                                            hdm.timestamp >= Time.time - endTime));
        }

        void RecognizeYes()
        {
            try
            {
                float basePos = Range(0.2f, 0.4f).Average(hdm => hdm.eulerAngles.x);
                float xMax = Range(0.01f, 0.2f).Max(hdm => hdm.eulerAngles.x);
                float current = hdms.First().eulerAngles.x;

                if (xMax - basePos > 10f &&
                    Mathf.Abs(current - basePos) < 5f)
                {
                    if (YesHandler != null) { YesHandler.Invoke(); }
                    waitTime = recognitionInterval;
                    hdms.Clear();
                }
            }
            catch (InvalidOperationException)
            {
               //pass
            }
        }

        void RecognizeShake() 
        {
            try {
                var nextType = 0; //0: unknown, 1: pos, 2: neg
                var diffSum = 0.0f;
                var shakeCount = 0;
                var shakeDuration = 0.0f;
                const float minShake = 10.0f;
                
                var beforeY = hdms.First().eulerAngles.y;
                var beforeShakeTime = float.NaN; 
                foreach (var hdm in hdms) {
                    diffSum += GetAngleDiff(beforeY, hdm.eulerAngles.y);
                    if (nextType == 0) {
                        if (diffSum > minShake || diffSum < -minShake) {
                            if (diffSum > minShake) {
                                nextType = 2;
                            }
                            else if (diffSum < -minShake) {
                                nextType = 1;
                            }
                            shakeCount += 1;
                            beforeShakeTime = hdm.timestamp;
                        }
                    }
                    else if (nextType == 1) {
                        if (diffSum > minShake) {
                            nextType = 2;
                            shakeCount += 1;
                            shakeDuration = hdm.timestamp - beforeShakeTime;
                        }
                    }
                    else if (nextType == 2) {
                        if (diffSum < -minShake) {
                            nextType = 1;
                            shakeCount += 1;
                            shakeDuration = hdm.timestamp - beforeShakeTime;
                        }
                    }
                }
                if (shakeCount > 2)
                {
                    if (ShakeHandler != null) { ShakeHandler.Invoke(shakeCount, shakeDuration / shakeCount); }
                    waitTime = recognitionInterval;
                    hdms.Clear();
                }
            }
            catch (InvalidOperationException)
            {
                // pass
            }
        }

        private static float GetAngleDiff(float from, float to) {
            var d1 = to - from + 360;
            var d2 = to - from;
            return Math.Abs(d1) > Math.Abs(d2) ? d2 : d1;
        }
    }
}


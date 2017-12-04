using UnityEngine;
using UnityEngine.VR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private enum Gesture {
            Yes,
            No,
            Shake
        }
        private Dictionary<Gesture, float> recogInterval = new Dictionary <Gesture, float>() {
            {Gesture.Yes, 0.0f},
            {Gesture.No, 0.0f},
            {Gesture.Shake, 0.0f}
        };
        private Dictionary<Gesture, long> recogIndex = new Dictionary <Gesture, long>() {
            {Gesture.Yes, 120},
            {Gesture.No, 120},
            {Gesture.Shake, 120}
        };

        private float gestureInterval = 0.5f;
        private Camera _cam;

        public event Action YesHandler;
        public event Action NoHandler;
        public event Action<float> ShakeHandler;

        LinkedList<hdm> hdms = new LinkedList<hdm>();
        
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
            Quaternion q = _cam.transform.rotation;

            hdms.AddLast(new hdm(Time.time, q));
            if (hdms.Count >= 120)
            {
                hdms.RemoveFirst();
            }
            foreach (Gesture key in Enum.GetValues(typeof(Gesture))) {
                var val = recogInterval[key];
                if (val > 0) {
                    val -= Time.deltaTime;
                    if (val < 0) {
                        val = 0.0f;
                    }
                    recogInterval[key] = val;
                }
                recogIndex[key] = Math.Max(recogIndex[key] - 1, 0);
            }
            RecognizeYes();
            RecognizeNo();
            RecognizeShake();
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

        void RecognizeYes()
        {
            if (recogInterval[Gesture.Yes] > 0) {
                return;
            }
            try
            {
                var diffSum = 0.0f;
                var didYes = false;
                const float minDiff = 40.0f;
                
                var beforeX = hdms.First().eulerAngles.x;
                var index = 0;
                foreach (var hdm in hdms) {
                    if (index < recogIndex[Gesture.Yes]) {
                        continue;
                    }
                    index++;
                    diffSum += GetAngleDiff(beforeX, hdm.eulerAngles.x);
                    if (diffSum > minDiff) {
                        didYes = true;
                    }
                    beforeX = hdm.eulerAngles.x;
                }
                if (didYes && Math.Abs(diffSum) < 5f)
                {
                    if (YesHandler != null) { YesHandler.Invoke(); }
                    recogInterval[Gesture.Yes] = gestureInterval;
                    recogIndex[Gesture.Yes] = hdms.Count;
                }
            }
            catch (InvalidOperationException)
            {
               //pass
            }
        }

        void RecognizeNo() 
        {
            if (recogInterval[Gesture.No] > 0) {
                return;
            }
            try {
                var nextType = 0; //0: unknown, 1: pos, 2: neg
                var diffSum = 0.0f;
                var shakeCount = 0;
                var shakeDuration = 0.0f;
                const float minShake = 40.0f;
                
                var beforeY = hdms.First().eulerAngles.y;
                var beforeShakeTime = float.NaN; 
                var index = 0;
                foreach (var hdm in hdms) {
                    if (index < recogIndex[Gesture.No]) {
                        continue;
                    }
                    index++;
                    diffSum += GetAngleDiff(beforeY, hdm.eulerAngles.y);
                    beforeY = hdm.eulerAngles.y;
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
                if (shakeCount >= 2) {
                    if (NoHandler != null) { NoHandler.Invoke(); }
                    recogInterval[Gesture.No] = gestureInterval;
                    recogIndex[Gesture.No] = hdms.Count;
                }
            }
            catch (InvalidOperationException)
            {
                // pass
            }
        }

        void RecognizeShake() 
        {
            if (recogInterval[Gesture.Shake] > 0) {
                return;
            }
            try {
                var nextType = 0; //0: unknown, 1: pos, 2: neg
                var diffSum = 0.0f;
                var shakeCount = 0;
                var shakeDuration = 0.0f;
                const float minShake = 20.0f;
                
                var beforeY = hdms.First().eulerAngles.y;
                var beforeShakeTime = float.NaN;
                var index = 0;
                foreach (var hdm in hdms) {
                    if (index < recogIndex[Gesture.Shake]) {
                        continue;
                    }
                    index++;
                    diffSum += GetAngleDiff(beforeY, hdm.eulerAngles.y);
                    beforeY = hdm.eulerAngles.y;
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
                if (shakeCount >= 4)
                {
                    Debug.LogFormat("Shake! {0}, {1}", shakeCount, shakeDuration / shakeCount);
                    if (ShakeHandler != null) { ShakeHandler.Invoke(shakeDuration / shakeCount); }
                    recogInterval[Gesture.Shake] = gestureInterval;
                    recogIndex[Gesture.Shake] = hdms.Count;
                }
            }
            catch (InvalidOperationException)
            {
                // pass
            }
        }

        private static float GetAngleDiff(float from, float to) {
            var minus = from > to;
            var d1 = (to - from);
            var d2 = d1 + 360 * (minus ? 1: -1);
            return Math.Abs(d1) > Math.Abs(d2) ? d2 : d1;
        }
    }
}


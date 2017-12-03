using UnityEngine;
using UnityEngine.VR;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public event Action NodHandler;
        public event Action HeadshakeHandler;

        LinkedList<hdm> hdms = new LinkedList<hdm>();
        float waitTime = 0f;

        void Update()
        {
            // Record orientation
            Quaternion q = InputTracking.GetLocalRotation(VRNode.Head);

            hdms.AddFirst(new hdm(Time.time, q));
            if (hdms.Count >= 120)
            {
                hdms.RemoveLast();
            }
            if (waitTime > 0)
            {
                waitTime -= Time.deltaTime;
            }
            else
            {
                RecognizeNod();
                RecognizeHeadshake();
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

        void RecognizeNod()
        {
            try
            {
                float basePos = Range(0.2f, 0.4f).Average(hdm => hdm.eulerAngles.x);
                float xMax = Range(0.01f, 0.2f).Max(hdm => hdm.eulerAngles.x);
                float current = hdms.First().eulerAngles.x;

                if (xMax - basePos > 10f &&
                    Mathf.Abs(current - basePos) < 5f)
                {
                    if (NodHandler != null) { NodHandler.Invoke(); }
                    waitTime = recognitionInterval;
                }
            }
            catch (InvalidOperationException)
            {
               //pass
            }
        }

        void RecognizeHeadshake()
        {
            try
            {
                float basePos = Range(0.2f, 0.4f).Average(hdm => hdm.eulerAngles.y);
                float yMax = Range(0.01f, 0.2f).Max(hdm => hdm.eulerAngles.y);
                float yMin = Range(0.01f, 0.2f).Min(hdm => hdm.eulerAngles.y);
                float current = hdms.First().eulerAngles.y;

                if ((yMax - basePos > 10f || basePos - yMin > 10f) &&
                    Mathf.Abs(current - basePos) < 5f)
                {
                    if (HeadshakeHandler != null) { HeadshakeHandler.Invoke(); }
                    waitTime = recognitionInterval;
                }
            }
            catch (InvalidOperationException)
            {
                // pass
            }
        }
    }
}


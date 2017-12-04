using UnityEngine;
using headmotion;

namespace headmotion
{
    public class YesNo : MonoBehaviour
    {
        [SerializeField]
        VRGesture vrGesture;

        void Start()
        {
            vrGesture.YesHandler += OnYes;
            vrGesture.ShakeHandler += OnShake;
        }

        void OnYes()
        {
            Debug.Log("YES!!");
            //INSERT HANDLER FOR YES MOTION

            gamebehavior.gesture = 1;
            gamebehavior.new_gesture = true;
        }

        void OnShake(int shakeCount, float timePerShake)
        {
            Debug.LogFormat("NO!! {0}, {1}",shakeCount, timePerShake);
            //INSERT HANDLER FOR NO MOTION
            gamebehavior.gesture = 2;
            gamebehavior.new_gesture = true;
        }
    }
}

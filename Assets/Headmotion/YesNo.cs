using UnityEngine;
using headmotion;
using Wakeup_mission;

namespace headmotion
{
    public class YesNo : MonoBehaviour
    {
        [SerializeField]
        VRGesture vrGesture;

        void Start()
        {
            vrGesture.YesHandler += OnYes;
            vrGesture.NoHandler += OnNo;
        }

        void OnYes()
        {
            Debug.Log("YES!!");
            GestureGame.gesture = 1;
            GestureGame.new_gesture = true;
        }

        void OnNo()
        {
            Debug.LogFormat("NO!!");
            GestureGame.gesture = 2;
            GestureGame.new_gesture = true;
        }
    }
}

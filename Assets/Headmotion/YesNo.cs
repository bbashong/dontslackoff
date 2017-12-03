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
            vrGesture.NodHandler += OnNod;
            vrGesture.HeadshakeHandler += OnHeadshake;
        }

        void OnNod()
        {
            print("YES!!");
            //INSERT HANDLER FOR YES MOTION
        }

        void OnHeadshake()
        {
            print("NO!!");
            //INSERT HANDLER FOR NO MOTION
        }
    }
}

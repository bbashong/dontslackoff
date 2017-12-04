// Write black pixels onto the GameObject that is located
// by the script. The script is attached to the _camera.
// Determine where the collider hits and modify the texture at that point.
//
// Note that the MeshCollider on the GameObject must have Convex turned off. This allows
// concave GameObjects to be included in collision in this example.
//
// Also to allow the texture to be updated by mouse button clicks it must have the Read/Write
// Enabled option set to true in its Advanced import settings.

using System;
using UnityEngine;
using System.Collections;
using VRTK;

namespace ReadingMission {
    public class CameraBrush : MonoBehaviour {
        public enum BrushType {
            Normal,
            Love,
            Game,
            Sleep
        }
        public Texture2D Brush;
        public Vector2 BrushSize = new Vector2(0.5f, 0.5f);
        public GameObject Target;
        private Camera _cam;
        private Texture2D _resizedBrush;

        protected void Awake() {
            VRTK_SDKManager.instance.AddBehaviourToToggleOnLoadedSetupChange(this);
        }

        protected virtual void OnDestroy() {
            VRTK_SDKManager.instance.RemoveBehaviourToToggleOnLoadedSetupChange(this);
        }

        protected virtual void OnEnable() {
            _cam = VRTK_SDKManager.instance.loadedSetup.headsetSDK.GetHeadsetCamera().GetComponent<Camera>();
        }

        protected void Start() {
            var pixRatioX = 
                Target.GetComponent<Renderer>().material.mainTexture.width / 
                Target.GetComponent<Collider>().bounds.size.x;
            var pixRatioY = 
                Target.GetComponent<Renderer>().material.mainTexture.height / 
                Target.GetComponent<Collider>().bounds.size.z;
            _resizedBrush = ScaleTexture(
                Brush,
                Mathf.RoundToInt(BrushSize.x * pixRatioX),
                Mathf.RoundToInt(BrushSize.y * pixRatioY));
        }

        protected void Update() {
            if (_cam == null) {
                return;
            }
            RaycastHit hit;
            Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit); 
            var centerTextCoord = new Vector2(float.NaN, float.NaN);
            if (Physics.Raycast(_cam.transform.position, _cam.transform.forward, out hit) 
                && string.Equals(hit.transform.gameObject.name, Target.name)) {
                centerTextCoord = hit.textureCoord;
            }
            Target.GetComponent<CameraCanvas>().Brush(centerTextCoord, _resizedBrush);
        }
    
        private Texture2D ScaleTexture(Texture2D source,int targetWidth,int targetHeight) {
            var result = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32 ,true);
            var rpixels = result.GetPixels(0);
            var incX = (1.0f / (float)targetWidth);
            var incY = (1.0f / (float)targetHeight); 
                for (var px = 0; px < rpixels.Length; ++px) { 
                rpixels[px] = source.GetPixelBilinear(
                    incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth))); 
            } 
            result.SetPixels(rpixels,0); 
            result.Apply(); 
            return result; 
        }

        public void SetBrushType(BrushType type) {
            
        }
    }
}

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
using System.Timers;
using UnityEngine.Networking;
using VRTK;

namespace ReadingMission {
    public class CameraBrush : MonoBehaviour {
        public enum BrushType {
            Normal,
            Love,
            Game,
            Sleep
        }
        public Texture2D BrushNormal;
        public Texture2D BrushLove;
        public Texture2D BrushGame;
        public Vector2 BrushSize = new Vector2(0.5f, 0.5f);
        public GameObject Target;
        private Texture2D _brush;
        private Camera _cam;
        private Texture2D _resizedBrush;
        private BrushType _brushType;
        private float _elapsedTime = 0.0f;
        private float _nextAngleChange = 0.0f;
        private Vector2 _angleDiff = new Vector2(0.0f, 0.0f);
        private Vector2 _brushSizeScale = new Vector2(1.0f, 1.0f);

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
            _brushType = BrushType.Normal;
            _brush = BrushNormal;
            ResizeBrush();
        }

        protected void Update() {
            if (_cam == null) {
                return;
            }
            RaycastHit hit;
            var centerTextCoord = new Vector2(float.NaN, float.NaN);
            var direction = _cam.transform.forward;
            Debug.LogFormat("{0}, {1}", direction, _angleDiff);
            if (_brushType == BrushType.Game) {
                direction += _angleDiff.x * _cam.transform.right;
                direction += _angleDiff.y * _cam.transform.up;
            }
            Debug.LogFormat("{0}", direction);
            if (Physics.Raycast(_cam.transform.position, direction, out hit) 
                && string.Equals(hit.transform.gameObject.name, Target.name)) {
                centerTextCoord = hit.textureCoord;
            }
            Target.GetComponent<CameraCanvas>().Brush(centerTextCoord, _resizedBrush);

            if (_brushType == BrushType.Game) {
                _nextAngleChange += Time.deltaTime;
                if (_nextAngleChange > 0.3f) {
                    _angleDiff = new Vector2(UnityEngine.Random.Range(0.0f, 0.2f) - 0.1f, UnityEngine.Random.Range(0.0f, 0.2f) - 0.1f);
                    _nextAngleChange = 0.0f;
                }
            }
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

        private void ResizeBrush() {
            var pixRatioX = 
                Target.GetComponent<Renderer>().material.mainTexture.width / 
                Target.GetComponent<Collider>().bounds.size.x;
            var pixRatioY = 
                Target.GetComponent<Renderer>().material.mainTexture.height / 
                Target.GetComponent<Collider>().bounds.size.z;
            _resizedBrush = ScaleTexture(
                _brush,
                Mathf.RoundToInt(BrushSize.x * _brushSizeScale.x * pixRatioX),
                Mathf.RoundToInt(BrushSize.y * _brushSizeScale.y * pixRatioY));
        }

        public void SetBrushType(BrushType type) {
            if (_brushType == type) {
                return;
            }
            _brushType = type;
            _angleDiff = new Vector2(0.0f, 0.0f);
            _brushSizeScale = new Vector2(1.0f, 1.0f);
            if (_brushType == BrushType.Normal) {
                _brush = BrushNormal;
            }
            else if (_brushType == BrushType.Love) {
                _brush = BrushLove;
                _brushSizeScale = new Vector2(0.3f, 0.3f);
            }
            else if (_brushType == BrushType.Game) {
                _brush = BrushGame;
            }
            ResizeBrush();
        }
    }
}

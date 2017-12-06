using UnityEngine;

namespace ReadingMission {
	public class CameraCanvas : MonoBehaviour {

		private Texture2D _drawTex;
		private Texture2D _brushTex;
		private long _paintedPixel;
		private long _totalPixel;
		private Color[] _clearColors;
	
		private void Start () {
			var renderTex = GetComponent<Renderer>().material.mainTexture as Texture2D;
			Debug.Assert(renderTex != null);
			_drawTex = new Texture2D (renderTex.width, renderTex.height, TextureFormat.RGBA32, true);
			for (var x = 0; x < renderTex.width; ++x) {
				for (var y = 0; y < renderTex.height; ++y) {
					_drawTex.SetPixel(x, y, new Color(0, 0, 0, 0));
				}
			}
			_drawTex.Apply();
			GetComponent<Renderer>().material.SetTexture("_DrawTex", _drawTex);
			
			_brushTex = new Texture2D (renderTex.width, renderTex.height, TextureFormat.RGBA32, true);
			for (var x = 0; x < renderTex.width; ++x) {
				for (var y = 0; y < renderTex.height; ++y) {
					_brushTex.SetPixel(x, y, new Color(0, 0, 0, 0));
				}
			}
			_brushTex.Apply();
			GetComponent<Renderer>().material.SetTexture("_BrushTex", _brushTex);

			_clearColors = _brushTex.GetPixels();	
			_totalPixel = renderTex.width * renderTex.height;
			_paintedPixel = 0;
			Debug.Assert(_brushTex != null);
			Debug.Assert(_clearColors != null);
		}
	
		private void Update () {
		
		}

		public float GetCompleteness() {
			return _paintedPixel / (float)_totalPixel;
		}

		public void Brush(Vector2 texturecoord, Texture2D brush) {
			texturecoord.x *= _drawTex.width;
			texturecoord.y *= _drawTex.height;
			
			_brushTex.SetPixels(_clearColors);
			if (float.IsNaN(texturecoord.x) || float.IsNaN(texturecoord.y)) {
                _brushTex.Apply();
				return;
			}
			for (var x = 0; x < brush.width; ++x) {
				for (var y = 0; y < brush.height; ++y) {
					var paintX = Mathf.RoundToInt(texturecoord.x + (x - brush.width * 0.5f));
					var paintY = Mathf.RoundToInt(texturecoord.y + (y - brush.height * 0.5f));
					if (paintX < 0 || paintX >= _drawTex.width || paintY < 0 || paintY >= _drawTex.height) {
						continue;
					}
					if (_drawTex.GetPixel(paintX, paintY).a <= 0.0f && brush.GetPixel(x, y).a >= 0.9f) {
						_drawTex.SetPixel(paintX, paintY, new Color(0, 0, 0, 1));
						_paintedPixel += 1;
					}
					if (brush.GetPixel(x, y).a > 0.0f) {
						_brushTex.SetPixel(paintX, paintY, brush.GetPixel(x, y));
					}
				}
			}
			_drawTex.Apply();
			_brushTex.Apply();
		}

		public void CopyCanvas(CameraCanvas other) {
			_paintedPixel = other._paintedPixel;
			Debug.Assert(_brushTex != null);
			Debug.Assert(_clearColors != null);
			_brushTex.SetPixels(_clearColors);
            _brushTex.Apply();
			_drawTex.SetPixels(other._drawTex.GetPixels());	
			_drawTex.Apply();
			GetComponent<Renderer>().material.mainTexture = other.GetComponent<Renderer>().material.mainTexture;
		}

		public void ClearCanvas(Texture2D newTexture) {
			_paintedPixel = 0;
			_brushTex.SetPixels(_clearColors);
            _brushTex.Apply();
			_drawTex.SetPixels(_clearColors);
			_drawTex.Apply();
			GetComponent<Renderer>().material.mainTexture = newTexture;
			GetComponent<Renderer>().material.SetTexture("_MainTex", GetComponent<Renderer>().material.mainTexture);
		}
	}
}

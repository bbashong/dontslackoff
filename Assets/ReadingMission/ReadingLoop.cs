using System;
using System.Timers;
using UnityEngine;
using UnityEngine.UI;

namespace ReadingMission {
	public class ReadingLoop : MonoBehaviour {
		public GameObject Paper;
		public GameObject PaperToRead;
		public GameObject PaperComplete;
		public GameObject PaperToReadStack;
		public GameObject PaperCompleteStack;
		public Text RemainTimeTxt;
		public Text RemainCountTxt;
		public Text PercentageTxt;
		public float PlayTime;
		public uint ReadCount;
		public Texture2D[] PaperTextures;

		private const string TimeFormat = "Remain Time: {0}";	
		private const string CountFormat = "Remain Count: {0}";	
		private const string PercentFormat = "{0}%";
		private const string PageTextureNameFormat = "page-texture-{0:2}";
		private const float ThicknessPerPaper = 0.01f;
		private const float OriginalPaperY = 1.49f;
		
		private Color32 _minColor = new Color32(252, 0, 88, 255);
		private Color32 _maxColor = new Color32(0, 252, 167, 255);
		private float _currentPlayTime;
		private uint _currentReadCount;
	
		private void Start () {
			_currentPlayTime = PlayTime;
			_currentReadCount = ReadCount;
			SetPlayTimeTxt();
			SetPercentageTxt(0);
			SetReadCountTxt();
			SetPaperStackThickness(PaperToReadStack, PaperToRead, _currentReadCount);
			SetPaperStackThickness(PaperCompleteStack, PaperComplete, ReadCount - _currentReadCount);
		}
	
		private void Update () {
			UpdateTimer();	
			UpdatePercentage();
		}

		private void UpdateTimer() {
			_currentPlayTime -= Time.deltaTime;
			if (_currentPlayTime <= 0) {
				_currentPlayTime = 0;
				// End Game
			}
			SetPlayTimeTxt();
		}

		private void UpdatePercentage() {
			var comp = Paper.GetComponent<CameraCanvas>().GetCompleteness();
			SetPercentageTxt(comp);
			if (comp > 0.95f) {
				// swap paper
				_currentReadCount -= 1;
				SetReadCountTxt();
                SetPaperStackThickness(PaperToReadStack, PaperToRead, _currentReadCount);
                SetPaperStackThickness(PaperCompleteStack, PaperComplete, ReadCount - _currentReadCount);
				
				Debug.Assert(PaperComplete.GetComponent<CameraCanvas>() != null);
				Debug.Assert(Paper.GetComponent<CameraCanvas>() != null);
				Debug.Assert(PaperToRead.GetComponent<CameraCanvas>() != null);
				PaperComplete.GetComponent<CameraCanvas>().CopyCanvas(Paper.GetComponent<CameraCanvas>());
				Paper.GetComponent<CameraCanvas>().CopyCanvas(PaperToRead.GetComponent<CameraCanvas>());
				PaperToRead.GetComponent<CameraCanvas>().ClearCanvas(PaperTextures[ReadCount % PaperTextures.Length]);
			}
		}

		private Color32 GetTxtColor(float ratio) {
			return new Color32(
				(byte) Math.Floor(_maxColor.r * ratio + _minColor.r * (1 - ratio)),
				(byte) Math.Floor(_maxColor.g * ratio + _minColor.g * (1 - ratio)),
				(byte) Math.Floor(_maxColor.b * ratio + _minColor.b * (1 - ratio)),
				(byte) Math.Floor(_maxColor.a * ratio + _minColor.a * (1 - ratio)));

		}

		private void SetPlayTimeTxt() {
			RemainTimeTxt.text = string.Format(
				TimeFormat, 
				new DateTime(new TimeSpan(0, 0, 0, 0, Mathf.RoundToInt(_currentPlayTime * 1000)).Ticks).ToString("mm:ss:ff")
            );
			RemainTimeTxt.color = GetTxtColor(_currentPlayTime / (float)PlayTime);
		}

		private void SetPercentageTxt(float completness) {
			PercentageTxt.text = string.Format(
				PercentFormat, 
				Mathf.RoundToInt(completness * 100)
            );
			PercentageTxt.color = GetTxtColor(completness);
		}

		private void SetReadCountTxt() {
			RemainCountTxt.text = string.Format(
				CountFormat, 
				_currentReadCount
            );
			RemainCountTxt.color = GetTxtColor(1 - _currentReadCount / (float)ReadCount);
		}

		private void SetPaperStackThickness(GameObject paperStack, GameObject topPaper, uint count) {
			if (count == 0) {
				paperStack.GetComponent<Renderer>().enabled = false;
				topPaper.GetComponent<Renderer>().enabled = false;
				return;
			}
            paperStack.GetComponent<Renderer>().enabled = true;
            topPaper.GetComponent<Renderer>().enabled = true;
			var beforeScale = paperStack.transform.localScale;
			paperStack.transform.localScale = new Vector3(beforeScale.x, count * ThicknessPerPaper, beforeScale.z);	
			var beforePos = paperStack.transform.localPosition;
			paperStack.transform.localPosition = new Vector3(beforePos.x, OriginalPaperY + 0.5f * count * ThicknessPerPaper, beforePos.z);

			beforePos = topPaper.transform.localPosition;
			topPaper.transform.localPosition = new Vector3(beforePos.x, OriginalPaperY + count * ThicknessPerPaper + 0.0001f, beforePos.z);
		}
		
	}
}

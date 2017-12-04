using System.Collections;
using ReadingMission;
using UnityEngine;
using UnityEngine.UI;

namespace Ending {
	public class Grading : MonoBehaviour {

		private float _scheduledTime;
		private float _remainTime;
		private Text _subtitle;
		private int _phase = 0;

		// Use this for initialization
		void Start () {
			_subtitle = GetComponent<Text>();
		 	_scheduledTime = 1.5f;
			Done();
		}
	
		// Update is called once per frame
		void Update () {
		
		}
		
		IEnumerator FadeInOut() {
			while (_remainTime < _scheduledTime) {
				_remainTime += 1 / 60.0f;
				_subtitle.GetComponent<CanvasRenderer>().SetAlpha(1.0f * _remainTime / _scheduledTime);
				yield return null;
			}
			yield return new WaitForSeconds(_scheduledTime);
			while (_remainTime > 0) {
				_remainTime -= 1 / 60.0f;
				_subtitle.GetComponent<CanvasRenderer>().SetAlpha(1.0f * _remainTime / _scheduledTime);
				yield return null;
			}
			Done();
		}
		
		IEnumerator FadeIn() {
			while (_remainTime < _scheduledTime) {
				_remainTime += 1 / 60.0f;
				_subtitle.GetComponent<CanvasRenderer>().SetAlpha(1.0f * _remainTime / _scheduledTime);
				yield return null;
			}
		}

		void Done() {
			if (_phase == 0) {
				_subtitle.text = "Taking Exam...";
				_phase++;
				StartCoroutine("FadeInOut");
			}
			else if (_phase == 1) {
				_subtitle.text = "TAs are making your grade...";
				_phase++;
				StartCoroutine("FadeInOut");
			}
			else if (_phase == 2) {
				_subtitle.text = "Your Grade is";
				_phase++;
				StartCoroutine("FadeInOut");
			}
			else {
                var finished = ReadingMissionResult.ReadingFinished;
                var count = ReadingMissionResult.RemainCount;
                var time = ReadingMissionResult.RemainTime;
				if (finished) {
					if (time >= 0.5f) {
                        _subtitle.text = "A+";
					}
					else if (time >= 0.25f) {
						_subtitle.text = "A0";
					}
					else {
						_subtitle.text = "A-";
					}
				}
				else {
					if (count <= 0.25f) {
						_subtitle.text = "B+";
					}
					else if (count <= 0.35f) {
						_subtitle.text = "B0";
					}
					else if (count <= 0.45f) {
						_subtitle.text = "B-";
					}
					else if (count <= 0.60f) {
						_subtitle.text = "C+";
					}
					else if (count <= 0.75f) {
						_subtitle.text = "C0";
					}
					else if (count <= 0.90f) {
						_subtitle.text = "C-";
					}
					else {
						_subtitle.text = "F";
					}
				}
				StartCoroutine("FadeIn");
			}
		}

	}
}

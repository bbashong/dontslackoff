using System;
using System.Collections;
using System.Timers;
using headmotion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Wakeup_mission;

namespace ReadingMission
{
    static class ReadingMissionResult
    {
        public static bool ReadingFinished;
        public static float RemainCount;
        public static float RemainTime;
    }

    public class ReadingLoop : MonoBehaviour
    {
        public GameObject Paper;
        public GameObject PaperToRead;
        public GameObject PaperComplete;
        public GameObject PaperToReadStack;
        public GameObject PaperCompleteStack;
        public GameObject DanceEventInstance;
        public GameObject NoiseEventInstance;
        public Image SleepBlocker;
        public Text RemainTimeTxt;
        public Text RemainCountTxt;
        public Text PercentageTxt;
        public Text SubTitleText;
        public AudioSource pageFlipSound;
        public float PlayTime;
        public uint ReadCount;
        public Texture2D[] PaperTextures;
        public float LoveDebuffStart;
        public float LoveDebuffEnd;
        public float GameDebuffStart;
        public float GameDebuffEnd;
        public float NoiseDebuffStart;
        public float NoiseDebuffEnd;
        public float DanceDebuffStart;
        public float DanceDebuffEnd;
        public float InitialFallingSleepTime;

        private const string TimeFormat = "Remain Time: {0}";
        private const string CountFormat = "Remain Count: {0}";
        private const string PercentFormat = "{0}%";
        private const float ThicknessPerPaper = 0.01f;
        private const float OriginalPaperY = 1.49f;

        private AudioSource clockSound;

        private enum DebuffType
        {
            None,
            Love,
            Game,
            Sleep,
            Noise,
            Dance
        }

        private Color32 _minColor = new Color32(252, 0, 88, 255);
        private Color32 _maxColor = new Color32(0, 252, 167, 255);
        private float _currentPlayTime;
        private uint _currentReadCount;
        private float _currentFallingSleepTime;
        private float _remainFallingSleepTime;
        private DebuffType _currentDebuffType;
        private CameraBrush _brush;
        private float _elapsedTime;
        private Color _originalSleepColor;


        private void Start()
        {
            _brush = GetComponent<CameraBrush>();
            _currentPlayTime = PlayTime;
            _currentReadCount = ReadCount;
            _currentDebuffType = DebuffType.None;
            _currentFallingSleepTime = InitialFallingSleepTime;
            _remainFallingSleepTime = _currentFallingSleepTime;
            SetPlayTimeTxt();
            SetPercentageTxt(0);
            SetReadCountTxt(false);
            SetPaperStackThickness(PaperToReadStack, PaperToRead, _currentReadCount);
            SetPaperStackThickness(PaperCompleteStack, PaperComplete, ReadCount - _currentReadCount);
            GetComponent<VRGesture>().ShakeHandler += OnShake;
            _elapsedTime = 0.0f;
            clockSound = GetComponent<AudioSource>();
            clockSound.volume = 0.0f;
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;
            UpdateTimer();
            UpdatePercentage();
            UpdateSound();
            if (_currentDebuffType == DebuffType.Sleep)
            {
                // pass	
            }
            else if (_elapsedTime > LoveDebuffStart && _elapsedTime < LoveDebuffEnd)
            {
                UpdateSleep();
                SetDebuffType(DebuffType.Love);
            }
            else if (_elapsedTime > GameDebuffStart && _elapsedTime < GameDebuffEnd)
            {
                UpdateSleep();
                SetDebuffType(DebuffType.Game);
            }
            else if (_elapsedTime > NoiseDebuffStart && _elapsedTime < NoiseDebuffEnd)
            {
                SetDebuffType(DebuffType.Noise);
            }
            else if (_elapsedTime >= NoiseDebuffEnd && _elapsedTime < DanceDebuffStart)
            {
                BackinSleep();
                UpdateSleep();
                SetDebuffType(DebuffType.None);
            }
            else if (_elapsedTime > DanceDebuffStart && _elapsedTime < DanceDebuffEnd)
            {
                SetDebuffType(DebuffType.Dance);
            }
            else if (_elapsedTime >= DanceDebuffEnd && _elapsedTime < DanceDebuffEnd + 10)
            {
                BackinSleep();
                UpdateSleep();
                SetDebuffType(DebuffType.None);
            }
            else
            {
                UpdateSleep();
                SetDebuffType(DebuffType.None);
            }
        }

        private void UpdateTimer()
        {
            _currentPlayTime -= Time.deltaTime;
            if (_currentPlayTime <= 0)
            {
                _currentPlayTime = 0;
                StartCoroutine("EndGame");
            }
            SetPlayTimeTxt();
        }

        private void UpdatePercentage()
        {
            var comp = Paper.GetComponent<CameraCanvas>().GetCompleteness();
            SetPercentageTxt(comp);
            if (comp > 0.95f)
            {
                // swap paper
                _currentReadCount -= 1;
                SetReadCountTxt(true);
                SetPaperStackThickness(PaperToReadStack, PaperToRead, _currentReadCount);
                SetPaperStackThickness(PaperCompleteStack, PaperComplete, ReadCount - _currentReadCount);
                PaperComplete.GetComponent<CameraCanvas>().CopyCanvas(Paper.GetComponent<CameraCanvas>());
                if (_currentReadCount == 0)
                {
                    Paper.GetComponent<Renderer>().enabled = false;
                    PaperToRead.GetComponent<Renderer>().enabled = false;
                    PaperToReadStack.GetComponent<Renderer>().enabled = false;
                    StartCoroutine("EndGame");
                    return;
                }
                Paper.GetComponent<CameraCanvas>().CopyCanvas(PaperToRead.GetComponent<CameraCanvas>());
                PaperToRead.GetComponent<CameraCanvas>().ClearCanvas(PaperTextures[ReadCount % PaperTextures.Length]);
                pageFlipSound.Play();
            }
        }

        private void UpdateSleep()
        {
            if (_remainFallingSleepTime <= 0)
            {
                return;
            }
            _remainFallingSleepTime -= Time.deltaTime;
            if (_remainFallingSleepTime <= 0)
            {
                OnSlept();
            }
            else
            {
                var color = SleepBlocker.color;
                color.a = 1 - (_remainFallingSleepTime / _currentFallingSleepTime);
                SleepBlocker.color = color;
            }
        }

        private void PauseSleep()
        {
            var color = SleepBlocker.color;
            _originalSleepColor = SleepBlocker.color;
            color.a = 0;
            SleepBlocker.color = color;
        }

        private void BackinSleep()
        {
            SleepBlocker.color = _originalSleepColor;
        }

        private void UpdateSound()
        {
            clockSound.volume = _elapsedTime / PlayTime;
        }


        private Color32 GetTxtColor(float ratio)
        {
            return new Color32(
                (byte)Math.Floor(_maxColor.r * ratio + _minColor.r * (1 - ratio)),
                (byte)Math.Floor(_maxColor.g * ratio + _minColor.g * (1 - ratio)),
                (byte)Math.Floor(_maxColor.b * ratio + _minColor.b * (1 - ratio)),
                (byte)Math.Floor(_maxColor.a * ratio + _minColor.a * (1 - ratio)));

        }

        private void SetPlayTimeTxt()
        {
            RemainTimeTxt.text = string.Format(
                TimeFormat,
                new DateTime(new TimeSpan(0, 0, 0, 0, Mathf.RoundToInt(_currentPlayTime * 1000)).Ticks).ToString("mm:ss:ff")
            );
            RemainTimeTxt.color = GetTxtColor(_currentPlayTime / (float)PlayTime);
        }

        private void SetPercentageTxt(float completness)
        {
            PercentageTxt.text = string.Format(
                PercentFormat,
                Mathf.RoundToInt(completness * 100)
            );
            PercentageTxt.color = GetTxtColor(completness);
            var scale = 1 + completness;
            PercentageTxt.GetComponent<RectTransform>().localScale = new Vector3(scale, scale, scale);
        }

        private void SetReadCountTxt(bool animate)
        {
            RemainCountTxt.text = string.Format(
                CountFormat,
                _currentReadCount
            );
            RemainCountTxt.color = GetTxtColor(1 - _currentReadCount / (float)ReadCount);
            if (animate)
            {
                RemainCountTxt.GetComponent<Animator>().SetTrigger("Changed");
            }
        }

        private static void SetPaperStackThickness(GameObject paperStack, GameObject topPaper, uint count)
        {
            if (count == 0)
            {
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

        private void OnSlept()
        {
            _currentFallingSleepTime = InitialFallingSleepTime;
            GetComponent<GestureGame>().StartGame(this);
            SetDebuffType(DebuffType.Sleep);
        }

        private void OnShake(float timePerShake)
        {
            if (_remainFallingSleepTime <= 0 || (_currentFallingSleepTime - _remainFallingSleepTime) <= 1)
            {
                return;
            }
            if (timePerShake <= 0.15)
            {
                _currentFallingSleepTime *= 0.666f;
                _remainFallingSleepTime = _currentFallingSleepTime;
            }
        }

        public void OnWakeUp()
        {
            _remainFallingSleepTime = _currentFallingSleepTime;
            SetDebuffType(DebuffType.None);
        }

        private void SetDebuffType(DebuffType type)
        {
            if (_currentDebuffType == type)
            {
                return;
            }
            _currentDebuffType = type;
            if (type == DebuffType.None)
            {
                _brush.SetBrushType(CameraBrush.BrushType.Normal);
            }
            else if (type == DebuffType.Love)
            {
                SubTitleText.text = "Suddenly I miss my girlfriend who broke up a year ago...";
                SubTitleText.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
                _brush.SetBrushType(CameraBrush.BrushType.Love);
            }
            else if (type == DebuffType.Game)
            {
                SubTitleText.text = "Suddenly I wanna play game... ANY game is ok...";
                SubTitleText.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
                _brush.SetBrushType(CameraBrush.BrushType.Game);
            }
            else if (type == DebuffType.Sleep)
            {
                SubTitleText.text = "I fell asleep! have to escape.";
                SubTitleText.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
                _brush.SetBrushType(CameraBrush.BrushType.Sleep);
            }
            else if (type == DebuffType.Noise)
            {
                SubTitleText.text = "Unwelcomed intruder comes...";
                SubTitleText.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
                _brush.SetBrushType(CameraBrush.BrushType.Noise);
                PauseSleep();
                NoiseEventInstance.SetActive(true);
            }
            else if (type == DebuffType.Dance)
            {
                SubTitleText.text = "My Roommates held HELL Dancing Party....";
                SubTitleText.GetComponent<CanvasRenderer>().SetAlpha(1.0f);
                _brush.SetBrushType(CameraBrush.BrushType.Dance);
                PauseSleep();
                DanceEventInstance.SetActive(true);
            }
            Invoke("HideSubtitle", 5);
        }

        private void HideSubtitle()
        {
            SubTitleText.GetComponent<CanvasRenderer>().SetAlpha(0.0f);
        }

        private IEnumerator EndGame()
        {
            enabled = false;
            var t = 0.0f;
            while (t < 1.5f)
            {
                t += 1 / 60.0f;
                var color = SleepBlocker.color;
                color.a = t;
                SleepBlocker.color = color;
                yield return null;
            }
            ReadingMissionResult.ReadingFinished = _currentReadCount == 0;
            ReadingMissionResult.RemainCount = _currentReadCount / (float)ReadCount;
            ReadingMissionResult.RemainTime = _currentPlayTime / PlayTime;
            SceneManager.LoadScene("Ending Scene", LoadSceneMode.Single);
        }
    }
}
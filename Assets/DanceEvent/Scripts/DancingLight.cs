﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DancingLight : MonoBehaviour {

  public Light sp1;
  public Light sp2;
  //public Light sp3;
  public Light sp4;
  public Light sp5;
  public Light original_Light;
  public AudioClip sound1;
  public AudioClip sound2;

  private Color initColor;
  private AudioSource au;

  private float nextActionTime;
  private float period = .4f;
  private bool isStarted;

  void OnEnable() {
    nextActionTime = 0f;
    //original_Light.SetActive(false);
    original_Light.enabled = false;
    RenderSettings.ambientLight = RenderSettings.ambientLight * 0.2f;
    isStarted = true;
    au = GetComponent<AudioSource>();

    Random.InitState((int)System.DateTime.Now.Ticks);
    int randomClipId = Random.Range(0, 2);
    if (randomClipId == 0)
      au.clip = sound1;
    else
      au.clip = sound2;
    au.Play();
  }
	
	// Update is called once per frame
	void Update () {
    Random.seed = (int)System.DateTime.Now.Ticks;
    

    sp1.transform.Rotate(Vector3.up * Time.deltaTime * 200.0f, Space.World);
    sp2.transform.Rotate(Vector3.up * Time.deltaTime * 250.0f, Space.World);
    //sp3.transform.Rotate(Vector3.up * Time.deltaTime * 400.0f, Space.World);
    sp4.transform.Rotate(Vector3.up * Time.deltaTime * 250.0f, Space.World);
    sp5.transform.Rotate(Vector3.up * Time.deltaTime * 250.0f, Space.World);

    if (Time.time > nextActionTime)
    {
      nextActionTime += period;
      sp1.color = Random.ColorHSV(0f, 1f, .7f, 1f, .7f, 1f);
      sp2.color = Random.ColorHSV(0f, 1f, .7f, 1f, .7f, 1f);
      //sp3.color = Random.ColorHSV(0f, 1f, .7f, 1f, .7f, 1f);
      sp4.color = Random.ColorHSV(0f, 1f, .7f, 1f, .7f, 1f);
      sp5.color = Random.ColorHSV(0f, 1f, .7f, 1f, .7f, 1f);

      if (!au.isPlaying && isStarted)
      {
        //original_Light.SetActive(true);
        original_Light.enabled = true;
        RenderSettings.ambientLight = RenderSettings.ambientLight * 5.0f;
        isStarted = false;
        //this.gameObject.SetActive(false);
      }
      if (!isStarted)
      {
        this.gameObject.SetActive(false);
      }
    }
  }
}



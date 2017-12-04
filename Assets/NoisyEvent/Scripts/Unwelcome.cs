using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Unwelcome : MonoBehaviour {

  private float elased_t;

  private Animation anim;
  public AnimationClip come1;
  public AnimationClip come2;
  public AnimationClip loop;

  private bool wasPlayCome1;
  private bool wasPlayCome2;
  private bool wasPlayLoop;

  public AudioSource quack;

  // Use this for initialization
  void OnEnable() {
    elased_t = 0.0f;
    anim = GetComponent<Animation>();
    quack = GetComponent<AudioSource>();
    wasPlayCome1 = false;
    wasPlayCome2 = false;
    wasPlayLoop = false;
  }
	
	// Update is called once per frame
	void Update () {
    elased_t += Time.deltaTime;

    if (elased_t > 3.0f && elased_t <= 12.0f)
    {
      if (!wasPlayCome1)
      {
        anim.wrapMode = WrapMode.Once;
        anim.clip = come1;
        anim.Play();

        quack.volume = 0.5f;

        wasPlayCome1 = true;
      }
      
    }
    else if (elased_t > 12.0f && elased_t <= 21.0f)
    {
      if (!wasPlayCome2)
      {
        anim.wrapMode = WrapMode.Once;
        anim.clip = come2;
        anim.Play();

        quack.volume = 0.7f;

        wasPlayCome2 = true;
      }
    }
    else if (elased_t > 21.0f && elased_t <= 35.0f)
    {
      if (!wasPlayLoop)
      {
        anim.wrapMode = WrapMode.Loop;
        anim.clip = loop;
        anim.Play();

        quack.volume = 1.0f;

        wasPlayLoop = true;
      }
    }
    else if (elased_t > 35.0f)
    {
      this.gameObject.SetActive(false);
    }
	}
}

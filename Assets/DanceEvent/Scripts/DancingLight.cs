using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DancingLight : MonoBehaviour {

  private float nextActionTime;
  private float period = .4f;

  public Light sp1;
  public Light sp2;
  //public Light sp3;
  public Light sp4;
  public Light sp5;

  public AudioSource au;
  // Use this for initialization
  void Start () {
    nextActionTime = 0f;
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

      if (!au.isPlaying)
      {
        DancingisOver();
      }
    }
  }

  void DancingisOver()
  {
    SceneManager.LoadScene("Main scene"); //Go back to Main Scene
  }
}



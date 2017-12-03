using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SleepEvent : MonoBehaviour {
  
  float time_limit;
  Vector3 init_position;
	
	void Start () {
    time_limit = 6.0f;
    init_position = Vector3.zero;
  }
	
	// Update is called once per frame
	void Update () {
    Camera curr = Camera.main;
    
    if (init_position == Vector3.zero)
      init_position = curr.transform.position;
    
    time_limit -= Time.deltaTime;
    curr.fieldOfView = curr.fieldOfView + 0.1f;
    
    if (time_limit < 0)
      Restart();

    if (curr.transform.position.x < -11)
      GameEnds();

    //Debug.Log(Camera.current.transform.position);
	}

  void Restart()
  {
    time_limit = 6.0f;
    Camera.main.transform.position = init_position;
    Camera.main.fieldOfView = 60.0f;
  }

  void GameEnds()
  {

    SceneManager.LoadScene("Main scene"); //Go back to Main Scene
  }

}

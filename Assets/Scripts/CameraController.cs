using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance;


    public Camera camera1;
    public Camera camera2;
    public Camera camera3;
    public Camera camera4;

    private void Awake()
    {
        Instance = this;
        EnableCamera(camera1);
    }

  
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Z))
            EnableCamera(camera1);
        if (Input.GetKeyUp(KeyCode.X))
            EnableCamera(camera2);
        if (Input.GetKeyUp(KeyCode.C))
            EnableCamera(camera3);
        if (Input.GetKeyUp(KeyCode.V))
            EnableCamera(camera4);
            
    }

    private void EnableCamera(Camera camera)
    {
        DisableCameras();
        camera.gameObject.SetActive(true);
    }

    private void DisableCameras()
    {
        camera1.gameObject.SetActive(false);
        camera2.gameObject.SetActive(false);
        camera3.gameObject.SetActive(false);
        camera4.gameObject.SetActive(false);
    }
}

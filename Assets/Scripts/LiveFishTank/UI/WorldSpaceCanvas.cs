using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldSpaceCanvas : MonoBehaviour
{
    private Camera _camera;
    private const string _cameraTag = "UICamera";

    void Start()
    {
        _camera = GameObject.FindWithTag(_cameraTag).GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        transform.LookAt(transform.position + _camera.transform.forward, _camera.transform.up);
    }
}

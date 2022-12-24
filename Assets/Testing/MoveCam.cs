using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MoveCam : MonoBehaviour
{
    [SerializeField] private Transform body;
    [SerializeField] private float speed = 1.0f;

    // Update is called once per frame
    void Update()
    {
        if (!Keyboard.current.shiftKey.isPressed) return;
        
        body.Translate(Vector3.down * (Time.deltaTime * speed));
    }
}

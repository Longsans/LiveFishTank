using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Accelerometer : Singleton<Accelerometer>
{
    [HideInInspector] public UnityEvent OnShake;
    private float shakeDetectionThreshold = 1.5f;
    private float updateInterval = 1f / 60f;
    private float lowPassKernelWidthInSeconds = 1f;
    private float lowPassFilterFactor = 1f / 60f;
    private Vector3 lowPassValue;

    // Start is called before the first frame update
    void Start()
    {
        lowPassFilterFactor = updateInterval / lowPassKernelWidthInSeconds;
        shakeDetectionThreshold *= shakeDetectionThreshold;
        lowPassValue = Input.acceleration;
    }

    // Update is called once per frame
    void Update()
    {
        var acceleration = Input.acceleration;
        lowPassValue = Vector3.Lerp(lowPassValue, acceleration, lowPassFilterFactor);
        var delta = acceleration - lowPassValue;
        if (delta.sqrMagnitude >= shakeDetectionThreshold)
            OnShake.Invoke();
    }
}

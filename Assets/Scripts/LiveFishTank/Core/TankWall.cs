using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TankWall : MonoBehaviour
{
    public bool IsCollidingWithVerticalPlane { get; set; } = false;
    private FishTank _tank;

    void Start()
    {
        _tank = GetComponentInParent<FishTank>();
    }

    void OnTriggerStay(Collider other)
    {
        var plane = other.GetComponent<ARPlane>();
        if (plane && plane.alignment == PlaneAlignment.Vertical)
        {
            IsCollidingWithVerticalPlane = true;
            _tank.CheckToggleWarningColor();
        }
    }

    void OnTriggerExit(Collider other)
    {
        var plane = other.GetComponent<ARPlane>();
        if (plane && plane.alignment == PlaneAlignment.Vertical)
        {
            IsCollidingWithVerticalPlane = false;
            _tank.CheckToggleWarningColor();
        }
    }
}

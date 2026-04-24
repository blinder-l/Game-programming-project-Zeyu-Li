using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitMotion : MonoBehaviour
{
    public Vector3 rotationAxis = Vector3.up;
    public float orbitSpeed = 20f;

    void Update()
    {
        transform.Rotate(rotationAxis, orbitSpeed * Time.deltaTime);
    }
}

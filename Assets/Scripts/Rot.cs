using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rot : MonoBehaviour
{
    private Transform _tf;
    
    void Start()
    {
        _tf = transform;
    }

    
    void Update()
    {
        _tf.Rotate(Vector3.up, 1, Space.Self);
    }
}

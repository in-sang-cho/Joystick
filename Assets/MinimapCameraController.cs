using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCameraController : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(
            transform.position.x,
            100.0f,
            transform.position.z);
    }
}

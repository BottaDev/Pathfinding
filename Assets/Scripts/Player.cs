using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float Speed;

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
            transform.position += Speed * Time.deltaTime * Vector3.back;
        else if (Input.GetKey(KeyCode.S))
            transform.position += Speed * Time.deltaTime * Vector3.forward;
        
        if (Input.GetKey(KeyCode.A))
            transform.position += Speed * Time.deltaTime * Vector3.right;
        else if (Input.GetKey(KeyCode.D))
            transform.position += Speed * Time.deltaTime * Vector3.left;
    }
}

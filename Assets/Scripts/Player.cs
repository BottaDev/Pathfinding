using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 11)
        {
            SceneManager.LoadScene("Level");
        }
    }
}

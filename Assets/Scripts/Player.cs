using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private CharacterController _controller;

    private float _speed = 6.0f;

    private Vector3 _direction, _velocity;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        // Store reference to input axis
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Implementing speed and direction to the movement before calling
        // the controller Move function
        _direction = new Vector3(horizontalInput, 0, verticalInput);
        _velocity = _direction * _speed;
        _controller.Move(_velocity * Time.deltaTime);

    }
}

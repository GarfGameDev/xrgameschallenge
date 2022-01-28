using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private CharacterController _controller;
    [SerializeField]
    private UIManager _uiManager;

    private float _speed = 6.0f;
    private int _playerScore;

    private Vector3 _direction, _velocity;
    // Start is called before the first frame update
    void Start()
    {
        if (GameObject.Find("GUI").GetComponent<UIManager>() != null)
        {
            _uiManager = GameObject.Find("GUI").GetComponent<UIManager>();
        }
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

    // Checks for collisions with the pickups and endzone
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("I have collided with something");
        // In the case of a pickup then 100 is added to this player's score
        // which is updated in the UI and the pickup is removed so that it
        // can't be collected again
        if (other.gameObject.tag == "Pickup")
        {
           
            Pickup pickup = other.gameObject.GetComponent<Pickup>();

            _playerScore += pickup.GetPickedUp();
            _uiManager.UpdateScoreText(_playerScore);
            pickup.gameObject.SetActive(false);

            Debug.Log("The current player score is:" + _playerScore);
        }
        else if (other.gameObject.tag == "EndZone")
        {
            other.gameObject.GetComponent<EndZone>().CheckForGameOver(_playerScore);
        }
    }
}

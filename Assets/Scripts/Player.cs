using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace Character
{
    public class Player : NetworkBehaviour
    {
        [SerializeField]
        private CharacterController _controller;
        [SerializeField]
        private UIManager _uiManager;

        private float _speed = 6.0f;
        private int _playerScore;
        
        // This bool is implemented so that each player prefab that's spawned in can be
        // controlled seperately
        private bool _canMove = false;

        [SerializeField]
        private Vector3 _direction, _velocity;

        // Create a Network Variable for position that it can be later read or updated on the server
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

        
        
        // Replace Start() function with an override for the Netcode OnNetworkSpawn() method
        // which is called when the NetworkObject component attached to the Player is spawned in
        public override void OnNetworkSpawn()
        {
            if (GameObject.Find("GUI").GetComponent<UIManager>() != null)
            {
                _uiManager = GameObject.Find("GUI").GetComponent<UIManager>();
            }

            if (IsOwner)
            {
                Debug.Log("I am the owner");
                SpawnPoint();
            }
        }

        // Determines which spawn point to use depending if you're the host or not
        public void SpawnPoint()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Debug.Log("IsServer check");
                Position.Value = GameObject.Find("SpawnPoint1").transform.position;
                transform.position = Position.Value;
                _canMove = true;
            }
            else
            {
                transform.position = GameObject.Find("SpawnPoint2").transform.position;
                Debug.Log("Is not a server check");
                PosRequestServerRpc();
                StartCoroutine(InitialSpawnRoutine());


            }
        }

        void Update()
        {
            if (NetworkManager.Singleton.IsServer  && !NetworkManager.Singleton.IsClient)
            {
                UpdateServer();
                
            }
            if (NetworkManager.Singleton.IsClient && IsOwner)
            {
                if (_canMove == true)
                {
                    UpdateClient();
                }
                
            }

        }
        
        private void UpdateServer()
        {
            transform.position = Position.Value;
           
        }

        // Checks for collisions with the pickups and endzone
        private void OnTriggerEnter(Collider other)
        {
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

        // Implemented coroutine so that the initial spawn position updates client side
        IEnumerator InitialSpawnRoutine()
        {
            yield return new WaitForSeconds(0.1f);
            transform.position = Position.Value;
            _canMove = true;
        }

        // Updates the Server's instance of the player's position to the 2nd spawn point
        // the host's instance of the game will see this change
        [ServerRpc]
        private void PosRequestServerRpc(ServerRpcParams rpcParams = default)
        {
            Position.Value = GameObject.Find("SpawnPoint2").transform.position;
            transform.position = Position.Value;

        }

        [ServerRpc]
        private void UpdateMovementServerRpc()
        {
            Position.Value = transform.position;
        }

        private void UpdateClient()
        {

                Vector3 oldPos = transform.position;

                // Store reference to input axis
                float horizontalInput = Input.GetAxis("Horizontal");
                float verticalInput = Input.GetAxis("Vertical");

                // Implementing speed and direction to the movement before calling
                // the controller Move function
                _direction = new Vector3(horizontalInput, 0, verticalInput);
                _velocity = _direction * _speed;
                _controller.Move(_velocity * Time.deltaTime);
                
            if (oldPos != transform.position)
            {
                UpdateMovementServerRpc();
            }
                    
                
                   
        }
    }
}


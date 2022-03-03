using UnityEngine;
using Unity.Netcode;
using Unity.Services.Analytics;
using System.Collections.Generic;

namespace Character
{
    public class Player : NetworkBehaviour
    {
        [SerializeField]
        private CharacterController _controller;
        [SerializeField]
        private Animator _animator;

        [SerializeField]
        private float _speed = 6.0f;

        // Creating two separate instance of player score to store and send info to the ScoreManager
        private int _playerScore;
        private int _playerScore2;
        
        private bool _isPlayer2 = false;

        [SerializeField]
        private Vector3 _direction, _velocity;

        public enum PlayerAnimState
        {
            Idle,
            Run,
        }

        // Create a Network Variable for position that it can be later read or updated on the server
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<Quaternion> Rotation = new NetworkVariable<Quaternion>();
        public NetworkVariable<PlayerAnimState> Animstate = new NetworkVariable<PlayerAnimState>();

        // Replace Start() function with an override for the Netcode OnNetworkSpawn() method
        // which is called when the NetworkObject component attached to the Player is spawned in
        public override void OnNetworkSpawn()
        {
            if (IsOwner)
            {
                SpawnPoint();
            }
        }

        // Determines which spawn point to use depending if you're the host or not
        public void SpawnPoint()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Position.Value = GameObject.Find("SpawnPoint1").transform.position;
                transform.position = Position.Value;
            }
            else
            {
                _isPlayer2 = true;
                PosRequestServerRpc();
            }
        }

        void Update()
        {
            if (NetworkManager.Singleton.IsClient && IsOwner)
            {             
                    UpdateClient();              
            }

            if (Animstate.Value == PlayerAnimState.Run)
            {
                _animator.SetFloat("Run", 1);
            }
            else
            {
                _animator.SetFloat("Run", 0);
            }

        }

        // Calls the ServerRpc methods in ScoreManager in order to assign the updated score to
        // the networked variables
        private void UpdateScoreManager()
        {
            if (_isPlayer2 == false)
            {
                ScoreManager.Instance.UpdateScore1ServerRpc(_playerScore);
            }
            else
            {               
                ScoreManager.Instance.UpdateScore2ServerRpc(_playerScore2);
            }         
        }

        // Adds on the score to local player score variables based on the value defined in Pickup
        private void UpdateClientScore(Pickup pickup)
        {
            if (_isPlayer2 == false)
            {
                _playerScore += pickup.GetPickedUp();                
                UpdateScoreManager();
            }
            else
            {
                _playerScore2 += pickup.GetPickedUp();
                UpdateScoreManager();
            }
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

                if (NetworkManager.Singleton.IsClient && IsOwner)
                {
                    UpdateClientScore(pickup);
                }

                pickup.gameObject.SetActive(false);

            }
            else if (other.gameObject.tag == "EndZone")
            {
                // Launches the check for game over status and stores 0 in the local score variables when it's a true condition
                // to avoid the winning player from launching the game over after coming from the end screen
                if (_isPlayer2 == false)
                {
                    other.gameObject.GetComponent<EndZone>().CheckForGameOver(_playerScore);                 
                    if (_playerScore > 400)
                    {
                        Dictionary<string, object> paramaters = new Dictionary<string, object>()
                        {
                            { "clientVersion", "v0.1" },
                            { "platform", "PC_CLIENT" },
                            { "userScore", _playerScore }
                        };
                        Events.CustomData("gameWon", paramaters);
                        Events.Flush();
                        _playerScore = 0;
                        _playerScore2 = 0;
                    }
                }
                else if (_isPlayer2 == true)
                {
                    other.gameObject.GetComponent<EndZone>().CheckForGameOver(_playerScore2);
                    if (_playerScore2 > 400)
                    {
                        Dictionary<string, object> paramaters = new Dictionary<string, object>()
                        {
                            { "clientVersion", "v0.1" },
                            { "platform", "PC_CLIENT" },
                            { "userScore", _playerScore2 }
                        };
                        Events.CustomData("gameWon", paramaters);
                        Events.Flush();
                        _playerScore = 0;
                        _playerScore2 = 0;
                    }
                }
                
            }
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
            Rotation.Value = transform.rotation;
        }

        [ServerRpc]
        private void UpdatePlayerStateServerRpc(PlayerAnimState state)
        {
            Animstate.Value = state;
        }

        private void UpdateClient()
        {
            Vector3 oldPos = transform.position;
            transform.rotation = Rotation.Value;
             
            // Store reference to input axis
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");


            // Implementing speed and direction to the movement before calling
            // the controller Move function
            _direction = new Vector3(horizontalInput, 0, verticalInput);
            _velocity = _direction * _speed;
            _controller.Move(_velocity * Time.deltaTime);
              
            // Updates the network values for Position and Rotation and also changes local rotation value
            // depending on the direction
            if (oldPos != transform.position)
            {
                transform.rotation = Quaternion.LookRotation(_direction);
                UpdateMovementServerRpc();
            }             
            
            if (verticalInput > 0 || horizontalInput > 0)
            {
                UpdatePlayerStateServerRpc(PlayerAnimState.Run);
            }
            else if (verticalInput < 0 || horizontalInput < 0)
            {
                UpdatePlayerStateServerRpc(PlayerAnimState.Run);
            }
            else
            {
                UpdatePlayerStateServerRpc(PlayerAnimState.Idle);
            }
        }
    }
}


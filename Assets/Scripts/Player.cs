using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

namespace Character
{
    public class Player : NetworkBehaviour
    {
        [SerializeField]
        private CharacterController _controller;
        [SerializeField]
        private Animator _animator;

        private float _speed = 6.0f;

        // Creating two separate instance of player score to store and send info to the ScoreManager
        private int _playerScore;
        private int _playerScore2;
        
        public bool _isPlayer2 = false;

        [SerializeField]
        private Vector3 _direction, _velocity;

        public enum playerAnimState
        {
            Idle,
            Run,
        }


        // Create a Network Variable for position that it can be later read or updated on the server
        public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
        public NetworkVariable<Quaternion> Rotation = new NetworkVariable<Quaternion>();
        public NetworkVariable<playerAnimState> Animstate = new NetworkVariable<playerAnimState>();

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

            if (Animstate.Value == playerAnimState.Run)
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
            ScoreManager.Instance.UpdateScore1ServerRpc(_playerScore);
            ScoreManager.Instance.UpdateScore2ServerRpc(_playerScore2);
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
                    // Store the networked variables of both player scores from the ScoreManager
                    // into local variables
                    _playerScore = ScoreManager.Instance.P1Score;
                    _playerScore2 = ScoreManager.Instance.P2Score;
                    UpdateClientScore(pickup);
                }

                pickup.gameObject.SetActive(false);

                Debug.Log("The current player1 score is:" + _playerScore);
                Debug.Log("The current player2 score is:" + _playerScore2);
            }
            else if (other.gameObject.tag == "EndZone")
            {
                if (_isPlayer2 == false)
                {
                    other.gameObject.GetComponent<EndZone>().CheckForGameOver(_playerScore);                 
                    if (_playerScore > 400)
                    {
                        _playerScore = 0;
                        _playerScore2 = 0;
                    }
                }
                else if (_isPlayer2 == true)
                {
                    other.gameObject.GetComponent<EndZone>().CheckForGameOver(_playerScore2);
                    if (_playerScore2 > 400)
                    {
                        _playerScore = 0;
                        _playerScore2 = 0;
                    }
                }
                
            }
        }

        // Implemented coroutine so that the initial spawn position updates client side
        IEnumerator InitialSpawnRoutine()
        {
            yield return new WaitForSeconds(0.1f);
            transform.position = Position.Value;
            
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
        private void UpdatePlayerStateServerRpc(playerAnimState state)
        {
            Animstate.Value = state;
        }

        private void UpdateClient()
        {
            Vector3 oldPos = transform.position;
            //Quaternion oldRot = transform.rotation;
            transform.rotation = Rotation.Value;
             

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
                transform.rotation = Quaternion.LookRotation(_direction);
                UpdateMovementServerRpc();
            }             
            
            if (verticalInput > 0 || horizontalInput > 0)
            {
                UpdatePlayerStateServerRpc(playerAnimState.Run);
            }
            else if (verticalInput < 0 || horizontalInput < 0)
            {
                UpdatePlayerStateServerRpc(playerAnimState.Run);
            }
            else
            {
                UpdatePlayerStateServerRpc(playerAnimState.Idle);
            }
        }
    }
}


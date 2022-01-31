using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Character
{
    public class NetworkState : NetworkBehaviour
    {
        private string _textString = "Enter Code";
        [SerializeField]
        private Button _startButton, _joinButton;
        // Creates GUI for the network buttons

        // Implements functionality for buttons
        private void Start()
        { 

            _startButton.onClick.AddListener(() => NetworkManager.Singleton.StartHost());
            _startButton.onClick.AddListener(() => NetworkManager.SceneManager.LoadScene("Main", 0));


            _joinButton.onClick.AddListener(() => NetworkManager.Singleton.StartClient());
            _joinButton.onClick.AddListener(() => NetworkManager.SceneManager.LoadScene("Main", 0));
        }


    }
}
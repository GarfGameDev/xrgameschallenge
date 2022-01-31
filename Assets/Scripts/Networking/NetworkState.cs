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

        [SerializeField]
        private InputField _joinCodeInput;
        // Creates GUI for the network buttons

        // Implements functionality for buttons
        private void Start()
        { 

            _startButton.onClick.AddListener(async() =>
            {
                if (RelayManager.Instance.IsRelayEnabled)
                {
                    await RelayManager.Instance.HostGame();
                }

                NetworkManager.Singleton.StartHost();
                NetworkManager.SceneManager.LoadScene("Main", 0);


            });


            _joinButton.onClick.AddListener(async() =>
            {
                if (RelayManager.Instance.IsRelayEnabled && !string.IsNullOrEmpty(_joinCodeInput.text))
                {
                    await RelayManager.Instance.JoinRelay(_joinCodeInput.text);
                }

                NetworkManager.Singleton.StartClient();
                NetworkManager.SceneManager.LoadScene("Main", 0);


                });
        }


    }
}
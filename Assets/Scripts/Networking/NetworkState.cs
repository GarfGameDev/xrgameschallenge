using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Character
{
    public class NetworkState : NetworkBehaviour
    {
        [SerializeField]
        private Button _startButton, _joinButton;

        [SerializeField]
        private TextMeshProUGUI _lobbyFullText;

        [SerializeField]
        private InputField _joinCodeInput;

        // Implementing the option to launch the game as a host or join as a client
        // depending on the button clicked on.
        // Also launches Main scene
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
                    if (RelayManager.Instance.Player2Connect < 2)
                    {
                        NetworkManager.SceneManager.LoadScene("Main", 0);
                    }
                    else
                    {
                        _lobbyFullText.gameObject.SetActive(true);
                    }




                });
        }


    }
}
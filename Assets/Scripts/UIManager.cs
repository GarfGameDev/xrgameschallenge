using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class UIManager : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _scoreTextP1, _scoreTextP2, _playerReadyText, _player2ReadyText;

    [SerializeField]
    private Button _restartButton;

    [SerializeField]
    private GameObject _endScreen;

    public NetworkVariable<int> PlayersReady = new NetworkVariable<int>();
    public NetworkVariable<int> ClientWin = new NetworkVariable<int>();
    public NetworkVariable<int> SinglePlayer = new NetworkVariable<int>();

    private void Start()
    {
        // If the restart button is clicked server side the main scene will be launched when conditions are true
        if (NetworkManager.Singleton.IsServer)
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(() =>
                {
                    if (PlayersReady.Value > 0 || RelayManager.Instance.Player2Connect == 1)
                    {
                        _endScreen.gameObject.SetActive(false);
                        NetworkManager.SceneManager.LoadScene("Main", 0);
                    }


                });
            }

        }
        // If the client presses restart it will display a message letting both players know that they're ready
        else
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(() =>
                {
                    UpdatePlayerReadyServerRpc();
                    _restartButton.gameObject.SetActive(false);
                    _player2ReadyText.gameObject.SetActive(true);
                });
            }
        }

    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerReadyServerRpc()
    {
        PlayersReady.Value += 1;
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateClientWinServerRpc()
    {
        ClientWin.Value = 1;
    }

    // Accesses Score properties from ScoreManager in order to get the current networked scores
    // and load the EndScreen scene when one of the players wins
    private void Update()
    {
        if (ScoreManager.Instance != null)
        {
            _scoreTextP1.text = "SCORE: " + ScoreManager.Instance.P1Score.ToString();
            _scoreTextP2.text = "SCORE: " + ScoreManager.Instance.P2Score.ToString();
        }

        if (PlayersReady.Value > 0)
        {
            _playerReadyText.gameObject.SetActive(true);
        }


        if (NetworkManager.Singleton.IsServer)
        {
            if (ClientWin.Value > 0)
            {
                NetworkManager.SceneManager.LoadScene("EndScreen", 0);
            }
        }
    }

    //
    public void GameOver()
    {
        UpdateClientWinServerRpc();
    }
}

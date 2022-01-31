using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class UIManager : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _scoreTextP1, _scoreTextP2;

    [SerializeField]
    private Button _restartButton;

    [SerializeField]
    private GameObject _networkManger, _endScreen;

    public NetworkVariable<int> PlayersReady = new NetworkVariable<int>();
    public NetworkVariable<int> ClientWin = new NetworkVariable<int>();

    private void Start()
    {
        //_restartButton.onClick.AddListener(() => Destroy(_networkManger));
        if (NetworkManager.Singleton.IsServer)
        {
            _restartButton.onClick.AddListener(() =>
            { 
                if (PlayersReady.Value > 0)
                {
                    _endScreen.gameObject.SetActive(false);
                    
                    NetworkManager.SceneManager.LoadScene("Main", 0);
                }
                
                
            });
        }
        else
        {
            _restartButton.onClick.AddListener(() => UpdatePlayerReadyServerRpc());
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
    private void Update()
    {
        _scoreTextP1.text = "SCORE: " + ScoreManager.Instance.P1Score.ToString();
        _scoreTextP2.text = "SCORE: " + ScoreManager.Instance.P2Score.ToString();

        if (NetworkManager.Singleton.IsServer)
        {
            if (ClientWin.Value > 0)
            {
                NetworkManager.SceneManager.LoadScene("EndScreen", 0);
            }
        }
    }

    public void GameOver()
    {
        UpdateClientWinServerRpc();
    }
}

using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ScoreManager : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _scoreTextP1, _scoreTextP2;
    // Quick implementation to turn the ScoreManager into a Singleton
    private static ScoreManager _instance;
    public static ScoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                return null;
            }
            return _instance;
        }
    }

    private NetworkVariable<int> P1Score= new NetworkVariable<int>();
    private NetworkVariable<int> P2Score = new NetworkVariable<int>();

    private void Awake()
    {
        _instance = this;
    }

    private void Update()
    {
        _scoreTextP1.text = "SCORE: " + P1Score.Value.ToString();
        _scoreTextP2.text = "SCORE: " + P2Score.Value.ToString();
    }

    // Makes call to server to update score with client info
    [ServerRpc(RequireOwnership = false)]
    public void UpdateScore1ServerRpc(int score)
    {
        P1Score.Value = score;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateScore2ServerRpc(int score)
    {
        P2Score.Value = score;
    }

}

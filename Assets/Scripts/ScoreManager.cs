using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ScoreManager : NetworkBehaviour
{
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

    private NetworkVariable<int> P1ScoreText = new NetworkVariable<int>();
    private NetworkVariable<int> P2ScoreText = new NetworkVariable<int>();

    // Create public properties so that stores the networked score so that it can
    // be used by other scripts
    public int P1Score
    {
        get
        {
            return P1ScoreText.Value;
        }
    }

    public int P2Score
    {
        get
        {
            return P2ScoreText.Value;
        }
    }
    private void Awake()
    {
        _instance = this;
    }

    // Makes call to server to update score with client info
    [ServerRpc(RequireOwnership = false)]
    public void UpdateScore1ServerRpc(int score)
    {
        P1ScoreText.Value = score;
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateScore2ServerRpc(int score)
    {
        P2ScoreText.Value = score;
    }

}

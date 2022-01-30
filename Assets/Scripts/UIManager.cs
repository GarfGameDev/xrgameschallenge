using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _scoreTextP1, _scoreTextP2;

    // Accesses Score properties from ScoreManager in order to get the current networked scores
    private void Update()
    {
        _scoreTextP1.text = "SCORE: " + ScoreManager.Instance.P1Score.ToString();
        _scoreTextP2.text = "SCORE: " + ScoreManager.Instance.P2Score.ToString();
    }
}

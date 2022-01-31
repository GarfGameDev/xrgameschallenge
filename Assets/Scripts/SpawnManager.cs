using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _joinCodeText;
    
    void Start()
    {
        _joinCodeText.text = "JOIN CODE: " + RelayManager.Instance.JoinCode;
    }


}

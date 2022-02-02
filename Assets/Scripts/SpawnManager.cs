using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _joinCodeText;

    [SerializeField]
    private GameObject[] _pickupArray;
    private int[] _numberArray = new int[11];

    private bool _alreadyActive = false;

    private NetworkVariable<int> RandomNumber = new NetworkVariable<int>();
    private NetworkVariable<int> NumOfLoops = new NetworkVariable<int>();

    void Start()
    {
        // Displays a join code to give to the other player to use when you're the host
        _joinCodeText.text = "JOIN CODE: " + RelayManager.Instance.JoinCode;

        for (int i = 0; i < _pickupArray.Length; i++)
        {
            _pickupArray[i].SetActive(false);
        }

        if (NetworkManager.Singleton.IsServer)
        {
            NumOfLoops.Value = 0;
        }


        StartCoroutine(StartPickupSpawnRoutine());


    }

    // This Coroutine handles the random spawning of the 10 collectibles in fixed positions once
    // every 2 seconds
    IEnumerator StartPickupSpawnRoutine()
    {
        yield return new WaitForSeconds(1.0f);
        while (NumOfLoops.Value < 10)
        {
            _alreadyActive = false;
            if (NetworkManager.Singleton.IsServer)
            {
                RandomNumber.Value = Random.Range(0, 11);
            }
            
            for (int i = 0; i < _numberArray.Length; i++)
            {
                if (_numberArray[i] == RandomNumber.Value)
                {
                    _alreadyActive = true;
                }
            }
            if (_alreadyActive == false)
            {
                _pickupArray[RandomNumber.Value].SetActive(true);
                _numberArray[RandomNumber.Value] = RandomNumber.Value;
                if (NetworkManager.Singleton.IsServer)
                {
                    NumOfLoops.Value++;
                }              
            }
            else
            {
                continue;
            }

            yield return new WaitForSeconds(2.0f);
        }
    }

}

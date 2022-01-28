using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndZone : MonoBehaviour
{
    // Check to see if player has collected enough pickups
    // and reload the game if they have
    public void CheckForGameOver(int score)
    {
        if (score > 400)
        {
            SceneManager.LoadScene(0);
        }
    }
}

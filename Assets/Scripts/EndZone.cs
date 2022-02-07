using UnityEngine;

public class EndZone : MonoBehaviour
{
    [SerializeField]
    private UIManager _uIManager;
    // Check to see if player has collected enough pickups
    // and run the GameOver() method in the UI Manager
    public void CheckForGameOver(int score)
    {
        if (score > 400)
        {
            GetComponent<BoxCollider>().enabled = false;
            _uIManager.GameOver();
            
        }
    }
}

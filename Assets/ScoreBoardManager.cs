using UnityEngine;
using TMPro;

public class ScoreBoardManager : MonoBehaviour
{
    public TMP_Text[] scoreTexts; // Array to hold score text references for each player
    private int[] playerScores; // Array to track scores for each player
    public int numberOfPlayers = 5; // Total number of players

    void Start()
    {
        playerScores = new int[numberOfPlayers]; // Initialize score array
        // Debug.Log("Score Length"+scoreTexts.Length);
        // if (scoreTexts.Length != numberOfPlayers)
        // {
        //     Debug.LogWarning("ScoreTexts array length does not match the number of players!");
        //     // scoreTexts = new TMP_Text[numberOfPlayers];
        // }

        UpdateScoreDisplays(); // Update the display at the start
    }

    public void AddScore(int playerIndex, int points)
    {
        if (playerIndex >= 0 && playerIndex < (numberOfPlayers))
        {
            playerScores[playerIndex] += points; // Add points to player's score
            UpdateScoreDisplays(); // Update the score display
        }
    }

    public void DeductScore(int playerIndex, int points)
    {
        if (playerIndex >= 0 && playerIndex < (numberOfPlayers))
        {
            playerScores[playerIndex] -= points; // Deduct points from player's score
            UpdateScoreDisplays(); // Update the score display
        }
    }

    private void UpdateScoreDisplays()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {   
            // Debug.Log(scoreTexts.Length);
            scoreTexts[i].text = "Player " + (i+1) + ": " + playerScores[i]; // Update UI text
            Debug.Log( scoreTexts[i].text);
            
        }
    }
}

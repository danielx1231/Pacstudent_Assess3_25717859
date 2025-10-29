using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Components")]
    public LevelGenerator levelGenerator;
    public PacStudentController pacStudent;
    public CherryController cherryController;
    public AudioPlayer audioPlayer;
    
    [Header("Game Settings")]
    public int score = 0;
    public int lives = 3;
    public bool gameStarted = false;
    
    [Header("UI References")]
    public UnityEngine.UI.Text scoreText;
    public UnityEngine.UI.Text livesText;
    public UnityEngine.UI.Text gameOverText;
    
    void Start()
    {
        InitializeGame();
    }
    
    void InitializeGame()
    {
        // Initialize game state
        score = 0;
        lives = 3;
        gameStarted = true;
        
        // Update UI
        UpdateUI();
        
        // Start background music
        if (audioPlayer != null)
        {
            // AudioPlayer will handle its own initialization
        }
        
        Debug.Log("Game initialized!");
    }
    
    void Update()
    {
        // Check for game over conditions
        if (lives <= 0)
        {
            GameOver();
        }
        
        // Update UI
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
        
        if (livesText != null)
        {
            livesText.text = "Lives: " + lives.ToString();
        }
    }
    
    public void AddScore(int points)
    {
        score += points;
        Debug.Log("Score added: " + points + " (Total: " + score + ")");
    }
    
    public void LoseLife()
    {
        lives--;
        Debug.Log("Life lost! Lives remaining: " + lives);
        
        if (lives > 0)
        {
            // Reset PacStudent position
            ResetPacStudent();
        }
    }
    
    void ResetPacStudent()
    {
        if (pacStudent != null)
        {
            // Reset PacStudent to starting position
            pacStudent.transform.position = Vector3.zero;
            // PacStudentController will handle its own reset
        }
    }
    
    void GameOver()
    {
        gameStarted = false;
        
        if (gameOverText != null)
        {
            gameOverText.text = "GAME OVER\nFinal Score: " + score;
            gameOverText.gameObject.SetActive(true);
        }
        
        Debug.Log("Game Over! Final Score: " + score);
    }
    
    public void RestartGame()
    {
        // Reset game state
        score = 0;
        lives = 3;
        gameStarted = true;
        
        // Hide game over text
        if (gameOverText != null)
        {
            gameOverText.gameObject.SetActive(false);
        }
        
        // Reset level
        if (levelGenerator != null)
        {
            // LevelGenerator will regenerate the level
        }
        
        // Reset PacStudent
        ResetPacStudent();
        
        Debug.Log("Game restarted!");
    }
    
    // Public methods for other scripts to call
    public void OnPelletEaten(Vector2Int gridPos)
    {
        AddScore(10);
        
        // Remove pellet from level
        if (levelGenerator != null)
        {
            levelGenerator.RemovePellet(gridPos);
        }
    }
    
    public void OnCherryCollected()
    {
        AddScore(100);
        Debug.Log("Cherry collected! +100 points");
    }
    
    public void OnPacStudentHit()
    {
        LoseLife();
    }
    
    public bool IsGameActive()
    {
        return gameStarted && lives > 0;
    }
}
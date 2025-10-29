using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Labels & Values (TMP)")]
    public TMP_Text levelNameText;
    public TMP_Text scoreValueText;       
    public TMP_Text timeValueText;       
    public TMP_Text scaredValueText;      

    [Header("Lives UI")]
    public Transform livesRow;            
    public Image lifeIconPrefab;          
    public int startingLives = 3;

    [Header("Exit")]
    public Button exitButton;
    public string startSceneName = "StartScene";

    [Header("Options")]
    public bool runTimer = false;         

    int lives;
    int score;
    float elapsed;                        
    int scaredSeconds;                    

    void Awake()
    {
        // 初始化显示
        SetLevelName("LEVEL 1");
        SetScore(0);
        ResetTimerUI();
        SetScaredSeconds(0);

        BuildLives(startingLives);

        if (exitButton)
            exitButton.onClick.AddListener(OnExitClicked);
    }

    void Update()
    {
        if (!runTimer) return;

        elapsed += Time.deltaTime;
        timeValueText.text = FormatClock(elapsed);
    }

    // -------- Lives ----------
    void BuildLives(int count)
    {
        lives = Mathf.Max(0, count);
        
        for (int i = livesRow.childCount - 1; i >= 0; i--)
            Destroy(livesRow.GetChild(i).gameObject);
        
        for (int i = 0; i < lives; i++)
            Instantiate(lifeIconPrefab, livesRow);
    }

    // -------- Score ----------
    public void SetScore(int value)
    {
        score = Mathf.Max(0, value);
        scoreValueText.text = score.ToString("D6"); 
    }

    // -------- Timer ----------
    void ResetTimerUI()
    {
        elapsed = 0f;
        timeValueText.text = "00:00:00";
    }

    string FormatClock(float seconds)
    {
        
        int totalCentis = Mathf.FloorToInt(seconds * 100f);
        int cs = totalCentis % 100;
        int totalSecs = totalCentis / 100;
        int mm = totalSecs / 60;
        int ss = totalSecs % 60;
        return $"{mm:00}:{ss:00}:{cs:00}";
    }

    // -------- Scared Timer ----------
    public void SetScaredSeconds(int seconds)
    {
        scaredSeconds = Mathf.Max(0, seconds);
        scaredValueText.gameObject.SetActive(scaredSeconds > 0);
        scaredValueText.text = scaredSeconds > 0 ? scaredSeconds.ToString() : "";
    }

    // -------- Level name ----------
    public void SetLevelName(string nameText)
    {
        if (levelNameText) levelNameText.text = nameText;
    }

    // -------- Exit ----------
    void OnExitClicked()
    {
        
        SceneManager.LoadScene(startSceneName);
    }
}

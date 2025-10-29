using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if TMP_PRESENT
using TMPro;
#endif

public class StartMenuController : MonoBehaviour
{
    [Header("Buttons (assign on the outer blue card)")]
    [SerializeField] private Button level1Button;
    [SerializeField] private Button level2Button;

    [Header("Scene Names")]
    [SerializeField] private string level1SceneName = "Level1";            
    [SerializeField] private string level2SceneName = "DesignIteration";   

    [Header("Enable/Disable Level 2")]
    [SerializeField] private bool enableLevel2 = false; 

    [Header("Background Music")]
    [SerializeField] private AudioSource bgmSource;  
    [SerializeField] private AudioClip startBgmClip; 

    [Header("Level 1 Texts")]
    [SerializeField] private Text level1ScoreText;
    [SerializeField] private Text level1TimeText;
#if TMP_PRESENT
    [SerializeField] private TextMeshProUGUI level1ScoreTMP;
    [SerializeField] private TextMeshProUGUI level1TimeTMP;
#endif

    [Header("Level 2 Texts")]
    [SerializeField] private Text level2ScoreText;
    [SerializeField] private Text level2TimeText;
#if TMP_PRESENT
    [SerializeField] private TextMeshProUGUI level2ScoreTMP;
    [SerializeField] private TextMeshProUGUI level2TimeTMP;
#endif

    [Header("Optional SFX")]
    [SerializeField] private AudioSource sfxSource;   
    [SerializeField] private AudioClip hoverClip;
    [SerializeField] private AudioClip clickClip;

    void Awake()
    {
        
        if (level1Button != null)
        {
            level1Button.onClick.RemoveAllListeners();
            level1Button.onClick.AddListener(OnLevel1Clicked);
            
        }

        if (level2Button != null)
        {
            level2Button.onClick.RemoveAllListeners();
            level2Button.onClick.AddListener(OnLevel2Clicked);
            level2Button.interactable = enableLevel2;
        }

        
        SetTexts(level1ScoreText, level1TimeText, "000000", "00:00:00");
        SetTexts(level2ScoreText, level2TimeText, "000000", "00:00:00");
    }

    void Start()
    {
        
        if (bgmSource != null && startBgmClip != null)
        {
            bgmSource.clip = startBgmClip;
            bgmSource.loop = true;
            if (!bgmSource.isPlaying) bgmSource.Play();
        }
    }

   
    public void OnLevel1Clicked()
    {
        PlayClick();
        SafeLoad(level1SceneName);
    }

    public void OnLevel2Clicked()
    {
        PlayClick();
        if (!enableLevel2) return;  
        SafeLoad(level2SceneName);
    }

    
    void SafeLoad(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("StartMenuController: Scene name is empty.");
            return;
        }
        
        SceneManager.LoadScene(sceneName);
    }

    void SetTexts(Text score, Text time, string scoreVal, string timeVal)
    {
        if (score) score.text = scoreVal;
        if (time) time.text = timeVal;

        
#if TMP_PRESENT
        if (level1ScoreTMP && score == level1ScoreText) level1ScoreTMP.text = scoreVal;
        if (level1TimeTMP  && time  == level1TimeText)  level1TimeTMP.text  = timeVal;
        if (level2ScoreTMP && score == level2ScoreText) level2ScoreTMP.text = scoreVal;
        if (level2TimeTMP  && time  == level2TimeText)  level2TimeTMP.text  = timeVal;
#endif
    }

    void PlayClick()
    {
        if (sfxSource != null && clickClip != null)
            sfxSource.PlayOneShot(clickClip);
    }

    
    public void PlayHover()
    {
        if (sfxSource != null && hoverClip != null && !sfxSource.isPlaying)
            sfxSource.PlayOneShot(hoverClip);
    }
}

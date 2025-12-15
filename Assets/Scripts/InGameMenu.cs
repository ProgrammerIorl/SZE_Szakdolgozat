using UnityEngine;
using UnityEngine.UI;

public class InGameMenu : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject ingameMenuPanel;  // a fõ menü (Resume / Settings / Quit)
    [SerializeField] private GameObject settingsPanel;    // a settings panel

    [Header("Settings UI")]
    [SerializeField] private Toggle companionAutoToggle;  // Automatic / Manual váltó

    public static bool IsPaused { get; private set; } = false;


    private void Start()
    {
        // Menük induláskor legyenek elrejtve
        if (ingameMenuPanel != null) ingameMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!IsPaused)
            {
                OpenMenu();
            }
            else
            {
                ResumeGame();
            }
        }
    }

    // --- Public metódusok a gombokhoz ---

    public void OnResumeButton()
    {
        ResumeGame();
    }

    public void OnQuitButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void OnSettingsButton()
    {
        if (ingameMenuPanel != null) ingameMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
    }

    public void OnSettingsBackButton()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (ingameMenuPanel != null) ingameMenuPanel.SetActive(true);
    }

    // Toggle: Companion actions automatic / manual
    public void OnCompanionAutoToggleChanged(bool isAutomatic)
    {
        // Globális beállítás: minden jelenlegi companionra vonatkozik
        Companion[] companions = FindObjectsOfType<Companion>();

        foreach (var c in companions)
        {
            c.controlMode = isAutomatic
                ? Companion.ControlMode.ML
                : Companion.ControlMode.Manual;
        }

        Debug.Log("Companion control mode set to: " + (isAutomatic ? "Automatic (ML)" : "Manual"));
    }

    // --- Belsõ helper metódusok ---

    private void OpenMenu()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        ingameMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    private void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        ingameMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

}

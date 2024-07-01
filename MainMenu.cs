using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private GameObject notLoading;
    [SerializeField] private GameObject quitPrompt;
    private string MainMenuMusic = "event:/Music/SmoothHands";
    private FMOD.Studio.EventInstance mainMenuMusicInstance;
    private void Awake()
    {
        loadingScreen.SetActive(false);
        quitPrompt.SetActive(false);
        notLoading.SetActive(true);
    }
    private void Start()
    {
        mainMenuMusicInstance = RuntimeManager.CreateInstance(MainMenuMusic);
        mainMenuMusicInstance.start();
    }
    public void LoadMain()
    {
        loadingScreen.SetActive(true);
        notLoading.SetActive(false);
        quitPrompt.SetActive(false);
        StartCoroutine(LoadMainCoroutine());
    }
    public void QuitConfirmPrompt()
    {
        quitPrompt.SetActive(true);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void QuitCancel()
    {
        quitPrompt.SetActive(false);
    }
    private IEnumerator LoadMainCoroutine()
    {
        yield return new WaitForSeconds(0.25f);
        mainMenuMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }
}

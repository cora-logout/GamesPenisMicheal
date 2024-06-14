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
    private void Awake()
    {
        loadingScreen.SetActive(false);
        notLoading.SetActive(true);
    }
    public void LoadMain()
    {
        loadingScreen.SetActive(true);
        notLoading.SetActive(false);
        StartCoroutine(LoadMainCoroutine());
    }

    private IEnumerator LoadMainCoroutine()
    {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
    }
}

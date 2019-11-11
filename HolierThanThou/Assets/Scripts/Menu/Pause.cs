﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pause : MonoBehaviour
{
    public GameObject PauseScreen;
    public GameObject OptionsScreen;
    public GameObject GameUI;

    private bool isPaused;
    AudioManager am;

    private void Start()
	{
		if(this.gameObject.name != "MenuKit")
		{
			Debug.LogError($"You are using an outdated version of the menu UI. \n " +
				$"Get the new menu UI in Resources / Prefabs / Menus, called MenuKit. ");
		}
		isPaused = false;
        Time.timeScale = 1;
        PauseScreen.SetActive(false);
        OptionsScreen.SetActive(false);
        
    }

    private void Update()
    {
        am = FindObjectOfType<AudioManager>();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;

        if (isPaused)
        {
            Time.timeScale = 0;
            PauseScreen.SetActive(true);
            GameUI.SetActive(false);
        }
        else
        {
            Time.timeScale = 1;
            PauseScreen.SetActive(false);
            OptionsScreen.SetActive(false);
            GameUI.SetActive(true);
        }
    }

    public void LoadLevelOnClick(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Rematch()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void ButtonClick()
    {
        if (am != null)
        {
            am.Play("ButtonClick");
        }
    }
}

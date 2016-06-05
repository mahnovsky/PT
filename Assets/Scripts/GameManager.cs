using System;
using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	public		AudioClip		buttonClip;
	public		GameObject		shadow;
	private		GameObject		m_currentPanel;

	public String GameDirectory;

	public static bool Pause { get; set; }

	void Awake ()
	{
		if (Instance == null)
		{
			Instance = this;

			Pause = false;

			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void PlayButtonSound( )
	{
		SoundManager.Instance.PlaySound(buttonClip);
	}

	public void OnEnablePanel( GameObject panel )
	{
		PlayButtonSound();

		panel.SetActive(true);
		shadow.SetActive(true);

		m_currentPanel = panel;

		Pause = true;
	}

	public void OnClosePanel( )
	{
		PlayButtonSound();

		m_currentPanel.SetActive(false);
		shadow.SetActive(false);

		m_currentPanel = null;

		Pause = false;
	}

	public void OnSwapScene( string nextScene )
	{
		PlayButtonSound();

		Application.LoadLevel(nextScene);
	}

	public void OnGameLoad (string gameDir)
	{
		PlayButtonSound();

		GameDirectory = gameDir;

		Application.LoadLevel("Game");
	}
}

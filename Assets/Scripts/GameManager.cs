using System;
using UnityEngine;
using Holoville.HOTween;
using Assets.Scripts;

public interface IScene
{
	void OnButtonPress(string name);

	void Init();

	void Free();

	bool IsLoaded();

	GameObject GetObject();
}

public class GameManager : MonoBehaviour
{
	public static GameManager Instance { get; private set; }

	public		AudioClip		buttonClip;
	public		GameObject		shadow;
	public		AudioClip		swapSound;
	public		GameObject		canvas;
	public		LoadScreen		loadScreen;
	public		Pointf			designSize;
	public		String			GameDirectory;

	public static bool Pause { get; set; }
	private		IScene			m_scene;
	private		IScene			m_currentScene;

	public GameObject CurrentPanel { get; private set; }

	void Awake ()
	{
		if (Instance == null)
		{
			Instance = this;

			Pause = false;

			DontDestroyOnLoad(gameObject);
			
			Camera.main.aspect = designSize.X / designSize.Y;

			HOTween.Init(false, false, true);

			RunScene<Menu>(SceneMove.Left);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void OnButtonPress( string name )
	{
		PlayButtonSound();

		if (m_currentScene != null)
			m_currentScene.OnButtonPress(name);
	}

	public void OnSceneLoaded( )
	{
		m_currentScene = loadScreen.Scene;
	}

	public void RunScene<T>(SceneMove move = SceneMove.None) where T : class, IScene 
	{
		var types = canvas.GetComponentsInChildren(typeof(T), true);
		var scene = types[0] as T;
		if (scene != null)
		{
			loadScreen.LoadScene(scene, move);

			if (m_currentScene != null)
			{
				m_currentScene.Free();
				m_currentScene = null;
			}
		}
	}

	public void PlayButtonSound( )
	{
		SoundManager.Instance.PlaySound(buttonClip);
	}

	public void OnEnablePanel( GameObject panel )
	{
		panel.SetActive(true);
		shadow.SetActive(true);

		CurrentPanel = panel;

		Pause = true;
	}

	public void OnClosePanel( )
	{
		CurrentPanel.SetActive(false);
		shadow.SetActive(false);

		CurrentPanel = null;

		Pause = false;
	}

	public void OnGameLoad (string gameDir)
	{
		PlayButtonSound();

		GameDirectory = gameDir;

		if (GameController.Instance != null)
			GameController.Instance.OnSceneSwap();

		Application.LoadLevel("Game");
	}
}

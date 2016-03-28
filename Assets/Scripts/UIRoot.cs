using UnityEngine;
using System.Collections;

public class UIRoot : MonoBehaviour
{
	private GameObject m_currentPanel;

	public AudioClip buttonClip;
	public GameObject shadow;

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
	}

	public void OnClosePanel( )
	{
		PlayButtonSound();

		m_currentPanel.SetActive(false);
		shadow.SetActive(false);

		m_currentPanel = null;
	}

	public void OnSwapScene( string nextScene )
	{
		PlayButtonSound();

		Application.LoadLevel(nextScene);
	}
}

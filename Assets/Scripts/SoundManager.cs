using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
	public AudioSource musicSource;
	public AudioSource soundSource;
	private bool m_soundMute;

	public static SoundManager Instance { get; private set; }

	void Awake ()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
		}
		else
		{
			DontDestroyOnLoad(gameObject);
			Instance = this;
		}
	}

	public void PlaySound(AudioClip clip)
	{
		soundSource.clip = clip;

		soundSource.Play();
	}

	public void TurnMusic(bool on)
	{
		musicSource.mute = !on;
	}
	public void TurnSound(bool on)
	{
		soundSource.mute = !on;
	}
}

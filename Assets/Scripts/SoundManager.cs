using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
	public AudioSource musicSource;
	public AudioSource soundSource;

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
}

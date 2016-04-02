using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class UIOption : MonoBehaviour
{
	public void OnSoundChange(Toggle tog)
	{
		SoundManager.Instance.TurnSound(tog.isOn);
	}

	public void OnMusicChange( Toggle tog )
	{
		SoundManager.Instance.TurnMusic(tog.isOn);
	}

	public void OnClose( )
	{
		GameManager.Instance.OnClosePanel();
	}
}

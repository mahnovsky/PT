using UnityEngine;
using System.Collections;

public class Menu : MonoBehaviour
{ 
	// Use this for initialization
	public void OnGameLoad ()
	{
		Application.LoadLevel("Game");
	}

	public void OnArcadeClick( )
	{
		Application.LoadLevel("LevelsList");
	}
}

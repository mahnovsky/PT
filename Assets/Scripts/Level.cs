using UnityEngine;
using System.Collections;

public abstract class Level : MonoBehaviour {

	public enum Mode 
	{
		Classic,
		MoveItem
	}

	public Sprite [] coinSprites;
	protected Mode m_mode;

	public Mode LevelMode 
	{
		get { return m_mode; }
		protected set { m_mode = value; }
	}

	public int maxCoinsCount() 
	{
		return coinSprites.Length;
	}

	public static Level currLevel() 
	{
		if (Game.Inst == null) 
		{
			return null;
		}

		return Game.Inst.CurrentLevel;
	}

	public void loadNextLevel() 
	{
		++Game.LevelNum;
		Application.LoadLevel ("Game");
	}

	public abstract void init();
	public abstract GameStrategy getStrategy(Map map);
}

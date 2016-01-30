using UnityEngine;
using System.Collections;

public abstract class Level : MonoBehaviour {

	public enum Mode
	{
		Classic,
		MoveItem
	}

	public Sprite []	coinSprites;
	public Point[]		disabledCoins;
	protected Mode		m_mode;

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
		if (GameController.Instance == null)
		{
			return null;
		}

		return GameController.Instance.CurrentLevel;
	}

	public void loadNextLevel()
	{
		++GameController.LevelNum;
		Application.LoadLevel ("Game");
	}

	public abstract void init();
	public abstract GameStrategy getStrategy(Map map);
}


public class LevelInfo {

	bool locked;
	int reward;
}
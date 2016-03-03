using UnityEngine;
using System.Collections;

public class MoveItemLevel : Level
{ 
	public int topInX;
	public int bottomOutX;
	public Sprite item;

	int			m_coinId;
	int			m_inIndex;
	int			m_outIndex;
	Sprite		m_coinSprite;


	// Use this for initialization
	public override void Init ()
	{
		LevelMode = Mode.MoveItem;
		var map = GameController.Instance.map;
		m_inIndex = map.posToIndex (topInX, map.Height - 1);
		m_outIndex = map.posToIndex (bottomOutX, 0);
		m_coinId = GameController.CoinSprites.Length + 1;
	}

	public override Coin CoinForIndex(bool init, int index)
	{
		var map = GameController.Instance.map;
		if (init && index == m_inIndex)
		{
			Level currLevel = GameController.CurrentLevel;
			
			return map.createCoin (index, GameController.CoinSprites.Length + 1, m_coinSprite);
		}
		
		return map.createRandomCoin (index);
	}

	public override void OnCoinsSwap (Coin c1, Coin c2)
	{
		if (c1.CoinId == m_coinId && c1.PlaceId == m_outIndex)
		{
			//GameController.Instance.CurrentLevel.loadNextLevel();
			GameController.Instance.OnLevelEnd();
		}

		if (c2.CoinId == m_coinId && c2.PlaceId == m_outIndex) {
			
			//Level.currLevel().loadNextLevel();
		}
	}

	public override void OnCoinMove (Coin c)
	{
		if (c.CoinId == m_coinId && c.YPos == 0) {
			
			//GameController.CurrentLevel.loadNextLevel();
		}
	}
}

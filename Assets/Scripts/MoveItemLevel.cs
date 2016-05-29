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

	public override Coin CoinForIndex(bool init, int index)
	{
		var map = GameController.Instance.board;
		if (init && index == m_inIndex)
		{
			Level currLevel = GameController.CurrentLevel;
			
			return map.CreateCoin (index, GameController.CoinSprites.Length + 1, m_coinSprite);
		}
		
		return map.CreateRandomCoin (index);
	}

	public override void OnCoinsSwap (Coin c1, Coin c2)
	{
		if (c1.CoinId == m_coinId && c1.PlaceId == m_outIndex)
		{
			//GameController.Instance.CurrentLevel.loadNextLevel();
			GameController.Instance.OnLevelWin();
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

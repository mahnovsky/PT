using UnityEngine;
using System.Collections;

public class MoveItemStrategy : GameStrategy {
	
	public MoveItemStrategy(Map map, int coinId, int inIndex, int outIndex, Sprite sp) :base(map) {
		m_inIndex = inIndex;
		m_outIndex = outIndex;
		m_coinId = coinId;
		m_coinSprite = sp;
	}
	
	public override Coin coinForIndex(bool init, int index) {
		if (init && index == m_inIndex) {
			GameController controller = GameObject.Find ("GameController").GetComponent<GameController>();
			Level currLevel = controller.CurrentLevel;
			
			return GameMap.createCoin (index, currLevel.maxCoinsCount() + 1, m_coinSprite);
		}
		
		return GameMap.createRandomCoin (index);
	}

	public override void onCoinsSwap (Coin c1, Coin c2) {

		if (c1.CoinId == m_coinId && c1.PlaceId == m_outIndex) {

			Level.currLevel().loadNextLevel();
		}

		if (c2.CoinId == m_coinId && c2.PlaceId == m_outIndex) {
			
			Level.currLevel().loadNextLevel();
		}
	}

	public override void onCoinMove (Coin c) {
		if (c.CoinId == m_coinId && c.YPos == 0) {
			
			Level.currLevel().loadNextLevel();
		}
	}

	int m_coinId;
	int m_inIndex;
	int m_outIndex;
	Sprite m_coinSprite;
}

public class MoveItemLevel : Level {

	public int topInX;
	public int bottomOutX;
	public Sprite item;


	// Use this for initialization
	public override void init () {
		LevelMode = Mode.MoveItem;
	}

	public override GameStrategy getStrategy(Map map) {

		int inIndex = map.posToIndex (topInX, map.Height - 1);
		int outIndex = map.posToIndex (bottomOutX, 0);
		int coinId = maxCoinsCount () + 1;

		return new MoveItemStrategy(map, coinId, inIndex, outIndex, item);
	}
}

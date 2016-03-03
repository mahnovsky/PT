using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClassicLevel : Level
{
	private int m_moveCount;

	public ClassicLevel()
		:base()
	{
	}

	public override void Init()
	{
		LevelMode = Mode.Classic;

		m_moveCount = 10;
	}

	public override  Coin CoinForIndex(bool init, int index)
	{	
		return GameController.CurrentLevel.CreateRandomCoin (index);
	}

	public override void OnMatch(int cid, int count)
	{
		--m_moveCount;

		if (m_moveCount <= 0)
		{
			GameController.Instance.OnLevelEnd();
		}
	}

	public override void InitParser(JsonParser parser)
	{
		parser.AddFunc("moveCount", (s, o) =>
		{
			m_moveCount = (int)o.n;
		});	
	}
}

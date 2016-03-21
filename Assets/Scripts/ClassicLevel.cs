using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClassicLevel : Level
{
	public int MaxMoveCount { get; set; }
	public int MoveCount { get; set; }

	public override void Init()
	{
		LevelMode = Mode.Classic;

		MoveCount = 10;
	}

	public override  Coin CoinForIndex(bool init, int index)
	{	
		return GameController.CurrentLevel.CreateRandomCoin (index);
	}

	public override void OnMatch(int cid, int count)
	{
		--MoveCount;
	}

	public override void OnBoardStable( )
	{
		if ( MoveCount <= 0 )
		{
			GameController.Instance.OnLevelEnd ( );
		}
	}

	public override void Refresh()
	{
		MoveCount = MaxMoveCount;
	}

	public override void InitParser(JsonParser parser)
	{
		parser.AddFunc("moveCount", (s, o) =>
		{
			MaxMoveCount = (int)o.n;
			Refresh();
		});	
	}

	protected override void SaveInherit(JSONObject root)
	{
		root.AddField("moveCount", MaxMoveCount);
	}
}

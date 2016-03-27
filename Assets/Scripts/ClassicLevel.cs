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
	}

	public override  Coin CoinForIndex(bool init, int index)
	{	
		return base.CreateRandomCoin (index);
	}

	public override void OnCoinsSwap(Coin c1, Coin c2)
	{
		base.OnCoinsSwap (c1, c2);

		--MoveCount;
	}

	public override void OnBoardStable( )
	{
		Debug.Log ("OnBoardStable mc / maxmc: " + MoveCount + " / " + MaxMoveCount);
		if ( MoveCount <= 0 )
		{
			GameController.Instance.OnLevelEnd ( );
		}
	}

	public override void Refresh()
	{
		base.Refresh ();

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

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Assets;
using Assets.Scripts;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Level
{
	public enum Mode
	{
		Classic,
		MoveItem
	}

	public List<Point> DisabledCells { get; set; }
	public int Number { get; set; }
	public int Score { get; protected set; }

	public ScoreCounter ScoreCounter { get; set; }

	public Action<int> OnScoreUpdate { get; set; }

	public Mode LevelMode { get; protected set; }

	public Level()
	{
		DisabledCells = new List<Point>();
		ScoreCounter = new ScoreCounter();
	}

	public virtual void Init()
	{
	}

	public virtual void Refresh()
	{
		Score = 0;
	}

	public virtual void OnBoardStable( )
	{ }

	public virtual Coin CoinForIndex(bool init, int index)
	{
		return null;
	}

	public virtual void OnCoinsSwap (Coin c1, Coin c2)
	{}

	public virtual void OnCoinMove (Coin c)
	{ }

	public virtual void OnMatch( List<Coin> coins )
	{
		int total = 5 * coins.Count;
		ScoreCounter.AddScore(total);

		int index = Mathf.FloorToInt( (float)coins.Count / 2 );
		var pos = Camera.main.WorldToScreenPoint(coins[index].transform.position);
		PopupLabelGenerator.Instance.Print(
			total.ToString(), pos, Vector2.up * 100, 2f, 1f);

		if (coins.Count == 4)
		{
			var fa = coins[index].transform.GetComponentsInChildren<FrameAnimator>(true);
			foreach (var frameAnimator in fa)
			{
				frameAnimator.gameObject.SetActive(true);
			}
		}
	}

	public Coin CreateRandomCoin( int index )
	{
		return GameController.Instance.map.createRandomCoin(index);
	}

	public bool Load(JSONObject obj)
	{
		var parser = new JsonParser();

		parser.AddFunc("disabledCells", ((key, ob) =>
		{
			var pointParser = new JsonParser();

			pointParser.AddFunc("x", (s, o) =>
			{
				DisabledCells.Last().X = (int)o.n;
			});

			pointParser.AddFunc("y", (s, o) =>
			{
				DisabledCells.Last().Y = (int)o.n;
			});

			pointParser.OnInit = o =>
			{
				DisabledCells.Add(new Point());
			};

			parser.Anonymous = pointParser;

			parser.ParseArray(ob);
		}));

		InitParser(parser);

		parser.ParseObject(obj);

		return true;
	}

	public virtual void InitParser(JsonParser parser)
	{}

	protected virtual void SaveInherit(JSONObject root)
	{}

	public void Save( JSONObject root )
	{
		JSONObject level = new JSONObject(JSONObject.Type.OBJECT);
		JSONObject dc = new JSONObject(JSONObject.Type.ARRAY);

		foreach (var disabledCell in DisabledCells)
		{
			var point = new JSONObject(JSONObject.Type.OBJECT);

			point.AddField("x", disabledCell.X);
			point.AddField("y", disabledCell.Y);

			dc.Add(point);
		}

		level.AddField("disabledCells", dc);

		SaveInherit( level );

		root.AddField("level", level);
	}
}

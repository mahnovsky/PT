using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.Assertions;
using UnityEngine.UI;

public class Level : MonoBehaviour
{
	public enum Mode
	{
		Classic,
		MoveItem
	}

	public List<Point> DisabledCells { get; set; }
	public int Number { get; set; }

	public Mode LevelMode { get; protected set; }

	public Level()
	{
		DisabledCells = new List<Point>();
	}

	public virtual  void Init( )
	{
	}

	public virtual Coin CoinForIndex(bool init, int index)
	{
		return null;
	}

	public virtual void OnCoinsSwap (Coin c1, Coin c2)
	{}

	public virtual void OnCoinMove (Coin c)
	{ }

	public virtual void OnMatch(int cid, int count)
	{ }

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
	{
		
	}
}

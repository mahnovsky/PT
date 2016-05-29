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

public class LevelComponent
{
	public virtual void Init (JSONObject obj) {}

	public virtual void Free() {}
}

class MoveCounter : LevelComponent
{
	private Level m_level;

	public int TotalMoves { get; set; }
	public int Moves { get; private set; }

	public override void Init(JSONObject obj)
	{
		var board = GameController.Instance.board;

		board.OnCoinsSwap += OnCoinsSwap;

		JSONObject mc = obj.GetField ("moveCount");

		if (mc != null) 
		{
			TotalMoves = (int)mc.n;
			Moves = TotalMoves;
		}

		Debug.Log ("Hello From MoveCounter");
	}

	public override void Free()
	{
		var board = GameController.Instance.board;

		board.OnCoinsSwap -= OnCoinsSwap;
	}

	void OnCoinsSwap(Coin c1, Coin c2)
	{
		--Moves;

		if (Moves <= 0) 
		{
			GameController.Instance.OnLevelFail ();
		}
	}
}

public class Level
{
	public List<Point> DisabledCells { get; set; }
	public int Number { get; set; }
	public int Score { get; protected set; }
	public float TotalTime { get; protected set; }

	public ScoreCounter ScoreCounter { get; set; }

	public Action<int> OnScoreUpdate { get; set; }

	private Dictionary<string, LevelComponent> m_components;
	private Dictionary<string, Func<LevelComponent>> m_componentCreators;

	public Level()
	{
		DisabledCells = new List<Point>();
		ScoreCounter = new ScoreCounter();
		m_components = new Dictionary<string, LevelComponent> ();
		m_componentCreators = new Dictionary<string, Func<LevelComponent>> ();
		RegistryComponent<MoveCounter> ();
	}

	public void RegistryComponent<T>() where T : LevelComponent, new()
	{
		m_componentCreators.Add (typeof(T).Name, () => new T ());
	}

	public T GetComponent<T>() where T : LevelComponent
	{
		LevelComponent comp;
		if (m_components.TryGetValue (typeof(T).Name, out comp)) 
		{
			return (T)comp;
		}

		return null;
	}

	public virtual void Refresh()
	{
		Score = 0;
	}

	public virtual Coin CoinForIndex(bool init, int index)
	{
		return CreateRandomCoin(index);
	}

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
		return GameController.Instance.board.CreateRandomCoin(index);
	}

	public bool Load(JSONObject obj)
	{
		for ( int i = 0; i < obj.list.Count; ++i )
		{
			string ckey = (string) obj.keys[i];
			JSONObject j = (JSONObject) obj.list[i];

			if (j.type == JSONObject.Type.OBJECT) 
			{
				Func<LevelComponent> creator;
				if (m_componentCreators.TryGetValue (ckey, out creator)) 
				{
					var comp = creator.Invoke ();

					comp.Init (j);

					m_components.Add (ckey, comp);
				}
			}
		}

		return true;
	}

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

		root.AddField("level", level);
	}
}

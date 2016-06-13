using System;
using UnityEngine;
using System.Collections.Generic;
using Assets;
using Assets.Scripts;
using Assets.Scripts.Utils;

public class MoveCounter : EntityComponent
{
	public int TotalMoves { get; set; }
	public int Moves { get; private set; }

	public override void Load( JSONObject obj )
	{
		JSONObject mc = obj.GetField ("moveCount");

		if (mc != null) 
		{
			TotalMoves = (int)mc.n;
			Moves = TotalMoves;
		}
	}

	public override void Init()
	{
		var board = GameController.Instance.board;

		board.OnCoinsSwap += OnCoinsSwap;

		Debug.Log ("Hello From MoveCounter");
	}

	public override void Free()
	{
		var board = GameController.Instance.board;

		board.OnCoinsSwap -= OnCoinsSwap;
	}

	public override void Refresh() 
	{
		Moves = TotalMoves;
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

public class LevelTimer : EntityComponent
{
	private bool m_levelDone;
	public float TotalTime { get; set; }
	public float Time { get; private set; }

	public void OnMatch( List<Coin> coins )
	{
		Time += coins.Count * 0.2f;
	}

	public override void Load( JSONObject obj )
	{
		JSONObject mc = obj.GetField ("time");

		if (mc != null) 
		{
			TotalTime = (int)mc.n;
			Time = TotalTime;
		}
	}

	public override void Init()
	{
		GameController.Instance.OnUpdate += Update;
		GameController.Instance.board.OnMatch += OnMatch;

		GameController.Instance.timeBar.gameObject.SetActive (true);

		m_levelDone = false;
	}

	public override void Free()
	{
		GameController.Instance.OnUpdate -= Update;
		GameController.Instance.board.OnMatch -= OnMatch;
	}

	public override void Refresh() 
	{
		Time = TotalTime;
		m_levelDone = false;
	}
		
	public void Update()
	{
		if (!GameManager.Pause)
			Time -= UnityEngine.Time.deltaTime;

		if (Time <= 0 && !m_levelDone) 
		{
			GameController.Instance.OnLevelFail ();
			m_levelDone = true;
		}
		if (!m_levelDone)
		{
			GameController.Instance.timeBar.fillAmount = Time / TotalTime;
		}
	}
}

public class LevelSound : EntityComponent
{
	void OnCoinsSwap(Coin c1, Coin c2)
	{
		var clip = GameManager.Instance.swapSound;

		SoundManager.Instance.PlaySound(clip);
	}

	public override void Init( )
	{
		GameController.Instance.board.OnCoinsSwap += OnCoinsSwap;
	}

	public override void Free()
	{
		GameController.Instance.board.OnCoinsSwap -= OnCoinsSwap;
	}
}

public class Level : Entity
{
	public List<Point> 	DisabledCells { get; set; }
	public int 			Number { get; set; }
	public int 			Score { get; protected set; }
	public float 		TotalTime { get; protected set; }
	private bool		m_levelDone = false;

	public Action<int> OnScoreUpdate { get; set; }

	public Level() : base()
	{
		DisabledCells = new List<Point>();

		RegistryComponent<MoveCounter> ();
		RegistryComponent<LevelTimer> ();
		RegistryComponent<ScoreCounter> ();
		AddComponent<LevelSound>();
	}

	public override void Refresh()
	{
		Score = 0;

		base.Refresh();
	}

	public virtual Coin CoinForIndex(bool init, int index)
	{
		return CreateRandomCoin(index);
	}

	public Coin CreateRandomCoin( int index )
	{
		return GameController.Instance.board.CreateRandomCoin(index);
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

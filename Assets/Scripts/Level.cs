using System;
using UnityEngine;
using System.Collections.Generic;
using Assets.Scripts;
using Assets.Scripts.Utils;

namespace Assets.Scripts
{
	public class MoveCounter : EntityComponent
	{
		public int TotalMoves { get; set; }
		public int Moves { get; private set; }

		public override void Load(JSONObject obj)
		{
			JSONObject mc = obj.GetField("moveCount");

			if (mc != null)
			{
				TotalMoves = (int) mc.n;
				Moves = TotalMoves;
			}
		}

		public override void Init(Entity owner)
		{
			base.Init(owner);

			var board = GameController.Instance.board;

			board.OnCoinsSwap += OnCoinsSwap;
			board.OnBoardStable += OnBoardStable;

			var controller = GameController.Instance;
	
			controller.movesPanel.gameObject.SetActive(true);

			Debug.Log("Hello From MoveCounter");
		}

		public override void Free()
		{
			var board = GameController.Instance.board;

			board.OnCoinsSwap -= OnCoinsSwap;
			board.OnBoardStable -= OnBoardStable;

			GameController.Instance.movesPanel.gameObject.SetActive(false);
		}

		public override void Refresh()
		{
			Moves = TotalMoves;
			GameController.Instance.MovesCountText.text = Moves.ToString();
		}
		
		private void OnCoinsSwap(Coin c1, Coin c2)
		{
			--Moves;
			GameController.Instance.MovesCountText.text = Moves.ToString();
		}

		void OnBoardStable()
		{
			if (Moves <= 0)
			{
				GameController.Instance.OnLevelFail();
			}
		}
	}

	public class LevelTimer : EntityComponent
	{
		private bool m_levelDone;
		public float TotalTime { get; set; }
		public float Time { get; private set; }

		public void OnMatch(List<Coin> coins)
		{
			Time += coins.Count*0.2f;
		}

		public override void Load(JSONObject obj)
		{
			JSONObject mc = obj.GetField("time");

			if (mc != null)
			{
				TotalTime = (int) mc.n;
				Time = TotalTime;
			}
		}

		private void OnBoardStable()
		{
			if (m_levelDone)
			{
				GameController.Instance.OnLevelFail();
			}
		}

		public override void Init(Entity owner)
		{
			base.Init(owner);
			var controller = GameController.Instance;
			controller.OnUpdate += Update;
			controller.board.OnMatch += OnMatch;
			controller.board.OnBoardStable += OnBoardStable;
			controller.timeBar.gameObject.SetActive(true);

			m_levelDone = false;

			Refresh();
		}

		public override void Free()
		{
			var controller = GameController.Instance;
			controller.OnUpdate -= Update;
			controller.board.OnMatch -= OnMatch;
			controller.board.OnBoardStable -= OnBoardStable;
			controller.timeBar.gameObject.SetActive(true);
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
				m_levelDone = true;
			}
			if (!m_levelDone)
			{
				GameController.Instance.timeBar.fillAmount = Time/TotalTime;
			}
			else if (GameController.Instance.board.IsStable)
			{
				GameController.Instance.OnLevelFail();
			}
		}
	}

	public class LevelSound : EntityComponent
	{
		private void OnCoinsSwap(Coin c1, Coin c2)
		{
			var clip = GameManager.Instance.swapSound;

			SoundManager.Instance.PlaySound(clip);
		}

		public override void Init(Entity owner)
		{
			base.Init(owner);

			GameController.Instance.board.OnCoinsSwap += OnCoinsSwap;
		}

		public override void Free()
		{
			GameController.Instance.board.OnCoinsSwap -= OnCoinsSwap;
		}
	}

	public class Level : Entity
	{
		public int Number { get; set; }
		public int Score { get; protected set; }
		public float TotalTime { get; protected set; }
		private bool m_levelDone = false;

		public Action<int> OnScoreUpdate { get; set; }

		public Level() : base()
		{
			RegistryComponent<MoveCounter>();
			RegistryComponent<LevelTimer>();
			RegistryComponent<ScoreCounter>();
			RegistryComponent<CellsInfo>();
			RegistryComponent<GoalController>();
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

		public Coin CreateRandomCoin(int index)
		{
			return GameController.Instance.board.CreateRandomCoin(index);
		}

		public void Save(JSONObject root)
		{
			JSONObject level = new JSONObject(JSONObject.Type.OBJECT);
			/*JSONObject dc = new JSONObject(JSONObject.Type.ARRAY);
		
		foreach (var disabledCell in DisabledCells)
		{
			var point = new JSONObject(JSONObject.Type.OBJECT);

			point.AddField("x", disabledCell.X);
			point.AddField("y", disabledCell.Y);

			dc.Add(point);
		}

		level.AddField("disabledCells", dc);*/

			root.AddField("level", level);
		}
	}

}

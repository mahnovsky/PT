using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Utils;

namespace Assets.Scripts
{
	public class CellsInfo : EntityComponent
	{
		public class GoalWindowInfo :IJsonReader
		{
			public Point	pos;
			public int		level;

			public void Read(JSONObject obj)
			{
				pos = new Point();
				pos.Read(obj);
				level = obj.GetInt("level");
			}
		}

		public List<Point> Disabled
		{
			get; private set;
		}
		public List<GoalWindowInfo> GoalWindows
		{
			get; private set;
		}

		public override void Load( JSONObject obj )
		{
			Disabled = obj.GetArray<Point>("disabled");
			GoalWindows = obj.GetArray<GoalWindowInfo> ("goalWindows");
		}

		public void InitCells( Board board )
		{
			foreach ( Point point in Disabled )
			{
				Cell cell = board.GetCell ( point.X, point.Y );

				if ( cell != null )
				{
					cell.Empty = true;
					cell.gameObject.SetActive ( false );
				}
			}

			foreach ( var g in GoalWindows )
			{
				Cell cell = board.GetCell ( g.pos.X, g.pos.Y );

				if ( cell != null )
				{
					cell.GoalLevel = g.level;
				}
			}
		}
	}

	class GoalController : EntityComponent
	{
		private int		m_windows;
		private int		m_needScore;
		private Board	m_board;

		public int TotalWindows { get; private set; }
		public int TotalScore   { get; private set; }

		public event Action<int> OnWindowsChange; 

		void OnCoinDestroy( Coin coin )
		{
			Cell c = m_board.GetCell ( coin.PlaceId );
			if ( c == null )
				return;

			if ( c.GoalLevel > 0 )
			{
				c.GoalLevel = c.GoalLevel - 1;
				--m_windows;
				if ( OnWindowsChange != null )
				{
					OnWindowsChange.Invoke(TotalWindows - m_windows);
				}
			}
		}

		void OnBoardStable()
		{
			var sc = OwnerEntity.GetComponent<ScoreCounter>();
			if (m_windows <= 0 && sc.Score >= m_needScore)
			{
				GameController.Instance.OnLevelWin();
			}
		}

		public override void Refresh()
		{
			m_windows = TotalWindows;
			m_needScore = TotalScore;
			if ( OnWindowsChange != null )
			{
				OnWindowsChange.Invoke ( TotalWindows - m_windows );
			}
		}

		public override void Start()
		{
			m_board = GameController.Instance.board;

			m_board.OnCoinDestroy	+= OnCoinDestroy;
			m_board.OnBoardStable	+= OnBoardStable;

			Refresh();
		}

		public override void Free( )
		{
			m_board.OnCoinDestroy	-= OnCoinDestroy;
			m_board.OnBoardStable	-= OnBoardStable;
		}

		public override void Load(JSONObject obj)
		{
			TotalScore		= obj.GetInt("needScore");
			TotalWindows	= obj.GetInt("windows");
		}
	}
}

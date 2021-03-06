using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Assets.Scripts;
using Assets.Scripts.Utils;
using UnityEngine.Assertions;

namespace Assets.Scripts
{

	public enum Axis
	{
		None	= 0,
		X		= 1,
		Y		= 2
	}

	public class Match
	{
		public Axis Axis { get; set; }
		public List<Coin> Coins { get; private set; }
		public int Id { get; set; }

		public Match( )
		{
			Axis = Axis.None;
			Coins = new List<Coin>();
		}

		public void Free()
		{
			if (Coins != null)
				Coins.Clear();
		}

		public void Add(Coin c)
		{
			if (c.MatchID != Id)
			{
				c.MatchID = Id;
				Coins.Add(c);
			}
		}
	}

	public class Board : MonoBehaviour
	{
		private ObjectPool<Coin>	m_coinsPool;
		private ObjectPool<Match>	m_matchPool; 

		public Cell					cellPrefab;
		public Coin					coinPrefab;
		public float				dropSpeed;
		public float				swapSpeed;
		public int Width { get; private set; }
		public int Height { get; private set; }

		public event Action<Coin, Coin> OnCoinsSwap;
		public event System.Action OnBoardStable;
		public event Action<List<Coin>> OnMatch;
		public event Action<Coin> OnCoinDestroy;

		// private section
		private Coin m_selected;
		private float m_dieDelay = 1.2f;
		private int[] m_lines;
		private Vector3 m_spawnPosition;
		private Vector2 m_size;
		private int m_notStableCoins;

		private List<Match> m_matches = new List<Match>();
		private Point[] m_axisDirection;

		public bool IsBoardInit { get; private set; }

		public bool IsStable
		{
			get { return m_notStableCoins == 0; }
		}

		public float DieDelay
		{
			get { return m_dieDelay; }
		}

		public Vector3 GetRealPosition(int x, int y)
		{
			Vector3 pos = new Vector3(Cell.Width*x, Cell.Height*y);

			return pos;
		}

		public Cell GetCell(int x, int y)
		{
			if (x < 0 || y < 0 || x >= Width || y >= Height)
			{
				return null;
			}

			return GetCell(PosToIndex(x, y));
		}

		public Cell GetCell(int index)
		{
			if (index >= 0 && index < Greed.Length)
			{
				return Greed[index];
			}

			return null;
		}

		private void CreateGreed()
		{
			Greed = new Cell[Count];

			for (int i = 0; i < Count; ++i)
			{
				var cell = Instantiate(cellPrefab);

				Point pos = IndexToPos(i);

				cell.Init(i, pos, false);

				cell.transform.parent = transform;

				cell.transform.localPosition = GetRealPosition(pos.X, pos.Y);

				Greed[i] = cell;
			}

			for (int i = 0; i < Count; ++i)
			{
				var cell = Greed[i];

				cell.SetNeighbour(NeighbourPos.Left, GetCell(cell.Position.X - 1, cell.Position.Y));
				cell.SetNeighbour(NeighbourPos.Right, GetCell(cell.Position.X + 1, cell.Position.Y));
				cell.SetNeighbour(NeighbourPos.Top, GetCell(cell.Position.X, cell.Position.Y + 1));
				cell.SetNeighbour(NeighbourPos.Bottom, GetCell(cell.Position.X, cell.Position.Y - 1));
			}


			var cellsInfo = GameController.CurrentLevel.GetComponent<CellsInfo>();
			if (cellsInfo != null)
			{
				cellsInfo.InitCells(this);
			}
		}

		Match MakeMatch()
		{
			Match m = m_matchPool.MakeNew();

			m.Id = m_matches.Count;
			m_matches.Add(m);

			return m;
		}

		Match GetMatch( int id )
		{
			return m_matches[id];
		}

		//public section
		public void Initialize(int w, int h)
		{
			print("map init w, h: " + w + ", " + h);

			m_lines = new int[w];
			m_spawnPosition.y = (Cell.Height*h) + 20f;
			m_spawnPosition.x = 0;
			Width = w;
			Height = h;
			Count = w*h;
			m_coinsPool = new ObjectPool<Coin>()
			{
				MakeFunc = () => Instantiate(coinPrefab),
				DisableFunc = (inst) =>
				{
					if (OnCoinDestroy != null && !IsBoardInit)
						OnCoinDestroy.Invoke(inst);
				}
			};

			m_matchPool = new ObjectPool<Match>()
			{
				MakeFunc = () => new Match(),
				DisableFunc = (inst) => inst.Free()
			};

			m_axisDirection = new Point[3]
			{
				null,
				new Point(1, 0),
				new Point(0, 1) 
			};

			RemoveList = new List<Coin>();

			CreateGreed();

			Fill();
		}

		public void Refresh()
		{
			foreach (Cell cell in Greed)
			{
				if (cell.Empty)
				{
					cell.Empty = false;
					cell.gameObject.SetActive(true);
				}
			}

			var cellsInfo = GameController.CurrentLevel.GetComponent<CellsInfo>();
			if (cellsInfo != null)
			{
				cellsInfo.InitCells(this);
			}

			Fill();
		}

		public int Count { get; private set; }

		public Coin GetCoin(int x, int y)
		{
			return GetCell(PosToIndex(x, y)).CoinRef;
		}

		public Coin GetCoin(int index)
		{
			return GetCell(index).CoinRef;
		}

		public int PosToIndex(int x, int y)
		{
			if (x < 0 || y < 0 || x >= Width || y >= Height)
			{
				return -1;
			}

			return x + y*Width;
		}

		public int PosToIndex(Point pos)
		{
			return PosToIndex(pos.X, pos.Y);
		}

		public Point IndexToPos(int index)
		{
			return new Point(index%Width, index/Width);
		}

		public Coin Select { get; set; }
		public Coin Focused { get; set; }

		public int[] Lines
		{
			get { return m_lines; }
			set { m_lines = value; }
		}

		public List<Coin> RemoveList { get; private set; }

		public Cell[] Greed { get; private set; }

		private int m_busy;

		public void OnMoveDone(Coin coin, string msg)
		{
			if (coin.State != eCoinState.Idle)
				return;
			Cell cell = GetCell(coin.PlaceId);
			cell.CoinRef = coin;

			if (msg == "failSwap")
			{
				coin.MoveToSpeedBased(coin.GetRealPosition(), 0, swapSpeed, "doneFailSwap");
			}
			else if (msg != "doneFailSwap" && m_busy == 0)
			{
				CheckAll(0);
			}
		}

		public void Swap(Coin c1, Coin c2)
		{
			if (Lines[c1.XPos] > 0 || Lines[c2.XPos] > 0)
				return;

			Vector3 pos1 = c1.transform.localPosition;
			Vector3 pos2 = c2.transform.localPosition;

			if (TrySwap(c1, c2))
			{
				ApplySwap(c1, c2);
				print("create move coins");

				c1.MoveToSpeedBased(pos2, 0, swapSpeed, "");
				c2.MoveToSpeedBased(pos1, 0, swapSpeed, "");
			}
			else
			{
				c1.MoveToSpeedBased(pos2, 0, swapSpeed, "failSwap");
				c2.MoveToSpeedBased(pos1, 0, swapSpeed, "failSwap");
			}
		}

		public bool TrySwap(Coin c1, Coin c2)
		{
			Focused = null;

			GetCell(c1.PlaceId).CoinRef = c2;
			GetCell(c2.PlaceId).CoinRef = c1;

			bool res = false;

			Point count = CountNearCoins(c1.Position);
			if (count.X > 1 || count.Y > 1)
			{
				res = true;
			}

			count = CountNearCoins(c2.Position);
			if (count.X > 1 || count.Y > 1)
			{
				res = true;
			}

			GetCell(c1.PlaceId).CoinRef = c1;
			GetCell(c2.PlaceId).CoinRef = c2;

			return res;
		}

		private void ApplySwap(Coin c1, Coin c2)
		{
			Point pos = c1.Position;
			c1.UpdateLoc(c2.PlaceId, c2.XPos, c2.YPos);
			c2.UpdateLoc(PosToIndex(pos.X, pos.Y), pos.X, pos.Y);

			if (OnCoinsSwap != null)
				OnCoinsSwap.Invoke(c1, c2);
		}

		public Coin CreateRandomCoin(int placeId)
		{
			print("[Board:CreateRandomCoin]");

			int maxCoin = GameController.CoinSprites.Length;

			int rndCoin = UnityEngine.Random.Range(0, maxCoin);

			return CreateCoin(placeId, rndCoin, null);
		}

		private void CoinStateChange(StateMachine<eCoinState> sender, eCoinState prevState, eCoinState currState)
		{
			Coin coin = sender.UserData as Coin;

			if (coin == null)
				return;

			if (currState == eCoinState.MarkDelete)
			{
				++Lines[coin.XPos];
			}

			if (prevState == eCoinState.Moved)
				--m_busy;
			if (currState == eCoinState.Moved)
				++m_busy;

			if (prevState == eCoinState.Idle)
				++m_notStableCoins;
			if (currState == eCoinState.Idle)
				--m_notStableCoins;
		}

		public void FreeCoin(Coin c)
		{
			m_coinsPool.Delete(c);

			c.State = eCoinState.Idle;
		}

		public Coin CreateCoin(int placeId, int coinId, Sprite sp)
		{
			Cell cell = GetCell(placeId);
			if (cell.Empty)
			{
				return null;
			}

			Point pos = IndexToPos(placeId);

			Coin coin = m_coinsPool.MakeNew();

			coin.transform.parent = transform;
			if (sp == null)
			{
				sp = GameController.CoinSprites[coinId];
			}

			coin.Init(placeId, pos.X, pos.Y, coinId, sp);
			coin.StateMachine.OnStateChange = CoinStateChange;

			GetCell(placeId).CoinRef = coin;

			return coin;
		}

		public void MoveCoinsDown(int x)
		{
			int coins = 0;
			for (int y = 0; y < Height; ++y)
			{
				Cell cell = GetCell(x, y);
				if (cell.Empty)
					continue;

				if (cell.CoinRef == null)
				{
					++coins;
					continue;
				}

				if (coins > 0)
				{
					Cell prevCell = GetCell(x, y - coins);
					if (prevCell != null)
					{
						Assert.IsNull(prevCell.CoinRef);
						Coin coin = cell.CoinRef;
						cell.CoinRef = null;
						Point prevPos = prevCell.Position;
						coin.UpdateLoc(prevCell.Index, prevPos.X, prevPos.Y);

						if (!IsBoardInit)
						{
							Vector3 pos = coin.GetRealPosition();
							coin.MoveToSpeedBased(pos, m_dieDelay, dropSpeed, "drop");
						}
						else
						{
							prevCell.CoinRef = coin;
							prevCell.CoinRef.State = eCoinState.Idle;
							prevCell.CoinRef.RefreshPosition();
						}
					}
				}
			}
		}

		private void CleanBoard()
		{
			for (int i = 0; i < Count; ++i)
			{
				Cell cell = GetCell(i);

				if (cell.CoinRef != null)
				{
					cell.CoinRef.State = eCoinState.MarkDelete;

					cell.CoinRef.Destroy();

					cell.CoinRef = null;
				}
			}
		}

		public void Fill()
		{
			IsBoardInit = true;
			do
			{
				CleanBoard();

				for (int x = 0; x < Width; ++x)
				{
					Lines[x] = Height;

					SpawnCoins(x);

					Lines[x] = 0;
				}

				CheckAll(0);
			} while (IsNextHelp() == -1);

			IsBoardInit = false;
		}

		public int GetCoinId(int x, int y)
		{
			return GetCoin(x, y).CoinId;
		}

		public Point CountNearCoins(Point pos)
		{
			Cell cell = GetCell(pos.X, pos.Y);

			int left = cell.GetCoinCount(NeighbourPos.Left);
			int right = cell.GetCoinCount(NeighbourPos.Right);

			int top = cell.GetCoinCount(NeighbourPos.Top);
			int bottom = cell.GetCoinCount(NeighbourPos.Bottom);

			return new Point(left + right, top + bottom);
		}

		public int CountNearCoinHor(Point pos)
		{
			Cell cell = GetCell(pos.X, pos.Y);

			if (cell != null)
			{
				int left = cell.GetCoinCount(NeighbourPos.Left);
				int right = cell.GetCoinCount(NeighbourPos.Right);

				return left + right;
			}

			return -1;
		}

		public int CountNearCoinVer(Point pos)
		{
			Cell cell = GetCell(pos.X, pos.Y);

			if (cell != null)
			{
				int top = cell.GetCoinCount(NeighbourPos.Top);
				int bottom = cell.GetCoinCount(NeighbourPos.Bottom);

				return top + bottom;
			}

			return -1;
		}

		private IEnumerator SpawnCoinsWithDelay(float sec, int x)
		{
			yield return new WaitForSeconds(sec);

			SpawnCoins(x);
		}

		private void SpawnCoins(int x)
		{
			int count = Lines[x];
			if (count > 0)
			{
				int removedCoins = 0;
				for (int y = Height - 1; y >= 0; --y)
				{
					int index = PosToIndex(x, y);
					Cell cell = GetCell(index);

					if (cell.Empty || cell.CoinRef != null)
						continue;

					cell.CoinRef = CreateRandomCoin(index);

					if (!IsBoardInit)
					{
						float delay = (Lines[x] - removedCoins - 1) * 0.1f;
						cell.CoinRef.OnSpawn(m_spawnPosition, delay);
					}

					++removedCoins;

					if (Lines[x] == removedCoins)
						break;
				}

				Lines[x] = 0;
			}
		}

		private void FallDownCoins()
		{
			for (int i = 0; i < Lines.Length; ++i)
			{
				if (Lines[i] > 0)
				{
					MoveCoinsDown(i);

					if (IsBoardInit)
					{
						SpawnCoins(i);
					}
					else
					{
						StartCoroutine(SpawnCoinsWithDelay(m_dieDelay, i));
					}
				}
			}

			if (IsBoardInit)
			{
				CheckAll(0);
			}
		}

		private Coin GetCoinWithCheck( int x, int y, int cid )
		{
			Cell cell = GetCell ( x, y );

			if ( cell != null &&
				!cell.Empty &&
				cell.CoinRef != null &&
				cell.CoinRef.CoinId == cid )
			{
				return cell.CoinRef;
			}

			return null;
		}

		private void MatchCoins(Axis axis, Coin startCoin)
		{
			Match match = null;
			if (startCoin.MatchID >= 0)
			{
				match = GetMatch(startCoin.MatchID);

				if (match.Axis == axis)
					return;
			}

			Point	startPos = new Point(startCoin.XPos, startCoin.YPos);
			Point	dir = m_axisDirection[(int) axis];
			int		count = 0;
			int		cid = startCoin.CoinId;
			Coin	c = startCoin;

			do
			{
				int x = c.XPos + dir.X;
				int y = c.YPos + dir.Y;

				c = GetCoinWithCheck ( x, y, cid );
				if ( c != null )
				{
					++count;
					if ( !IsBoardInit && match == null && c.MatchID >= 0 )
					{
						match = GetMatch ( c.MatchID );
					}
				}
			}
			while ( c != null );


			if ( count >= 2 )
			{
				if (match == null)
					match = MakeMatch ( );

				for(int i = 0; i < count + 1; ++i)
				{
					Coin coin = GetCoinWithCheck(startPos.X, startPos.Y, cid);
					
					if ( coin != null )
					{
						startPos.X = coin.XPos + dir.X;
						startPos.Y = coin.YPos + dir.Y;
						coin.State = eCoinState.MarkDelete;

						match.Add ( coin );
					}

				}
			}
		}

		public void CheckAll(int start)
		{
			for (int i = Count - 1; i >= start; --i)
			{
				Cell cell = GetCell(i);
				if (cell.Empty || cell.CoinRef == null)
				{
					continue;
				}

				bool isMarked = cell.CoinRef.State == eCoinState.MarkDelete;
				if (isMarked)
				{
					continue;
				}

				MatchCoins(Axis.X, cell.CoinRef);
				MatchCoins(Axis.Y, cell.CoinRef);
			}

			if ( RemoveMarkCoins() )
			{
				FallDownCoins();
			}
			else if (OnBoardStable != null && IsStable)
			{
				OnBoardStable.Invoke();
			}
		}

		private bool RemoveMarkCoins()
		{
			bool deletedCoins = m_matches.Count > 0;

			foreach (Match mMatch in m_matches)
			{
				foreach (Coin coin in mMatch.Coins)
				{
					RemoveCoin(coin.PlaceId);
				}

				if (OnMatch != null && !IsBoardInit)
				{
					OnMatch.Invoke(mMatch.Coins);
				}

				m_matchPool.Delete(mMatch);
			}
			m_matches.Clear();

			return deletedCoins;
		}

		public void RemoveCoin(int ind)
		{
			Cell cell = GetCell(ind);

			if (cell.CoinRef != null)
			{
				if (IsBoardInit)
				{
					cell.CoinRef.Destroy();
				}
				else
				{
					cell.CoinRef.DieWithDelay(m_dieDelay);
				}

				cell.CoinRef = null;
			}
		}

		public int IsNextHelp()
		{
			for (int i = 0; i < Count; ++i)
			{
				Point pos = IndexToPos(i);

				if (pos.X > 0)
				{
					if (CheckPicksOnlyStart(GetCoin(i), GetCoin(pos.X - 1, pos.Y)))
					{
						return i;
					}
				}

				if (pos.Y > 0)
				{
					if (CheckPicksOnlyStart(GetCoin(i), GetCoin(pos.X, pos.Y - 1)))
					{
						return i;
					}
				}

				if (pos.X + 1 < Width)
				{
					if (CheckPicksOnlyStart(GetCoin(i), GetCoin(pos.X + 1, pos.Y)))
					{
						return i;
					}
				}

				if (pos.Y + 1 < Height)
				{
					if (CheckPicksOnlyStart(GetCoin(i), GetCoin(pos.X, pos.Y + 1)))
					{
						return i;
					}
				}
			}

			return -1;
		}

		public bool CheckPicksOnlyStart(Coin startpick, Coin endpick)
		{
			if (startpick == null || endpick == null)
			{
				return false;
			}

			return TrySwap(startpick, endpick);
		}
	}
}

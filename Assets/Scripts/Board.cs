using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;
using System.Runtime.InteropServices;
using Assets.Scripts;
using Assets.Scripts.Utils;
using UnityEngine.Assertions;

public class Board : MonoBehaviour
{
	private ObjectPool<Coin> m_coinsPool;
	 
	public Cell				cellPrefab;
	public Coin				coinPrefab;
	public float			dropSpeed;
	public float			swapSpeed;
	public int Width { get; private set; }
	public int Height { get; private set; }

	public event Action<Coin, Coin> OnCoinsSwap;
	public event System.Action		OnBoardStable;
	public event Action<List<Coin>> OnMatch;
	public event Action<Coin>		OnCoinDestroy; 

	// private section
	private Cell[]		m_greed;
	private Coin 		m_selected;
	private float 		m_dieDelay = 1.2f;
	private int []		m_lines;
	private Vector3		m_spawnPosition;
	private Vector2		m_size;

	public float DieDelay
	{
		get { return m_dieDelay; }
	}

	public Vector3 GetRealPosition(int x, int y)
	{	
		Vector3 pos = new Vector3 (Cell.Width * x, Cell.Height * y);

		return pos;
	}

	public Cell GetCell(int x, int y)
	{
		if (x < 0 || y < 0 || x >= Width || y >= Height)
		{
			return null;
		}

		return GetCell(posToIndex(x, y));
	}

	public Cell GetCell(int index)
	{
		if (index >= 0 && index < Greed.Length)
		{
			return Greed[index];
		}
		
		return null;
	}

	void CreateGreed()
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

	//public section
	public void Initialize(int w, int h)
	{
		print ("map init w, h: " + w + ", " + h);
		m_lines = new int[w];
		m_spawnPosition.y = (Cell.Height * h) + 20f;
		m_spawnPosition.x = 0;
		Width = w;
		Height = h;
		Count = w * h;
		m_coinsPool = new ObjectPool<Coin>()
		{
			MakeFunc = () => Instantiate(coinPrefab),
			DisableFunc = ( inst ) =>
			{
				if (OnCoinDestroy != null)
					OnCoinDestroy.Invoke(inst);
			}
		};

		RemoveList = new List<Coin> ();

		CreateGreed ();

		Fill (true);
	}

	public void Refresh()
	{
		var cellsInfo = GameController.CurrentLevel.GetComponent<CellsInfo>();
		if (cellsInfo != null)
		{
			cellsInfo.InitCells(this);
		}

		Fill(true);
	}

	public int Count { get; private set; }

	public Coin getCoin(int x, int y)
	{
		return GetCell (posToIndex (x, y)).CoinRef;
	}

	public Coin getCoin(int index)
	{
		return GetCell(index).CoinRef;
	}

	public int posToIndex(int x, int y)
	{
		if (x < 0 || y < 0 || x >= Width || y >= Height)
		{
			return -1;
		}

		return x + y * Width;
	}

	public int posToIndex(Point pos)
	{
		return posToIndex(pos.X, pos.Y);
	}

	public Point IndexToPos(int index)
	{
		return new Point (index % Width, index / Width);
	}

	public Coin Select { get; set; }
	public Coin Focused { get; set; }

	public int [] Lines
	{
		get { return m_lines; }
		set { m_lines = value; }
	}

	public List<Coin> RemoveList { get; private set; }

	public Cell[] Greed
	{
		get { return m_greed; }
		private set { m_greed = value; }
	}

	private int m_busy;

	public void OnMoveDone(Coin coin, string msg)
	{
		if (coin.State != eCoinState.Idle)
			return;
		Cell cell = GetCell(coin.PlaceId);
		cell.CoinRef = coin;

		if(msg == "failSwap")
		{		
			coin.MoveToSpeedBased(coin.GetRealPosition(), 0, swapSpeed, "doneFailSwap");
		}
		else if (msg != "doneFailSwap" && m_busy == 0)
		{
			CheckAll(false, 0);
		}
	}

	public void Swap(Coin c1, Coin c2)
	{
		if (Lines[c1.XPos] > 0 || Lines[c2.XPos] > 0)
			return;

		Vector3 pos1 = c1.transform.localPosition;
		Vector3 pos2 = c2.transform.localPosition;

		if (TrySwap (c1, c2))
		{
			ApplySwap(c1, c2);
			print ("create move coins");

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

		GetCell (c1.PlaceId).CoinRef = c2;
		GetCell (c2.PlaceId).CoinRef = c1;

		bool res = false;

		Point count = CountNearCoins (c1.Position);
		if (count.X > 1 || count.Y > 1)
		{
			res = true;
		}

		count = CountNearCoins (c2.Position);
		if (count.X > 1 || count.Y > 1)
		{
			res = true;
		}

		GetCell (c1.PlaceId).CoinRef = c1;
		GetCell (c2.PlaceId).CoinRef = c2;

		return res;
	}

	private void ApplySwap(Coin c1, Coin c2)
	{
		Point pos = c1.Position;
		c1.UpdateLoc (c2.PlaceId, c2.XPos, c2.YPos);
		c2.UpdateLoc (posToIndex (pos.X, pos.Y), pos.X, pos.Y);

		if (OnCoinsSwap != null)
			OnCoinsSwap.Invoke (c1, c2);
	}

	public Coin CreateRandomCoin(int placeId)
	{
		print ("[Board:CreateRandomCoin]");

		int maxCoin = GameController.CoinSprites.Length;

		int rndCoin = UnityEngine.Random.Range (0, maxCoin);

		return CreateCoin(placeId, rndCoin, null);
	}

	private void CoinStateChange( StateMachine<eCoinState> sender, eCoinState prevState, eCoinState currState )
	{
		Coin coin = sender.UserData as Coin;
		
		if (coin == null)
			return;

		if ( currState == eCoinState.MarkDelete )
		{
			++Lines[coin.XPos];
		}

		if (prevState == eCoinState.Moved)
			--m_busy;
		if (currState == eCoinState.Moved)
			++m_busy;
	}

	public void FreeCoin(Coin c)
	{
		m_coinsPool.Delete(c);
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
			sp = GameController.CoinSprites [coinId];
		}

		coin.Init (placeId, pos.X, pos.Y, coinId, sp);
		coin.StateMachine.OnStateChange = CoinStateChange;

		GetCell(placeId).CoinRef = coin;
		
		return coin;
	}

	public void MoveCoinsDown( bool init, int x )
	{
		int coins = 0;
		for ( int y = 0; y < Height; ++y )
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

					if (!init)
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

	void CleanBoard( )
	{
		for ( int i = 0; i < Count; ++i )
		{
			Cell cell = GetCell ( i );

			if ( cell.CoinRef != null )
			{
				cell.CoinRef.State = eCoinState.MarkDelete;
				
				cell.CoinRef.Destroy ( );

				cell.CoinRef = null;
			}
		}
	}

	public void Fill(bool init)
	{
		do
		{
			CleanBoard();

			for (int x = 0; x < Width; ++x)
			{
				Lines[x] = Height;

				SpawnCoins(init, x);

				Lines[x] = 0;
			}

			CheckAll(true, 0);
		}
		while (IsNextHelp() == -1);
	}

	public int GetCoinId(int x, int y)
	{
		return getCoin(x, y).CoinId;
	}
	
	public Point CountNearCoins(Point pos)
	{
		Cell cell = GetCell (pos.X, pos.Y);

		int left	= cell.GetCoinCount (NeighbourPos.Left);
		int right	= cell.GetCoinCount (NeighbourPos.Right);

		int top		= cell.GetCoinCount (NeighbourPos.Top);
		int bottom	= cell.GetCoinCount (NeighbourPos.Bottom);
		
		return new Point(left + right, top + bottom);
	}
	
	public bool MarkCoin(int x, int y, int idcoin)
	{
		int index = posToIndex(x, y);

		if (GetCell (index).Empty)
		{
			return false;
		}

		Coin coin = getCoin(index);
		
		if (coin.CoinId == idcoin && coin.State != eCoinState.Deleted)
		{
			print ("[Coin:markCoin] Deleted set true for: " + index);
			coin.State = eCoinState.MarkDelete;

			return true;
		}
		
		return false;
	}

	public void RemoveHorizontalCoins(int x, int y, int coinId, bool init)
	{
		int xstart = x - 1;
		List<Coin> match = new List<Coin>();
		match.Add(GetCell(x, y).CoinRef);
		while (xstart >= 0 && MarkCoin(xstart, y, coinId))
		{
			match.Add(GetCell(xstart, y).CoinRef);

			if(CountNearCoinVer(new Point(xstart, y)) > 1)
			{
				RemoveVerticalCoins(xstart, y, coinId, init);
			}

			--xstart;
		}
		
		xstart = x + 1;
		while (xstart < Width && MarkCoin(xstart, y, coinId))
		{
			match.Add(GetCell(xstart, y).CoinRef);

			if(CountNearCoinVer(new Point(xstart, y)) > 1)
			{
				RemoveVerticalCoins(xstart, y, coinId, init);
			}

			++xstart;
		}

		if (match.Count > 2 && !init && OnMatch != null)
		{
			OnMatch.Invoke( match );
		}
	}

	public void RemoveVerticalCoins(int x, int y, int coinId, bool init)
	{
		List<Coin> match = new List<Coin> ();
		match.Add(GetCell(x, y).CoinRef);
		int ystart = y - 1;
		while (ystart >= 0 && MarkCoin(x, ystart, coinId))
		{
			match.Add(GetCell(x, ystart).CoinRef);

			if(CountNearCoinHor(new Point(x, ystart)) > 1)
			{
				RemoveHorizontalCoins(x, ystart, coinId, init);
			}

			--ystart;
		}
		
		ystart = y + 1;
		while (ystart < Height && MarkCoin(x, ystart, coinId))
		{
			match.Add(GetCell(x, ystart).CoinRef);

			if(CountNearCoinHor(new Point(x, ystart)) > 1)
			{
				RemoveHorizontalCoins(x, ystart, coinId, init);
			}

			++ystart;
		}

		if (match.Count > 2 && !init && OnMatch != null)
		{
			OnMatch.Invoke(match);
		}
	}

	public int CountNearCoinHor(Point pos)
	{
		Cell cell = GetCell (pos.X, pos.Y);

		if (cell != null)
		{
			int left  = cell.GetCoinCount (NeighbourPos.Left);
			int right = cell.GetCoinCount (NeighbourPos.Right);
		
			return left + right;
		}

		return -1;
	}

	public int CountNearCoinVer(Point pos)
	{
		Cell cell = GetCell (pos.X, pos.Y);

		if (cell != null)
		{
			int top    = cell.GetCoinCount (NeighbourPos.Top);
			int bottom = cell.GetCoinCount (NeighbourPos.Bottom);
		
			return top + bottom;
		}

		return -1;
	}

	IEnumerator SpawnCoinsWithDelay( float sec, bool init, int x )
	{
		yield return new WaitForSeconds(sec);

		SpawnCoins(init, x);
	}

	void SpawnCoins( bool init, int x )
	{
		int count = Lines[x];
		if ( count > 0 )
		{
			int startY = Height - count;
			for ( int y = startY; y < Height; ++y )
			{
				Cell cell = GetCell ( x, y );
				if (cell.Empty)
					continue;

				int index = posToIndex ( x, y );
				cell.CoinRef = CreateRandomCoin ( index );

				if ( !init )
				{
					float delay = (y - startY) * 0.1f;
					cell.CoinRef.OnSpawn ( m_spawnPosition, delay );
				}
			}

			Lines[x] = 0;
		}
	}

	void FallDownCoins( bool init )
	{
		for (int i = 0; i < Lines.Length; ++i)
		{
			if (Lines[i] > 0)
			{
				MoveCoinsDown(init, i);

				if (init)
				{
					SpawnCoins(true, i);	
				}
				else
				{
					StartCoroutine(SpawnCoinsWithDelay(m_dieDelay, false, i));	
				}
			}
		}

		if (init)
		{
			CheckAll(true, 0);
		}
	}

	public void CheckAll(bool init, int start)
	{
		for (int i = Count - 1; i >= start; --i)
		{
			Cell cell = GetCell(i);
			if( cell.Empty || cell.CoinRef == null)
			{
				continue;
			}

			bool isDeleted = cell.CoinRef.State == eCoinState.MarkDelete;
			if (isDeleted)
			{
				continue;
			}

			int hcount = CountNearCoinHor(cell.Position);
			if (hcount > 1)
			{
				int idc = getCoin(i).CoinId;
				MarkCoin(cell.Position.X, cell.Position.Y, idc);
				RemoveHorizontalCoins(cell.Position.X, cell.Position.Y, idc, init);
			}

			int vcount = CountNearCoinVer(cell.Position);
			if (vcount > 1)
			{
				int idc = getCoin(i).CoinId;
				MarkCoin(cell.Position.X, cell.Position.Y, idc);
				RemoveVerticalCoins(cell.Position.X, cell.Position.Y, idc, init);
			}
		}

		if (RemoveMarkCoins(init) > 0)
		{
			FallDownCoins(init);
		}
		else if (OnBoardStable != null)
		{	
			OnBoardStable.Invoke ();
		}
	}

	int RemoveMarkCoins(bool init)
	{
		int deletedCoins = 0;
		for (int i = 0; i < Count; ++i )
		{
			Cell cell = GetCell(i);
			var coin = cell.CoinRef;
			if (coin != null && coin.State == eCoinState.MarkDelete)
			{ 
				print ("[Coin:checkAll] remove coin: " + i);
				++deletedCoins;
				RemoveCoin(i, init);
			}
		}

		return deletedCoins;
	}
	
	public void RemoveCoin(int ind, bool init)
	{
		Cell cell = GetCell(ind);
		
		if (cell.CoinRef != null)
		{
			if ( init )
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
				if (CheckPicksOnlyStart(getCoin(i), getCoin(pos.X - 1, pos.Y)) )
				{
					return i;
				}
			}
			
			if (pos.Y > 0)
			{
				if (CheckPicksOnlyStart(getCoin(i), getCoin(pos.X, pos.Y-1)))
				{
					return i;
				}
			}
			
			if (pos.X + 1 < Width)
			{
				if (CheckPicksOnlyStart(getCoin(i), getCoin(pos.X+1, pos.Y)))
				{
					return i;
				}
			}
			
			if (pos.Y + 1 < Height)
			{
				if (CheckPicksOnlyStart(getCoin(i), getCoin(pos.X, pos.Y+1)))
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

		return TrySwap(startpick, endpick);;
	}
}

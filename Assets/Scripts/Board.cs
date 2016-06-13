using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System;

public class Board : MonoBehaviour
{
	public Cell				cellPrefab;
	public Coin				coinPrefab;
	public float			dropSpeed;
	public float			swapSpeed;
	public int Width { get; private set; }
	public int Height { get; private set; }

	public Action<Coin, Coin> 	OnCoinsSwap { get; set; }
	public System.Action	 	OnBoardStable { get; set; }
	public Action<List<Coin>> 	OnMatch { get; set; }

	// private section
	private Cell[]		m_greed;
	private Coin 		m_selected;
	private float 		m_dieDelay = 1.2f;
	private int 		m_blockCount = 0;
	private Vector3		m_spawnPosition;
	private Level 		m_currLevel;
	private Vector2		m_size;

	// Use this for initialization

	public Vector3 GetRealPosition(int x, int y)
	{	
		Vector3 pos = new Vector3 (Cell.Width * x * 0.01f, Cell.Height * y * 0.01f);

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

		var disableCoins = GameController.CurrentLevel.DisabledCells;
		if (disableCoins != null)
		{
			foreach (Point pos in disableCoins)
			{
				int index = posToIndex(pos);

				var cell = GetCell(index);
				if (cell != null)
				{
					cell.Empty = true;

					cell.gameObject.SetActive(false);
				}
			}
		}
	}

	//public section
	public void Initialize(int w, int h)
	{
		print ("map init w, h: " + w + ", " + h);

		float realCoinW = Cell.Width * 0.01f;
		float realCoinH = Cell.Height * 0.01f;

		m_size = new Vector2 (w * realCoinW, h * realCoinH);
		Width = w;
		Height = h;
		Count = w * h;

		GameController.Instance.InitLevel ();

		m_currLevel = GameController.CurrentLevel;

		RemoveList = new List<Coin> ();

		CreateGreed ();

		m_spawnPosition.y = SceneTransform.getHeightInUnits ();
		m_spawnPosition.x = 0;

		Fill (true);
		
		float hw = (m_size.x / 2f) - (realCoinW / 2f);
		float hh = (m_size.y / 2f) - (realCoinH / 2f);

		float halfH = SceneTransform.getHeightInUnits () / 2;
		float delta = halfH - (m_size.y / 2f);

		if (delta > 0) 
		{
			hh += (delta * 0.4f);
		}

		transform.localPosition = new Vector2(-hw, -hh);
	}

	public void Refresh()
	{
		var disableCoins = GameController.CurrentLevel.DisabledCells;
		if (disableCoins != null && disableCoins.Count > 0)
		{
			foreach (Point pos in disableCoins)
			{
				int index = posToIndex(pos);

				var cell = GetCell(index);
				if (cell != null)
				{
					cell.Empty = true;
					var coin = getCoin(cell.Index);
					if (coin != null)
					{
						coin.State = eCoinState.MarkDelete;
						coin.gameObject.SetActive(false);
					}

					cell.gameObject.SetActive(false);
				}
			}
		}
		else
		{
			foreach (var cell in m_greed)
			{
				if ( cell.Empty )
				{
					cell.Empty = false;
					cell.gameObject.SetActive(true);
				}
			}
		}
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

	public int Blocked
	{
		get { return m_blockCount; }
		set { m_blockCount = value; }
	}

	public List<Coin> RemoveList { get; private set; }

	public Cell[] Greed
	{
		get { return m_greed; }
		private set { m_greed = value; }
	}

	public void OnMoveDone(Coin coin, string msg)
	{
		if (coin.State != eCoinState.Idle)
			return;

		GetCell(coin.PlaceId).CoinRef = coin;

		if(msg == "failSwap")
		{		
			coin.MoveToSpeedBased(coin.GetRealPosition(), swapSpeed, "doneFailSwap");
		}
		else if (Blocked < 1 && msg != "doneFailSwap")
		{
			CheckAll(false, 0);
		}
	}

	public void Swap(Coin c1, Coin c2)
	{
		Vector3 pos1 = c1.transform.localPosition;
		Vector3 pos2 = c2.transform.localPosition;

		if (TrySwap (c1, c2))
		{
			applySwap(c1, c2);
			print ("create move coins");
			//Coin cid = c1.PlaceId < c2.PlaceId ? c1 : c2;

			c1.MoveToSpeedBased(c1.GetRealPosition(), swapSpeed, "");
			c2.MoveToSpeedBased(c2.GetRealPosition(), swapSpeed, "");
		}
		else
		{
			print (pos1);
			print (pos2);
		
			c1.MoveToSpeedBased(pos2, swapSpeed, "failSwap");
			c2.MoveToSpeedBased(pos1, swapSpeed, "failSwap");
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

	private void applySwap(Coin c1, Coin c2)
	{
		Point pos = c1.Position;
		c1.UpdateLoc (c2.PlaceId, c2.XPos, c2.YPos);
		c2.UpdateLoc (posToIndex (pos.X, pos.Y), pos.X, pos.Y);

		if (OnCoinsSwap != null)
			OnCoinsSwap.Invoke (c1, c2);
	}

	public Coin CreateRandomCoin(int placeId)
	{
		print ("[Board:createRandomCoin]");

		int maxCoin = GameController.CoinSprites.Length;

		int rndCoin = UnityEngine.Random.Range (0, maxCoin);

		return CreateCoin(placeId, rndCoin, null);
	}

	private void CoinStateChange( eCoinState prevState, eCoinState currState )
	{
		if ( currState == eCoinState.Moved )
		{
			++m_blockCount;
		}
		if (currState == eCoinState.Idle && prevState == eCoinState.Moved)
		{
			--m_blockCount;
		}
	}

	public Coin CreateCoin(int placeId, int coinId, Sprite sp)
	{
		if (GetCell (placeId).Empty)
		{
			return null;
		}

		Point pos = IndexToPos(placeId);
		
		Coin coin = null;
		if (RemoveList.Count > 0)
		{	
			coin = RemoveList[RemoveList.Count - 1];
			
			if(!RemoveList.Remove(coin) )
			{
				Debug.Log("Failed remove coin from remove list");
			}	
		}
		else
		{
			coin = Instantiate (coinPrefab, new Vector3 (pos.X, pos.Y, 0f), Quaternion.identity) as Coin;
		}
		
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

	bool SwapCoins(int index1, int index2)
	{
		Cell cell1 = GetCell (index1);
		Cell cell2 = GetCell (index2);

		if (cell1.Empty || cell2.Empty)
		{
			return false;
		}

		Coin c1 = getCoin (index1);
		Coin c2 = getCoin (index2);

		cell1.CoinRef = c2;
		cell2.CoinRef = c1;

		return true;
	}

	public int MoveCoinsDown(int index, int x, int y, bool init)
	{
		int ny = y + 1;
		
		while(ny < Height)
		{ 
			int nid = posToIndex(x, ny);
			Cell cell = GetCell(nid);

			if (cell.CoinRef != null && !cell.Empty)
			{
				Coin coin = cell.CoinRef;

				if( !SwapCoins(nid, index) )
				{
					continue;
				}

				coin.UpdateLoc(index, x, y);
				
				if (init)
				{
					coin.RefreshPosition();
				}
				else
				{
					coin.Delay = m_dieDelay;
					coin.MoveToSpeedBased(coin.GetRealPosition(), dropSpeed, "drop");
				}

				break;
			}
			++ny;
		}

		return ny;
	}

	bool fillForCell(int x, int y, bool init)
	{
		int index = posToIndex(x, y);
		bool update = false;

		if (getCoin(x, y) == null && !GetCell (index).Empty)
		{
			int ny = MoveCoinsDown(index, x, y, init);
			
			print ("ny: " + ny);
			if (ny >= Height)
			{
				print ("create random coin");
				Coin coin = m_currLevel.CoinForIndex(init, index);

				if(coin == null)
				{
					return false;
				}

				if (!init)
				{
					coin.Delay = m_dieDelay;
					
					coin.transform.localPosition = coin.GetRealPosition() + m_spawnPosition;
					
					coin.MoveToSpeedBased(coin.GetRealPosition(), dropSpeed, "drop");
				}
				else
				{
					coin.State = eCoinState.Idle;
					
					GetCell(coin.PlaceId).CoinRef = coin;
				}
				
				update = true;
			}
		}

		return update;
	}

	public void Fill(bool init)
	{
		bool update = false;

		print ("fill with init: " + init);
		
		for (int x = 0; x < Width; ++x)
		{
			for(int y = 0; y < Height; ++y)
			{
				if( fillForCell(x, y, init) && !update)
				{
					update = true;
				}
			}
		}

		if (update && init)
		{
			CheckAll(init, 0);
		}

		while (IsNextHelp() == -1)
		{
			for (int i = 0; i < Count; ++i)
			{
				Cell cell = GetCell(i);
				var coin = getCoin(cell.Index);
				if (coin != null)
				{
					coin.Destroy();

					cell.CoinRef = null;
				}
			}

			Fill(true);
		}
	}

	public int GetCoinId(int x, int y)
	{
		return getCoin(x, y).CoinId;
	}
	
	public Point CountNearCoins(Point pos)
	{
		Cell cell = GetCell (pos.X, pos.Y);

		int left = cell.GetCoinCount (NeighbourPos.Left);
		int right = cell.GetCoinCount (NeighbourPos.Right);

		int top = cell.GetCoinCount (NeighbourPos.Top);
		int bottom = cell.GetCoinCount (NeighbourPos.Bottom);
		
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
			int left = cell.GetCoinCount (NeighbourPos.Left);
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
			int top = cell.GetCoinCount (NeighbourPos.Top);
			int bottom = cell.GetCoinCount (NeighbourPos.Bottom);
		
			return top + bottom;
		}

		return -1;
	}
	


	public void CheckAll(bool init, int start)
	{
		for (int i = Count - 1; i >= start; --i)
		{
			Cell cell = GetCell(i);
			if(cell.Empty || cell.CoinRef.State == eCoinState.MarkDelete)
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
				cell.CoinRef = null;
			}
		}

		if (deletedCoins == 0)
		{
			if (OnBoardStable != null)
				OnBoardStable.Invoke ();
		}
		else
		{
			Fill (init);	
		}
	}
	
	public void RemoveCoin(int ind, bool init)
	{
		Coin picked = getCoin(ind);
		
		if (picked != null)
		{
			if ( init )
			{
				picked.Destroy();
			}
			else
			{
				picked.DieWithDelay(m_dieDelay);
			} 
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

	public Vector2 CoinOffset
	{
		get { return m_size / 2; }
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class Map : MonoBehaviour
{
	public Cell				cellPrefab;
	public Coin				coinPrefab;
	public float			dropSpeed;
	public float			swapSpeed;
	public int Width { get; private set; }
	public int Height { get; private set; }

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
		Vector3 offset = new Vector3 (CoinOffset.x, CoinOffset.y);

		Vector3 pos = new Vector3 ((Coin.coinWidth + Coin.border / 2) * x, (Coin.coinHeight + Coin.border / 2) * y);

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

		float realCoinW = Coin.coinWidth + Coin.border;
		float realCoinH = Coin.coinHeight + Coin.border;

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

		fill (true);
		
		float hw = (m_size.x / 2f) - (realCoinW / 2f);
		float hh = (m_size.y / 2f) - (realCoinH / 2f);
		
		transform.localPosition = new Vector2(-hw, -hh);
	}

	public void Refresh()
	{
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
					var coin = getCoin(cell.Index);
					if (coin != null)
					{
						coin.Deleted = true;
						coin.gameObject.SetActive(false);
					}

					cell.gameObject.SetActive(false);
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

	public Coin Select
	{
		get { return m_selected; }
		set { m_selected = value; }
	}

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

	public void onMoveBegin(Coin coin)
	{
		++m_blockCount;
	}

	public void onMoveDone(Coin coin, string msg)
	{
		coin.State = eCoinState.Idle;

		--m_blockCount;

		m_currLevel.OnCoinMove(coin);

		GetCell(coin.PlaceId).CoinRef = coin;

		if(msg == "failSwap")
		{		
			coin.moveToSpeedBased(coin.GetRealPosition(), swapSpeed, "doneFailSwap");
		}
		else if (Blocked < 1 && msg != "doneFailSwap")
		{
			checkAll(false, 0);
		}
	}

	public void swap(Coin c1, Coin c2)
	{
		Vector3 pos1 = c1.transform.localPosition;
		Vector3 pos2 = c2.transform.localPosition;

		if (trySwap (c1, c2))
		{
			applySwap(c1, c2);
			print ("create move coins");
			//Coin cid = c1.PlaceId < c2.PlaceId ? c1 : c2;

			c1.moveToSpeedBased(c1.GetRealPosition(), swapSpeed, "");
			c2.moveToSpeedBased(c2.GetRealPosition(), swapSpeed, "");
		}
		else
		{
			print (pos1);
			print (pos2);
		
			c1.moveToSpeedBased(pos2, swapSpeed, "failSwap");
			c2.moveToSpeedBased(pos1, swapSpeed, "failSwap");
		}
	}

	public bool trySwap(Coin c1, Coin c2)
	{
		Select = null;

		GetCell (c1.PlaceId).CoinRef = c2;
		GetCell (c2.PlaceId).CoinRef = c1;

		bool res = false;

		Point count = countNearCoins (c1.Position);
		if (count.X > 1 || count.Y > 1)
		{
			res = true;
		}

		count = countNearCoins (c2.Position);
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
		c1.updateLoc (c2.PlaceId, c2.XPos, c2.YPos);
		c2.updateLoc (posToIndex (pos.X, pos.Y), pos.X, pos.Y);

		m_currLevel.OnCoinsSwap (c1, c2);
	}

	public Coin createRandomCoin(int placeId)
	{
		print ("[Map:createRandomCoin]");

		int maxCoin = GameController.CoinSprites.Length;

		int rndCoin = UnityEngine.Random.Range (0, maxCoin);

		return createCoin(placeId, rndCoin, null);
	}

	public Coin createCoin(int placeId, int coinId, Sprite sp)
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

		coin.init (placeId, pos.X, pos.Y, coinId, sp);

		GetCell(placeId).CoinRef = coin;
		
		return coin;
	}

	bool swapCoins(int index1, int index2)
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

	public int moveCoinsDown(int index, int x, int y, bool init)
	{
		int ny = y + 1;
		
		while(ny < Height)
		{ 
			int nid = posToIndex(x, ny);
			Cell cell = GetCell(nid);

			if (cell.CoinRef != null && !cell.Empty)
			{
				Coin coin = cell.CoinRef;

				if( !swapCoins(nid, index) )
				{
					continue;
				}

				coin.updateLoc(index, x, y);
				
				if (init)
				{
					coin.refreshPosition();
				}
				else
				{

					coin.Delay = m_dieDelay;
					coin.moveToSpeedBased(coin.GetRealPosition(), dropSpeed, "drop");
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
			int ny = moveCoinsDown(index, x, y, init);
			
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
					
					coin.moveToSpeedBased(coin.GetRealPosition(), dropSpeed, "drop");
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

	public void fill(bool init)
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
			checkAll(init, 0);
		}

		while (isNextHelp() == -1)
		{
			for (int i = 0; i < Count; ++i)
			{
				Cell cell = GetCell(i);
				var coin = getCoin(cell.Index);
				if (coin != null)
				{
					coin.destroy();

					cell.CoinRef = null;
				}
			}

			fill(true);
		}
	}

	public int getCoinId(int x, int y)
	{
		return getCoin(x, y).CoinId;
	}
	
	public Point countNearCoins(Point pos)
	{
		Cell cell = GetCell (pos.X, pos.Y);

		int left = cell.GetCoinCount (NeighbourPos.Left);
		int right = cell.GetCoinCount (NeighbourPos.Right);

		int top = cell.GetCoinCount (NeighbourPos.Top);
		int bottom = cell.GetCoinCount (NeighbourPos.Bottom);
		
		return new Point(left + right, top + bottom);
	}
	
	public bool markCoin(int x, int y, int idcoin)
	{
		int index = posToIndex(x, y);

		if (GetCell (index).Empty)
		{
			return false;
		}

		Coin coin = getCoin(index);
		
		if (coin.CoinId == idcoin && !coin.Deleted)
		{
			print ("[Coin:markCoin] Deleted set true for: " + index);
			coin.Deleted = true;

			return true;
		}
		
		return false;
	}

	public void removeHorizontalCoins(int x, int y, int coinId)
	{
		int xstart = x - 1;
		List<Coin> match = new List<Coin>();
		while (xstart >= 0 && markCoin(xstart, y, coinId))
		{
			match.Add(GetCell(xstart, y).CoinRef);

			if(countNearCoinVer(new Point(xstart, y)) > 1)
			{
				removeVerticalCoins(xstart, y, coinId);
			}

			--xstart;
		}
		
		xstart = x + 1;
		while (xstart < Width && markCoin(xstart, y, coinId))
		{
			match.Add(GetCell(xstart, y).CoinRef);

			if(countNearCoinVer(new Point(xstart, y)) > 1)
			{
				removeVerticalCoins(xstart, y, coinId);
			}

			++xstart;
		}

		if (match.Count > 0)
		{
			m_currLevel.OnMatch(coinId, match.Count);
		}
	}

	public void removeVerticalCoins(int x, int y, int coinId)
	{
		List<Coin> match = new List<Coin> ();
		int ystart = y - 1;
		while (ystart >= 0 && markCoin(x, ystart, coinId))
		{
			match.Add(GetCell(x, ystart).CoinRef);

			if(countNearCoinHor(new Point(x, ystart)) > 1)
			{
				removeHorizontalCoins(x, ystart, coinId);
			}

			--ystart;
		}
		
		ystart = y + 1;
		while (ystart < Height && markCoin(x, ystart, coinId))
		{
			match.Add(GetCell(x, ystart).CoinRef);

			if(countNearCoinHor(new Point(x, ystart)) > 1)
			{
				removeHorizontalCoins(x, ystart, coinId);
			}

			++ystart;
		}

		if (match.Count > 0)
		{
			m_currLevel.OnMatch(match[0].CoinId, match.Count);
		}
	}

	public int countNearCoinHor(Point pos)
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

	public int countNearCoinVer(Point pos)
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
	


	public void checkAll(bool init, int start)
	{
		for (int i = Count - 1; i >= start; --i)
		{
			Cell cell = GetCell(i);
			if(cell.Empty)
			{
				continue;
			}

			int hcount = countNearCoinHor(cell.Position);
			if (hcount > 1)
			{
				int idc = getCoin(i).CoinId;
				markCoin(cell.Position.X, cell.Position.Y, idc);
				removeHorizontalCoins(cell.Position.X, cell.Position.Y, idc);
			}

			int vcount = countNearCoinVer(cell.Position);
			if (vcount > 1)
			{
				int idc = getCoin(i).CoinId;
				markCoin(cell.Position.X, cell.Position.Y, idc);
				removeVerticalCoins(cell.Position.X, cell.Position.Y, idc);
			}

			if (hcount > 1 || vcount > 1)
			{
				break;
			}
		}
		
		int deletedCoins = 0;
		for (int i = 0; i < Count; ++i )
		{
			Cell cell = GetCell(i);
			if (cell.CoinRef != null && cell.CoinRef.Deleted)
			{ 
				print ("[Coin:checkAll] remove coin: " + i);
				++deletedCoins;
				removeCoin(i, init);
				cell.CoinRef = null;
			}
		}

		if ( deletedCoins == 0 )
		{
			GameController.CurrentLevel.OnBoardStable();
		}
		
		fill(init);	
	}

	private GameObject m_lighting;
	public void removeCoin(int ind, bool init)
	{
		Coin picked = getCoin(ind);
		
		if (picked != null)
		{
			if (init == true)
			{
				picked.destroy();
			}
			else
			{
				picked.dieWithDelay(m_dieDelay);

				if ( m_lighting == null )
				{
					m_lighting = Instantiate(GameController.Instance.lighting);
				}

			} 
		} 
	}

	public int isNextHelp()
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

		return trySwap(startpick, endpick);;
	}

	public Vector2 CoinOffset
	{
		get { return m_size / 2; }
	}
}

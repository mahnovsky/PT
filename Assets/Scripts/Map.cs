using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

public class GameStrategy {

	public GameStrategy(Map map) {
		m_map = map;
	}

	public virtual Coin coinForIndex(bool init, int index) {

		return m_map.createRandomCoin (index);
	}

	public virtual void onCoinsSwap (Coin c1, Coin c2) {}

	public virtual void onCoinMove (Coin c) {}

	public virtual void onMatch(int cid, int count) {}

	public Map GameMap {
		get { return m_map; }
	}

	private Map m_map;
}

public class Map : MonoBehaviour
{
	public GameController controller;
	public Coin coinPrefab;
	public int width;
	public int height;
	public ActionManager actionMng;
	public float dropSpeed;
	public float swapSpeed;

	public Vector3 getContentSize() {
		return m_contentSize;
	}

	// Use this for initialization
	void Awake ()
	{
		initialize (width, height);
	}

	Cell getCell(int x, int y) {

		if (x < 0 || y < 0 || x >= Width || y >= Height) {
			return null;
		}

		return getCell(posToIndex(x, y));
	}

	Cell getCell(int index) {

		if (index >= 0 && index < Greed.Length) {
			return Greed[index];
		}
		
		return null;
	}

	void createGreed() {

		Greed = new Cell[Count];

		for (int i = 0; i < Count; ++i) {

			Greed[i] = new Cell(i, indexToPos(i), false);
		}

		for (int i = 0; i < Count; ++i) {
			
			var cell = Greed[i];

			cell.setNeighbour(NeighbourPos.Left, getCell(cell.Position.X - 1, cell.Position.Y));
			cell.setNeighbour(NeighbourPos.Right, getCell(cell.Position.X + 1, cell.Position.Y));
			cell.setNeighbour(NeighbourPos.Top, getCell(cell.Position.X, cell.Position.Y + 1));
			cell.setNeighbour(NeighbourPos.Bottom, getCell(cell.Position.X, cell.Position.Y - 1));
		}

		foreach (Point pos in Level.currLevel().disabledCoins) {

			int index = posToIndex(pos);

			getCell (index).Empty = true;
		}
	}

	//public section
	public void initialize(int w, int h)
	{
		print ("map init w, h: " + w + ", " + h);

		float realCoinW = Coin.coinWidth + Coin.border;
		float realCoinH = Coin.coinHeight + Coin.border;

		m_size = new Vector2 (w * realCoinW, h * realCoinH);

		m_count = w * h;

		/*float qsize = Mathf.Min (SceneTransform.getHeightInUnits () * 0.8f, 
		                         SceneTransform.getWidthInUnits () * 0.9f);

		if (m_size.x > qsize || m_size.y > qsize)
		{
			transform.localScale = new Vector3(qsize / m_size.x, qsize / m_size.y);
			m_size.x = qsize;
			m_size.y = qsize;
		}*/

		controller.initLevel ();

		m_currLevel = controller.CurrentLevel;

		m_removed = new List<Coin> ();

		createGreed ();

		if (m_currLevel != null)
		{
			Strategy = m_currLevel.getStrategy (this);
		}
		else
		{
			Strategy = new GameStrategy(this);
		}


		m_spawnPosition.y = SceneTransform.getHeightInUnits ();
		m_spawnPosition.x = 0;

		fill (true);

		if (m_currLevel.LevelMode == Level.Mode.MoveItem)
		{
			MoveItemLevel mil = m_currLevel as MoveItemLevel;
			int index = posToIndex(mil.topInX, h - 1);

			removeCoin(index, true);
			
			createCoin(posToIndex(mil.topInX, h - 1), m_currLevel.maxCoinsCount() + 1, mil.item);
		}
		
		float hw = (m_size.x / 2f) - (realCoinW / 2f);
		float hh = (m_size.y / 2f) - (realCoinH / 2f);
		
		transform.localPosition = new Vector2(-hw, -hh);
	}
	
	public int Width {
		get { return width; }
	}
	
	public int Height {
		get { return height; }
	}
	
	public int Count {
		get { return m_count; }
	}
	
	public Coin getCoin(int x, int y) {
		return getCell (posToIndex (x, y)).CoinRef;
	}

	public Coin getCoin(int index) {
		return getCell(index).CoinRef;
	}

	public int posToIndex(int x, int y) {

		if (x < 0 || y < 0 || x >= Width || y >= Height) {
			return -1;
		}

		return x + y * Width;
	}

	public int posToIndex(Point pos) {
		return posToIndex(pos.X, pos.Y);
	}

	public Point indexToPos(int index) {
		return new Point (index % Width, index / Width);
	}

	public Coin Select {
		get { return m_selected; }
		set { m_selected = value; }
	}

	public int Blocked {
		get { return m_blockCount; }
		set { m_blockCount = value; }
	}

	public GameStrategy Strategy {
		get { return m_strategy; }
		set { m_strategy = value; }
	}

	public List<Coin> RemoveList {
		get { return m_removed; }
	}

	public Cell[] Greed {
		get { return m_greed; }
		private set { m_greed = value; }
	}

	public void onMoveBegin(Coin coin) {

		++m_blockCount;
	}

	public void onMoveDone(Coin coin, string msg) {

		coin.State = eCoinState.Idle;

		--m_blockCount;

		Strategy.onCoinMove(coin);

		getCell(coin.PlaceId).CoinRef = coin;

		if(msg == "failSwap") {
				
			coin.moveToSpeedBased(coin.getRealPosition(), swapSpeed, "doneFailSwap");
		}
		else if (Blocked < 1 && msg != "doneFailSwap") {

			checkAll(false, 0);
		}
	}

	public void swap(Coin c1, Coin c2) {

		Vector3 pos1 = c1.transform.localPosition;
		Vector3 pos2 = c2.transform.localPosition;

		if (trySwap (c1, c2)) {

			applySwap(c1, c2);
			print ("create move coins");
			//Coin cid = c1.PlaceId < c2.PlaceId ? c1 : c2;

			c1.moveToSpeedBased(c1.getRealPosition(), swapSpeed, "");
			c2.moveToSpeedBased(c2.getRealPosition(), swapSpeed, "");
		}
		else {

			print (pos1);
			print (pos2);
		
			c1.moveToSpeedBased(pos2, swapSpeed, "failSwap");
			c2.moveToSpeedBased(pos1, swapSpeed, "failSwap");
		}
	}

	public bool trySwap(Coin c1, Coin c2) {

		Select = null;

		getCell (c1.PlaceId).CoinRef = c2;
		getCell (c2.PlaceId).CoinRef = c1;

		bool res = false;

		Point count = countNearCoins (c1.Position);
		if (count.X > 1 || count.Y > 1) {

			res = true;
		}

		count = countNearCoins (c2.Position);
		if (count.X > 1 || count.Y > 1) {

			res = true;
		}

		getCell (c1.PlaceId).CoinRef = c1;
		getCell (c2.PlaceId).CoinRef = c2;

		return res;
	}

	private void applySwap(Coin c1, Coin c2) {

		Point pos = c1.Position;
		c1.updateLoc (c2.PlaceId, c2.XPos, c2.YPos);
		c2.updateLoc (posToIndex (pos.X, pos.Y), pos.X, pos.Y);

		Strategy.onCoinsSwap (c1, c2);
	}

	public Coin createRandomCoin(int placeId) {

		print ("[Map:createRandomCoin]");

		int maxCoin = Level.currLevel () == null ? 8 : Level.currLevel().maxCoinsCount ();

		int rndCoin = UnityEngine.Random.Range (0, maxCoin);

		return createCoin(placeId, rndCoin, null);
	}

	public Coin createCoin(int placeId, int coinId, Sprite sp) {

		if (getCell (placeId).Empty) {
			return null;
		}

		Point pos = indexToPos(placeId);
		
		Coin coin = null;
		if (m_removed.Count > 0) {
			
			coin = m_removed[m_removed.Count - 1];
			
			if(!m_removed.Remove(coin) ) {
				Debug.Log("Failed remove coin from remove list");
			}
			
		} else {
			coin = Instantiate (coinPrefab, new Vector3 (pos.X, pos.Y, 0f), Quaternion.identity) as Coin;
		}
		
		coin.transform.parent = transform;
		if (sp == null) {
			sp = Level.currLevel ().coinSprites [coinId];
		}

		coin.init (placeId, pos.X, pos.Y, coinId, sp);

		getCell(placeId).CoinRef = coin;
		
		return coin;
	}

	bool swapCoins(int index1, int index2) {

		Cell cell1 = getCell (index1);
		Cell cell2 = getCell (index2);

		if (cell1.Empty || cell2.Empty) {
			return false;
		}

		Coin c1 = getCoin (index1);
		Coin c2 = getCoin (index2);

		cell1.CoinRef = c2;
		cell2.CoinRef = c1;

		return true;
	}

	public int moveCoinsDown(int index, int x, int y, bool init) {

		int ny = y + 1;
		
		while(ny < Height) {
			
			int nid = posToIndex(x, ny);
			Cell cell = getCell(nid);

			if (cell.CoinRef != null && !cell.Empty) {

				Coin coin = cell.CoinRef;

				if( !swapCoins(nid, index) ) {
					continue;
				}

				coin.updateLoc(index, x, y);
				
				if (init) {
					coin.refreshPosition();
				}
				else {

					coin.Delay = m_dieDelay;
					coin.moveToSpeedBased(coin.getRealPosition(), dropSpeed, "drop");
				}

				break;
			}
			++ny;
		}

		return ny;
	}

	bool fillForCell(int x, int y, bool init) {

		int index = posToIndex(x, y);
		bool update = false;

		if (getCoin(x, y) == null && !getCell (index).Empty) {
			
			int ny = moveCoinsDown(index, x, y, init);
			
			print ("ny: " + ny);
			if (ny >= Height) {
				
				print ("create random coin");
				Coin coin = Strategy.coinForIndex(init, index);

				if(coin == null) {
					return false;
				}

				if (!init) {
					
					coin.Delay = m_dieDelay;
					
					coin.transform.localPosition = coin.getRealPosition() + m_spawnPosition;
					
					coin.moveToSpeedBased(coin.getRealPosition(), dropSpeed, "drop");
				}
				else {
					coin.State = eCoinState.Idle;
					
					getCell(coin.PlaceId).CoinRef = coin;
				}
				
				update = true;
			}
		}

		return update;
	}

	public void fill(bool init) {
		
		bool update = false;

		print ("fill with init: " + init);
		
		for (int x = 0; x < Width; ++x) {
			
			for(int y = 0; y < Height; ++y) {
				
				if( fillForCell(x, y, init) && !update) {
					update = true;
				}
			}
		}

		if (update && init) {
			checkAll(init, 0);
		}
		
		while (isNextHelp() == -1) {
			
			for (int i = 0; i < Count; ++i) {

				Cell cell = getCell(i);
				if (cell.CoinRef != null) {

					cell.CoinRef.destroy();

					cell.CoinRef = null;
				}
			}

			fill(true);
		}
	}

	public int getCoinId(int x, int y) {

		return getCoin(x, y).CoinId;
	}
	
	public Point countNearCoins(Point pos) {

		Cell cell = getCell (pos.X, pos.Y);

		int left = cell.getCoinCount (NeighbourPos.Left);
		int right = cell.getCoinCount (NeighbourPos.Right);

		int top = cell.getCoinCount (NeighbourPos.Top);
		int bottom = cell.getCoinCount (NeighbourPos.Bottom);
		
		return new Point(left + right, top + bottom);
	}
	
	public bool markCoin(int x, int y, int idcoin) {
		
		int index = posToIndex(x, y);

		if (getCell (index).Empty) {
			return false;
		}

		Coin coin = getCoin(index);
		
		if (coin.CoinId == idcoin && !coin.Deleted) {
			print ("[Coin:markCoin] Deleted set true for: " + index);
			coin.Deleted = true;

			return true;
		}
		
		return false;
	}

	public void removeHorizontalCoins(int x, int y, int coinId) {
		
		int xstart = x - 1;
		List<Coin> match = new List<Coin>();
		while (xstart >= 0 && markCoin(xstart, y, coinId)) {
			match.Add(getCell(xstart, y).CoinRef);
			xstart = xstart -1;

			if(countNearCoinVer(new Point(xstart, y)) > 1) {
				removeVerticalCoins(xstart, y, coinId);
			}
		}
		
		xstart = x + 1;
		while (xstart < Width && markCoin(xstart, y, coinId)) {
			match.Add(getCell(xstart, y).CoinRef);
			xstart = xstart + 1;

			if(countNearCoinVer(new Point(xstart, y)) > 1) {
				removeVerticalCoins(xstart, y, coinId);
			}
		}
		if (match.Count > 0)
		{
			Strategy.onMatch(match[0].CoinId, match.Count);
		}
	}

	public void removeVerticalCoins(int x, int y, int coinId) {

		List<Coin> match = new List<Coin> ();
		int ystart = y - 1;
		while (ystart >= 0 && markCoin(x, ystart, coinId)) {
			match.Add(getCell(x, ystart).CoinRef);
			ystart = ystart -1;

			if(countNearCoinHor(new Point(x, ystart)) > 1) {
				removeHorizontalCoins(x, ystart, coinId);
			}
		}
		
		ystart = y + 1;
		while (ystart < Height && markCoin(x, ystart, coinId)) {
			match.Add(getCell(x, ystart).CoinRef);
			ystart = ystart + 1;

			if(countNearCoinHor(new Point(x, ystart)) > 1) {
				removeHorizontalCoins(x, ystart, coinId);
			}
		}

		Strategy.onMatch (match [0].CoinId, match.Count);
	}

	public int countNearCoinHor(Point pos) {

		Cell cell = getCell (pos.X, pos.Y);

		if (cell != null) {
			int left = cell.getCoinCount (NeighbourPos.Left);
			int right = cell.getCoinCount (NeighbourPos.Right);
		
			return left + right;
		}

		return -1;
	}

	public int countNearCoinVer(Point pos) {

		Cell cell = getCell (pos.X, pos.Y);

		if (cell != null) {

			int top = cell.getCoinCount (NeighbourPos.Top);
			int bottom = cell.getCoinCount (NeighbourPos.Bottom);
		
			return top + bottom;
		}

		return -1;
	}
	


	public void checkAll(bool init, int start) {

		for (int i = Count - 1; i >= start; --i) {

			Cell cell = getCell(i);
			if(cell.Empty) {
				continue;
			}

			int hcount = countNearCoinHor(cell.Position);
			if (hcount > 1) {
				int idc = getCoin(i).CoinId;
				markCoin(cell.Position.X, cell.Position.Y, idc);
				removeHorizontalCoins(cell.Position.X, cell.Position.Y, idc);
			}

			int vcount = countNearCoinVer(cell.Position);
			if (vcount > 1) {
				int idc = getCoin(i).CoinId;
				markCoin(cell.Position.X, cell.Position.Y, idc);
				removeVerticalCoins(cell.Position.X, cell.Position.Y, idc);
			}

			if (hcount > 1 || vcount > 1) {
				break;
			}
		}
		
		int deletedCoins = 0;
		for (int i = 0; i < Count; ++i ) {

			Cell cell = getCell(i);
			if (cell.CoinRef != null && cell.CoinRef.Deleted) { 
				print ("[Coin:checkAll] remove coin: " + i);
				++deletedCoins;
				removeCoin(i, init);
				cell.CoinRef = null;
			}
		}
		
		fill(init);	
	}
	
	public void removeCoin(int ind, bool init) {
		
		Coin picked = getCoin(ind);
		
		if (picked != null) {

			if (init == true) {

				picked.destroy();
			}
			else {

				picked.dieWithDelay(m_dieDelay);
			} 
		} 
	}

	public int isNextHelp() {

		for (int i = 0; i < Count; ++i) {
			
			Point pos = indexToPos(i);
			
			if (pos.X > 0) {
				if (checkPicksOnlyStart(getCoin(i), getCoin(pos.X - 1, pos.Y)) ) {
					return i;
				}
			}
			
			if (pos.Y > 0) {
				if (checkPicksOnlyStart(getCoin(i), getCoin(pos.X, pos.Y-1))) {
					return i;
				}
			}
			
			if (pos.X + 1 < Width) {
				if (checkPicksOnlyStart(getCoin(i), getCoin(pos.X+1, pos.Y))) {
					return i;
				}
			}
			
			if (pos.Y + 1 < Height) {
				if (checkPicksOnlyStart(getCoin(i), getCoin(pos.X, pos.Y+1))) {
					return i;
				}
			}
		}
		
		return -1;
	}
	
	public bool checkPicksOnlyStart(Coin startpick, Coin endpick) {
		
		if (startpick == null || endpick == null) {
			return false;
		}

		return trySwap(startpick, endpick);;
	}

	public Vector2 CoinOffset {
		get { return m_size / 2; }
	}

	
	// private section
	private List<Coin> m_removed;
	private Cell[] m_greed;
	private int 	m_count;
	private Coin 	m_selected;
	private float 	m_dieDelay = 0.5f;
	private int 	m_blockCount = 0;
	private DelegateAction m_currDelay;
	private Vector3 m_spawnPosition;
	private Level 	m_currLevel;
	private GameStrategy m_strategy;	
	private Vector3 m_contentSize;
	Vector2 m_size;
}

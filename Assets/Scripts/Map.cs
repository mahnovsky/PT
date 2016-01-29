using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Point {
	
	public Point() {
		m_x = 0;
		m_y = 0;
	}
	
	public Point(int x, int y) {
		m_x = x;
		m_y = y;
	}
	
	public int X {
		get { return m_x; }
		set { m_x = value; }
	}
	
	public int Y {
		get { return m_y; }
		set { m_y = value; }
	}

	private int m_x;
	private int m_y;
}

public class Pointf {
	
	public Pointf() {
		m_x = 0;
		m_y = 0;
	}
	
	public Pointf(float x, float y) {
		m_x = x;
		m_y = y;
	}
	
	public float X {
		get { return m_x; }
		set { m_x = value; }
	}
	
	public float Y {
		get { return m_y; }
		set { m_y = value; }
	}
	
	private float m_x;
	private float m_y;
}

public class GameStrategy {

	public GameStrategy(Map map) {
		m_map = map;
	}

	public virtual Coin coinForIndex(bool init, int index) {

		return m_map.createRandomCoin (index);
	}

	public virtual void onCoinsSwap (Coin c1, Coin c2) {}

	public virtual void onCoinMove (Coin c) {}

	public Map GameMap {
		get { return m_map; }
	}

	private Map m_map;
}

public class Map : MonoBehaviour, IActionListener {

	public Game controller;
	public Coin coinPrefab;
	public int width;
	public int height;
	public ActionManager actionMng;
	public float dropSpeed;
	public float coinWidth;
	public float coinHeight;

	// Use this for initialization
	void Start () {
		init (width, height);
	}

	//public section
	public void init(int w, int h) {
		print ("map init w, h: " + w + ", " + h);
		controller.initLevel ();
		m_currLevel = controller.CurrentLevel;

		m_count = w * h;

		m_removed = new List<Coin> ();
		m_coins = new Coin[m_count];

		if (m_currLevel != null) {
			Strategy = m_currLevel.getStrategy (this);
		} else {
			Strategy = new GameStrategy(this);
		}


		m_spawnPosition.y = SceneTransform.getHeightInUnits ();
		m_spawnPosition.x = 0;

		fill (true);

		Vector3 vs = m_currLevel.coinSprites[0].bounds.size;

		if (m_currLevel.LevelMode == Level.Mode.MoveItem) {

			MoveItemLevel mil = m_currLevel as MoveItemLevel;
			int index = posToIndex(mil.topInX, h - 1);

			removeCoin(index, true);
			
			createCoin(posToIndex(mil.topInX, h - 1), m_currLevel.maxCoinsCount() + 1, mil.item);
		}

		vs.x *= 0.9f;
		vs.y *= 0.9f;
		float mapWidth  = vs.x * w;
		float mapHeight = vs.y * h;
		Vector3 nPos = new Vector3 (-mapWidth / 2, -mapHeight / 2) + transform.localPosition;
		transform.localPosition = nPos;
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
	
	public Coin[] Coins {
		get { return m_coins; }
	}
	
	public Coin getCoin(int x, int y) {
		return Coins [x + y * Width];
	}
	
	public int posToIndex(int x, int y) {
		return x + y * Width;
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

	public void onActionBegin(Action action) {

		if (action.GetType ().Name == "MoveAction") {
			++m_blockCount;
			Coin coin = action.Target.GetComponent<Coin>();
			coin.State = eCoinState.Moved;
		}
	}

	public void onActionDone(Action action) {

		if (action.GetType ().Name == "MoveAction") {

			string str = action.UserData as string;

			Coin coin = action.Target.GetComponent<Coin>();
			coin.State = eCoinState.Idle;

			--m_blockCount;

			Strategy.onCoinMove(coin);

			if (Blocked < 1 && str != "failSwap") {

				checkAll(false, 0);
			}
		} 
		else if (action.GetType ().Name == "DelegateAction") {

			List<Coin> rl = action.UserData as List<Coin>;

			if (rl != null) {

				foreach(Coin type in rl) {

					type.State = eCoinState.Deleted;
					m_removed.Add(type);
				}
			}
		}
	}

	public void swap(Coin c1, Coin c2) {

		Vector3 pos1 = c1.transform.localPosition;
		Vector3 pos2 = c2.transform.localPosition;

		if (trySwap (c1, c2)) {

			applySwap(c1, c2);
			print ("create move coins");
			Coin cid = c1.PlaceId < c2.PlaceId ? c1 : c2;
			actionMng.runAction(new MoveAction(this, c1.gameObject, 0.125f, pos2, cid));
			actionMng.runAction(new MoveAction(this, c2.gameObject, 0.125f, pos1, cid));
		}
		else {

			print (pos1);
			print (pos2);

			SequenceAction sq = new SequenceAction(this);

			sq.addAction(new MoveAction(this, c1.gameObject, 0.125f, pos2, "failSwap"));
			sq.addAction(new MoveAction(this, c1.gameObject, 0.125f, pos1, "failSwap"));
			actionMng.runAction(sq);

			sq = new SequenceAction(this);

			sq.addAction(new MoveAction(this, c2.gameObject, 0.125f, pos1, "failSwap"));
			sq.addAction(new MoveAction(this, c2.gameObject, 0.125f, pos2, "failSwap"));
			actionMng.runAction(sq);
		}
	}

	public bool trySwap(Coin c1, Coin c2) {

		Select = null;
		Coins [c1.PlaceId] = c2;
		Coins [c2.PlaceId] = c1;

		bool res = false;

		Point count = countNearCoins (c1.Position, c2.CoinId);
		if (count.X > 1 || count.Y > 1) {

			res = true;
		}

		count = countNearCoins (c2.Position, c1.CoinId);
		if (count.X > 1 || count.Y > 1) {

			res = true;
		}

		Coins [c1.PlaceId] = c1;
		Coins [c2.PlaceId] = c2;

		return res;
	}

	private void applySwap(Coin c1, Coin c2) {

		Coins [c1.PlaceId] = c2;
		Coins [c2.PlaceId] = c1;

		Point pos = c1.Position;
		c1.updateLoc (c2.PlaceId, c2.XPos, c2.YPos);
		c2.updateLoc (posToIndex (pos.X, pos.Y), pos.X, pos.Y);

		Strategy.onCoinsSwap (c1, c2);
	}

	public Coin createRandomCoin(int placeId) {

		print ("[Map:createRandomCoin]");

		int maxCoin = Level.currLevel () == null ? 8 : Level.currLevel().maxCoinsCount ();

		int rndCoin = Random.Range (0, maxCoin);

		return createCoin(placeId, rndCoin, null);
	}

	public Coin createCoin(int placeId, int coinId, Sprite sp) {

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
			sp = controller.CurrentLevel.coinSprites[coinId];
		}
		
		coin.init (placeId, pos.X, pos.Y, coinId, sp);
		coin.State = eCoinState.Created;
		Coins [placeId] = coin;
		
		return coin;
	}

	public int moveCoinsDown(int index, int x, int y, bool init) {

		int ny = y + 1;
		
		while(ny < Height) {
			
			int nid = posToIndex(x, ny);
			if (Coins[nid] != null) {
				
				Coins[index] = Coins[nid];
				Coin coin = Coins[index];
				Point p = indexToPos(index);
				coin.updateLoc(index, p.X, p.Y);
				
				if (init) {
					coin.refreshPosition();
				}
				else {

					coin.Delay = m_dieDelay;
					coin.setDrop(this, coin.transform.localPosition);
				}
				Coins[nid] = null;
				
				break;
			}
			++ny;
		}

		return ny;
	}

	public void setDropCoin(Coin coin) {

		Vector3 dest = coin.getRealPosition();

		float s = (coin.transform.localPosition - coin.getRealPosition()).magnitude;
		
		SequenceAction sq = new SequenceAction(this);
		sq.addAction(new DelegateAction(this, m_dieDelay, null));
		sq.addAction(new MoveAction(this, coin.gameObject, dest, s / dropSpeed, "drop"));
		actionMng.runAction(sq);
	}
	
	public void fill(bool init) {
		
		bool update = false;

		print ("fill with init: " + init);
		
		for (int x = 0; x < Width; ++x) {
			
			for(int y = 0; y < Height; ++y) {
				
				int index = posToIndex(x, y);
				
				if(Coins[index] == null) {

					int ny = moveCoinsDown(index, x, y, init);

					print ("ny: " + ny);
					if (ny >= Height) {

						print ("create random coin");
						Coin coin = Strategy.coinForIndex(init, index);
						if (!init) {

							coin.Delay = m_dieDelay;
							coin.setDrop(this, coin.getRealPosition() + m_spawnPosition);
						}
						else {
							coin.State = eCoinState.Idle;
						}

						update = true;
					}
				}
			}
		}

		if (update && init) {
			checkAll(init, 0);
		}
		
		while (isNextHelp() == -1) {
			
			for (int i = 0; i < Count; ++i) {
				
				if (Coins[i] != null) {
					
					removeCoin(i, init);
					Coins[i] = null;
					fill(init);
				}
			}
		}
	}

	public int getCoinId(int x, int y) {

		int index = posToIndex (x, y);
		
		Coin coin = Coins[index];
		
		return coin.CoinId;
	}
	
	public Point countNearCoins(Point pos, int coinId) {
		
		int xstart = pos.X -1;
		int hcount = 0;
		while (xstart >= 0 && getCoinId(xstart, pos.Y) == coinId) {
			xstart = xstart -1;
			++hcount;
		}
		
		xstart = pos.X + 1;
		while (xstart < Width && getCoinId(xstart, pos.Y) == coinId) {
			xstart = xstart + 1;
			++hcount;
		}
		
		int ystart = pos.Y -1;
		int vcount = 0;
		while (ystart >= 0 && getCoinId(pos.X, ystart) == coinId) {
			ystart = ystart -1;
			++vcount;
		}
		
		ystart = pos.Y + 1;
		while (ystart < Height && getCoinId(pos.X, ystart) == coinId) {
			ystart = ystart + 1;
			++vcount;
		}
		
		return new Point(hcount, vcount);
	}
	
	public bool markCoin(int x, int y, int idcoin) {
		
		int index = posToIndex(x, y);
		Coin coin = Coins[index];
		
		if (coin.CoinId == idcoin && !coin.Deleted) {
			print ("[Coin:markCoin] Deleted set true for: " + index);
			coin.Deleted = true;

			return true;
		}
		
		return false;
	}
	
	public void removeHorizontalCoins(int x, int y, int coinId) {
		
		int xstart = x - 1;
		while (xstart >= 0 && markCoin(xstart, y, coinId)) {
			xstart = xstart -1;
		}
		
		xstart = x + 1;
		while (xstart < Width && markCoin(xstart, y, coinId)) {
			xstart = xstart + 1;
		}
	}
	
	
	public void removeVerticalCoins(int x, int y, int coinId) {
		
		int ystart = y - 1;
		while (ystart >= 0 && markCoin(x, ystart, coinId)) {
			ystart = ystart -1;
		}
		
		ystart = y + 1;
		while (ystart < Height && markCoin(x, ystart, coinId)) {
			ystart = ystart + 1;
		}
		
	}
	
	public void checkAll(bool init, int start) {

		for (int i = start; i < Count; ++i) {
			
			Point pos = indexToPos(i);
			
			Point count = countNearCoins(pos, Coins[i].CoinId);

			int hcount = count.X;
			if (hcount > 1) {
				
				int idc = Coins[i].CoinId;
				markCoin(pos.X, pos.Y, idc);
				removeHorizontalCoins(pos.X, pos.Y, idc);
			}

			int vcount = count.Y;
			if (vcount > 1) {
				
				int idc = Coins[i].CoinId;
				markCoin(pos.X, pos.Y, idc);
				removeVerticalCoins(pos.X, pos.Y, idc);
			}

			if (hcount > 1 || vcount > 1) {
				break;
			}
		}
		
		int deletedCoins = 0;
		for (int i = 0; i < Count; ++i ) {
			
			Coin coin = Coins[i];
			if (coin != null && coin.Deleted) { 
				print ("[Coin:checkAll] remove coin: " + i);
				++deletedCoins;
				removeCoin(i, init);
				Coins[i] = null;
			}
		}
		
		fill(init);	
	}
	
	public void removeCoin(int ind, bool init) {
		
		Coin picked = Coins[ind];
		
		if (picked != null) {

			picked.GetComponent<SpriteRenderer>().enabled = false;
			picked.GetComponent<BoxCollider2D>().enabled = false;

			if (init == true) {
				picked.State = eCoinState.Deleted;
				m_removed.Add(picked);
			}
			else {
				picked.State = eCoinState.Idle;

				picked.dieWithDelay(m_dieDelay);
			} 
		} 
	}

	public int isNextHelp() {

		for (int i = 0; i < Count; ++i) {
			
			Point pos = indexToPos(i);
			
			if (pos.X > 0) {
				if (checkPicksOnlyStart(Coins[i], Coins[posToIndex(pos.X-1, pos.Y)])) {
					return i;
				}
			}
			
			if (pos.Y > 0) {
				if (checkPicksOnlyStart(Coins[i], Coins[posToIndex(pos.X, pos.Y-1)])) {
					return i;
				}
			}
			
			if (pos.X + 1 < Width) {
				if (checkPicksOnlyStart(Coins[i], Coins[posToIndex(pos.X+1, pos.Y)])) {
					return i;
				}
			}
			
			if (pos.Y + 1 < Height) {
				if (checkPicksOnlyStart(Coins[i], Coins[posToIndex(pos.X, pos.Y+1)])) {
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

	
	// private section
	private List<Coin> m_removed;
	private Coin [] m_coins;
	private int 	m_count;
	private Coin 	m_selected;
	private float 	m_dieDelay = 0.5f;
	private int 	m_blockCount = 0;
	private DelegateAction m_currDelay;
	private Vector3 m_spawnPosition;
	private Level 	m_currLevel;
	private GameStrategy m_strategy;	
}

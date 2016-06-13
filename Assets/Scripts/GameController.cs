using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Assets.Scripts.Utils;

public class GameController : MonoBehaviour
{
	public Board			board;
	public Pointf			designSize;
	public GameObject		destroyEffect;
	public GameObject		lighting;
	public Sprite[]			coinSprites;

	public GameObject		failPanel;
	public GameObject		winPanel;

	public Image			timeBar;

	public GameObject		background;
	public bool				enableCheats	= true;

	GUIStyle				m_style;

	int						m_changeCoin	= 0;
	int						m_currentCoin	= 0;
	private LevelSaver		m_levelSaver;
	private Sprite			m_currSprite;
	private Texture2D		m_texture;
	private string			m_number		= "";

	public Action OnUpdate { get; set; }

	void Update( )
	{
		if (OnUpdate != null)
			OnUpdate.Invoke ();
	}

	void Awake()
	{
		Instance = this;

		board.Initialize(8, 8);
		if (CurrentLevel != null && Debug.isDebugBuild)
		{
			m_style = new GUIStyle();

			m_style.fontSize = 16;
		}
		
		if (!enableCheats)
			Camera.main.aspect = designSize.X / designSize.Y;

		GameManager.Pause = false;
	}

	void OnGUI()
	{
		if (!Debug.isDebugBuild || CurrentLevel == null)
		{
			return;
		}

		if (!enableCheats)
		{
			return;
		}

		string sw = "";
		if (m_changeCoin == 0)
			sw = "None";
		else if (m_changeCoin == 1)
			sw = "Change Coin ON";
		else if (m_changeCoin == 2)
			sw = "Remove cell";
		else if (m_changeCoin == 3)
			sw = "Move count";
		else if (m_changeCoin == 4)
			sw = "Save level";
		else if (m_changeCoin == 5)
			sw = "Refresh";

		if (GUI.Button (new Rect (0, 30, 200, 50), sw))
		{
			++m_changeCoin;
			if (m_changeCoin > 5)
				m_changeCoin = 0;
		}
		
		if (CoinSprites[m_currentCoin] != m_currSprite)
		{
			var sp = CoinSprites[m_currentCoin];
			m_currSprite = sp;
			
			m_texture = new Texture2D((int)sp.rect.width, (int)sp.rect.height);

			m_texture.SetPixels(sp.texture.GetPixels((int)sp.rect.x,(int)sp.rect.y, (int)sp.rect.width, (int)sp.rect.height));

			m_texture.Apply();
		}

		if (GUI.Button (new Rect (0, 80, 100, 50), m_texture))
		{
			int max = CoinSprites.Length;
			if(m_currentCoin  + 1 < max)
			{
				++m_currentCoin;
			}
			else
			{
				m_currentCoin = 0;
			}
		}

		if ( GUI.Button ( new Rect ( 0, 130, 100, 50 ), "apply" ) )
		{
			int p = 0;
			if (!String.IsNullOrEmpty(m_number))
				p = Int32.Parse ( m_number );
			
			if (m_changeCoin == 3)
			{
				MoveCounter mc = CurrentLevel.GetComponent<MoveCounter> ();
				if (mc != null)
					mc.TotalMoves = p;
			}
			if ( m_changeCoin == 4)
			{
				CurrentLevel.Number = p;
				SaveLevel();
			}
			else if (m_changeCoin == 5)
			{
				CurrentLevel.DisabledCells.Clear();
				board.Refresh();
			}
		}

		string num = GUI.TextField(new Rect(0, 180, 100, 50), m_number);
		if (String.IsNullOrEmpty(num) || m_number == num)
			return;

		m_number = num;
	}

	public void SaveLevel( )
	{
		if ( m_levelSaver == null )
		{
			m_levelSaver = new LevelSaver();
		}

		m_levelSaver.SaveLevel(CurrentLevel);
	}

	public void InitLevel()
	{
		if (LevelNum <= 0)
		{
			LevelNum = 1;
		}

		string levelFile = "level_" + LevelNum;
		string gameDir = GameManager.Instance.GameDirectory;
		JSONObject jsonObject = TextLoader.GetFileAsJson(gameDir, levelFile);
		
		CurrentLevel = new Level()
		{ 
			Number = LevelNum
		};

		CurrentLevel.Load( jsonObject );
	}

	public static int LevelNum { get; set; }

	public static Level CurrentLevel { get; private set; }

	public void OnCoinTap(Coin c)
	{
		if (m_changeCoin == 1)
		{
			c.ChangeCoinId (m_currentCoin);
		}
		else if (m_changeCoin == 2)
		{
			CurrentLevel.DisabledCells.Add(new Point(c.XPos, c.YPos));

			board.Refresh();
		}
	}

	public void OnNextLevelBtn()
	{
		++LevelNum;
		Application.LoadLevel("Game");
	}

	public void OnLevelFail()
	{
		GameManager.Instance.OnEnablePanel(failPanel);
	}

	public void OnLevelWin()
	{
		GameManager.Instance.OnEnablePanel(winPanel);	
	}

	public void Repeat()
	{
		CurrentLevel.Refresh();

		GameManager.Instance.OnClosePanel ();
	}

	public void OnSceneSwap( )
	{
		CurrentLevel.Free();
		CurrentLevel = null;
	}

	public static GameController Instance { get; private set; }

	public static Sprite[] CoinSprites
	{
		get { return Instance.coinSprites; }
	}
}
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour
{
	public Map				map;
	public Pointf			designSize;
	public GameObject		destroyEffect;
	public GameObject		lighting;
	public Sprite[]			coinSprites;

	public bool				enableCheats	= true;
	public GameObject		levelEndPanel;

	GUIStyle				m_style;

	string					m_mode;
	string					m_levelNumStr;
	int						m_changeCoin = 0;
	int						m_currentCoin	= 0;
	private LevelLoader		m_levelLoader;
	private LevelSaver		m_levelSaver;
	private Sprite			m_currSprite;
	private Texture2D m_texture;
	private string m_number = "";

	void Awake()
	{
		if ( Instance != null )
		{
			Destroy(gameObject);

			return;
		}

		Instance = this;

		map.Initialize(6, 8);
		if (CurrentLevel != null)
		{
			m_style = new GUIStyle();

			m_style.fontSize = 16;

			m_mode = "Mode: " + CurrentLevel.LevelMode.ToString();

			m_levelNumStr = "Level: " + LevelNum.ToString();
		}

		if (!enableCheats)
			Camera.main.aspect = designSize.X / designSize.Y;
	}

	void OnGUI()
	{
		if (!Debug.isDebugBuild || CurrentLevel == null)
		{
			return;
		}

		GUI.Label (new Rect (0, 0, 100, 50), m_levelNumStr, m_style);
		
		GUI.Label (new Rect (0, 30, 100, 50), m_mode, m_style);

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
			sw = "Curr level";
		else if (m_changeCoin == 4)
			sw = "Move count";

		if (GUI.Button (new Rect (0, 30, 200, 50), sw))
		{
			++m_changeCoin;
			if (m_changeCoin > 4)
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
			int max = GameController.CoinSprites.Length;
			if(m_currentCoin  + 1 < max)
			{
				++m_currentCoin;
			}
			else
			{
				m_currentCoin = 0;
			}
		}

		if (GUI.Button(new Rect(0, 130, 100, 50), "save"))
		{
			SaveLevel();
		}

		string num = GUI.TextField(new Rect(0, 180, 100, 50), m_number);
		if (String.IsNullOrEmpty(num) || m_number == num)
			return;

		m_number = num;
		int p = Int32.Parse(num);

		if (p > 0)
		{
			ClassicLevel cl = CurrentLevel as ClassicLevel;
			
			if (m_changeCoin == 3 && p != CurrentLevel.Number)
				CurrentLevel.Number = p;
			else if (cl != null && m_changeCoin == 4 && p != cl.MaxMoveCount)
				cl.MaxMoveCount = p;
		}
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
		if ( m_levelLoader == null )
		{
			m_levelLoader = new LevelLoader();
		}

		if (LevelNum <= 0)
		{
			LevelNum = 1;
		}

		if ( CurrentLevel != null && LevelNum == CurrentLevel.Number )
		{
			CurrentLevel.Refresh();
			return;
		}

		CurrentLevel = m_levelLoader.Load( LevelNum );

		CurrentLevel.Init ();
	}

	public static int LevelNum { get; set; }

	public static Level CurrentLevel { get; private set; }

	public void OnCoinTap(Coin c)
	{
		if (m_changeCoin == 1)
		{
			c.changeCoinId (m_currentCoin);
		}
		else if (m_changeCoin == 2)
		{
			CurrentLevel.DisabledCells.Add(new Point(c.XPos, c.YPos));

			map.Refresh();
		}
	}

	public void OnMenuButton()
	{
		Application.LoadLevel ("MainMenu");
	}

	public void OnMapBtn()
	{
		Application.LoadLevel ("Map");
	}

	public void OnLevelEnd()
	{
		levelEndPanel.SetActive (true);
	}

	public void OnRepeatLevelBtn()
	{
		Application.LoadLevel ("Game");
	}

	public void OnNextLevelBtn()
	{
		++LevelNum;
		Application.LoadLevel ("Game");
	}

	public static GameController Instance { get; private set; }

	public static Sprite[] CoinSprites
	{
		get { return Instance.coinSprites; }
	}
}
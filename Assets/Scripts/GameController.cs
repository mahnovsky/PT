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
	bool					m_changeCoin	= false;
	int						m_currentCoin	= 0;
	private LevelLoader		m_levelLoader;

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

		string sw = m_changeCoin ? "Change Coin ON" : "Change Coin OFF";
		if (GUI.Button (new Rect (0, 30, 200, 50), sw))
		{
			m_changeCoin = !m_changeCoin;
		}

		Texture texture = GameController.CoinSprites [m_currentCoin].texture;
		if (GUI.Button (new Rect (0, 80, 100, 50), texture))
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
			return;
		}

		CurrentLevel = m_levelLoader.Load( LevelNum );

		CurrentLevel.Init ();
	}

	public static int LevelNum { get; set; }

	public static Level CurrentLevel { get; private set; }

	public void OnCoinTap(Coin c)
	{
		if (m_changeCoin)
		{
			c.changeCoinId (m_currentCoin);
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
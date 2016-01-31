using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameController : MonoBehaviour
{
	public Map map;
	public Pointf designSize;

	void Awake()
	{
		_inst = this;

		m_style = new GUIStyle ();

		m_style.fontSize = 16;

		m_mode = "Mode: " + m_currentLevel.LevelMode.ToString ();

		m_levelNumStr = "Level: " + m_levelNum.ToString ();

		Camera.main.aspect = designSize.X / designSize.Y;
	}

	void OnGUI()
	{
		if (!Debug.isDebugBuild)
		{
			return;
		}

		GUI.Label (new Rect (0, 0, 100, 50), m_levelNumStr, m_style);

		if (m_currentLevel != null)
		{
			GUI.Label (new Rect (0, 30, 100, 50), m_mode, m_style);
		}

		if (!enableCheats)
		{
			return;
		}

		string sw = m_changeCoin ? "Change Coin ON" : "Change Coin OFF";
		if (GUI.Button (new Rect (0, 30, 200, 50), sw))
		{
			m_changeCoin = !m_changeCoin;
		}

		Texture texture = CurrentLevel.coinSprites [m_currentCoin].texture;
		if (GUI.Button (new Rect (0, 80, 100, 50), texture))
		{
			int max = CurrentLevel.coinSprites.Length;
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

	public void initLevel()
	{
		m_currentLevel = null;

		if (m_levelNum <= 0)
		{
			m_levelNum = 1;
		}

		Object res = Resources.Load ("Level_" + m_levelNum);
		GameObject currentLevel = res as GameObject;
		m_currentLevel = currentLevel.GetComponent<Level> ();

		m_currentLevel.init ();
	}

	public static int LevelNum
	{
		get { return m_levelNum; }
		set { m_levelNum = value; }
	}

	public Level CurrentLevel
	{
		get { return m_currentLevel; }
	}

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

	public static GameController Instance
	{
		get { return _inst; }
	}

	public bool enableCheats = true;
	public GameObject levelEndPanel;

	private static int m_levelNum;
	private Level m_currentLevel;
	GUIStyle m_style;

	string m_mode;
	string m_levelNumStr;
	bool m_changeCoin = false;
	int m_currentCoin = 0;

	static GameController _inst;
}
using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour 
{
	public Level level;
	// Use this for initialization
	void Awake()
	{
		if (Inst != null && Inst != this)
		{
			Destroy(this.gameObject);

			return;
		}

		Inst = this;
	}

	void OnGUI() 
	{
		GUI.Label (new Rect (400,0,100,50), "Level: " + m_levelNum.ToString());
		if (m_currentLevel != null) 
		{
			GUI.Label (new Rect (400, 30, 100, 50), "Mode: " + m_currentLevel.LevelMode.ToString ());
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
		if (res != null) {
			GameObject currentLevel = res as GameObject;
			m_currentLevel = currentLevel.GetComponent<Level> ();

			m_currentLevel.init ();
		}
		else 
		{
			m_currentLevel = level;

			m_currentLevel.init ();
		}
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

	public static Game Inst 
	{
		get { return m_instance; }
		private set { m_instance = value; }
	}

	private static Game m_instance;
	private static int m_levelNum;
	private Level m_currentLevel;
}

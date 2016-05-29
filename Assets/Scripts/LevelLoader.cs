using System;
using UnityEngine;
using System.Collections;
using System.IO;

public class LevelLoader
{
	private Level m_level;
	private bool m_loadStatus;
	
	public LevelLoader( )
	{
	}

	public Level Load( int num )
	{
		string levelDir = GameManager.Instance.GameDirectory;
		string fileName = "level_" + num;
		var path = levelDir != null ? Path.Combine(levelDir, fileName) : fileName;
		TextAsset textData = Resources.Load(levelDir + "/" + fileName, typeof(TextAsset)) as TextAsset;

		if (textData != null)
		{
			JSONObject root = new JSONObject(textData.text);

			m_level = new Level ();

			m_loadStatus = m_level.Load (root);

			if (!m_loadStatus)
			{
				m_level = null;
			}
		}

		if ( m_level != null )
		{
			m_level.Number = num;
		}

		return m_level;
	}
}

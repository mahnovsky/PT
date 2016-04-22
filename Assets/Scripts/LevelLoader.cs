using System;
using UnityEngine;
using System.Collections;
using System.IO;

public class LevelLoader
{
	private Level m_level;
	private readonly JsonParser m_parser;
	private bool m_loadStatus;
	
	public LevelLoader( )
	{
		m_parser = new JsonParser();
		m_parser.AddFunc("type", (key, obj) =>
		{
			Level.Mode mode = (Level.Mode)Enum.Parse(typeof (Level.Mode), obj.str);

			if ( mode == Level.Mode.Classic )
			{
				m_level = new ClassicLevel();
			}
			else
			{
				m_level = new MoveItemLevel();	
			}
		});
		m_parser.AddFunc("level", (key, obj) =>
		{
			m_loadStatus = m_level.Load(obj);
		});
	}

	public Level Load( int num )
	{
		string levelDir = GameManager.Instance.GameDirectory;
		string fileName = GameManager.Instance.GameDirectory + "level_" + num;
		var path = levelDir != null ? Path.Combine(levelDir, fileName) : fileName;
		TextAsset textData = Resources.Load(path, typeof(TextAsset)) as TextAsset;

		if (textData != null)
		{
			JSONObject root = new JSONObject(textData.text);

			m_parser.ParseObject(root);

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

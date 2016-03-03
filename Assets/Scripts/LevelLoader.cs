using System;
using UnityEngine;
using System.Collections;

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
		string fileName = "level_" + num;

		TextAsset textData = Resources.Load(fileName, typeof(TextAsset)) as TextAsset;

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

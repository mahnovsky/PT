using System;
using System.Collections.Generic;
using Assets.Scripts.Utils;

namespace Assets.Scripts
{
	public class LevelList
	{
		private readonly List<string> m_levels;
		public event Action<Level> OnLevelChange; 

		public LevelList()
		{
			m_levels = new List<string>();

			string gameDir = GameManager.Instance.GameDirectory;
			JSONObject obj = TextLoader.GetFileAsJson(gameDir, "LevelList");

			m_levels = obj.GetArray("list");
		}

		public Level GetLevel(int levelNum)
		{
			if (levelNum < 1 && levelNum >= m_levels.Count)
				throw new Exception ( "Level " + levelNum + " not exists." );

			string fileName = m_levels[levelNum - 1];
			string gameDir = GameManager.Instance.GameDirectory;
			JSONObject jsonObject = TextLoader.GetFileAsJson ( gameDir, fileName );

			var level = new Level ( )
			{
				Number = levelNum
			};

			level.Load ( jsonObject );

			if (OnLevelChange != null)
				OnLevelChange.Invoke(level);

			level.Init();

			return level;
		}
	}
}

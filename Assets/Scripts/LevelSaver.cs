using UnityEngine;
using System.Collections;
using System.IO;
using Assets.Scripts;

public class LevelSaver
{
	public void SaveLevel( Level level )
	{
		JSONObject root = new JSONObject(JSONObject.Type.OBJECT);

		level.Save(root);
		string filePath = Application.dataPath + "/Resources/level_" + level.Number + ".json";
		
		File.WriteAllBytes(filePath, System.Text.Encoding.UTF8.GetBytes (root.Print(true)));
	}
}

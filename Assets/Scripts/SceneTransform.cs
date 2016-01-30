using UnityEngine;
using System.Collections;

public class SceneTransform
{
	public static float getWidthInUnits() 
	{
		return Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0)).x * 2;
	}

	public static float getHeightInUnits() 
	{
		return Camera.main.ScreenToWorldPoint(new Vector3(0, Screen.height)).y * 2;
	}
}

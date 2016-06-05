using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UILevelList : MonoBehaviour
{
	public GameObject buttonPrefab;
	private Dictionary<Button, int> m_indexes = new Dictionary<Button, int>(); 
	// Use this for initialization
	void Awake ()
	{
		var path = "config";
		TextAsset textData = Resources.Load(path, typeof(TextAsset)) as TextAsset;

		if ( textData != null )
		{
			JSONObject root = new JSONObject(textData.text);
			int count = 0;
			root.GetField(out count, "levels", 0);

			if ( count > 0 )
			{
				for ( int i = 0; i < count; ++i )
				{
					var btn = Instantiate(buttonPrefab);

					var label = btn.GetComponentInChildren<Text>();
					label.text = "Level " + (i + 1);

					var uiBtn = btn.GetComponent<Button>();
					m_indexes.Add(uiBtn, i);
					uiBtn.onClick.AddListener(() =>
					{
						int index = 0;
						if ( m_indexes.TryGetValue ( uiBtn, out index ) )
						{
							GameController.LevelNum = index + 1;
	
							GameManager.Instance.OnGameLoad( "Arcade" );
						}
					});

					btn.transform.SetParent( transform, false );
					btn.transform.localScale = new Vector3(1, 1, 1);
				}
			}
		}

	}
}

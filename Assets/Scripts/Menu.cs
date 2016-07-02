using UnityEngine;
using System.Collections;
using Assets.Scripts;

namespace Assets.Scripts
{
	public class Menu : MonoBehaviour, IScene
	{
		private GameManager m_manager;

		public void Init( )
		{
			m_manager = GameManager.Instance;

			gameObject.SetActive ( true );
		}

		public void Free( )
		{
			gameObject.SetActive ( false );
		}

		public void OnButtonPress( string name )
		{
			m_manager.GameDirectory = name;

			if ( name == "Arcade" )
			{
			}
			else if ( name == "Time" )
			{
				m_manager.RunScene<GameUI> (SceneMove.Left);
			}
			else if (name == "OtherGames")
			{
				
			}
		}

		public bool IsLoaded()
		{
			return enabled;
		}

		public GameObject GetObject()
		{
			return gameObject;
		}
	}
}

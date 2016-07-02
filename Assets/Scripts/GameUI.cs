
using Holoville.HOTween;
using UnityEngine;

namespace Assets.Scripts
{
	public class GameUI : MonoBehaviour, IScene
	{
		public GameObject TopPanel;
		public GameController controller;
		private GameManager m_manager;

		public void Init( )
		{
			m_manager = GameManager.Instance;

			//controller.gameObject.SetActive(true);
			gameObject.SetActive(true);
		}

		public void Free()
		{
			gameObject.SetActive(false);
		}

		public void OnButtonPress( string name )
		{
			if (name == "MainMenu")
			{
				m_manager.RunScene<Menu>(SceneMove.Right);
				GameController.CurrentLevel.Refresh();
				m_manager.OnClosePanel();
			}
		}

		public bool IsLoaded()
		{
			if (controller.isActiveAndEnabled)
			{
				TopPanel.SetActive(true);
			}
			return controller.isActiveAndEnabled;
		}

		public GameObject GetObject()
		{
			return gameObject;
		}
	}
}

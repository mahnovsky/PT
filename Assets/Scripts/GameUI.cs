
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
			
			gameObject.SetActive(true);
		}

		public void Free()
		{
			gameObject.SetActive(false);
		}

		public void OnButtonPress( string cmd )
		{
			if (cmd == "MainMenu")
			{
				m_manager.RunScene<Menu>(SceneMove.Right);
				GameController.CurrentLevel.Refresh();
				m_manager.OnClosePanel();
			}
			else if (cmd == "Repeat")
			{
				GameController.Instance.Repeat();
			}
			else if (cmd == "Continue")
			{
				GameController.Instance.OnNextLevel();
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

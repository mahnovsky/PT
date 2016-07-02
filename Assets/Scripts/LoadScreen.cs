using System;
using UnityEngine;
using Holoville.HOTween;

namespace Assets.Scripts
{
	public enum SceneMove
	{
		None,
		Left,
		Right,
	}

	public class LoadScreen : MonoBehaviour
	{
		public Canvas		canvas;
		public Transform	Root;
		private IScene		m_scene;
		private bool		m_initialized;
		private Tweener		m_moveLoader;
		private Vector3		m_startPos;
		private SceneMove	m_move;
		private float		m_delay;

		public IScene Scene {
			get { return m_scene; }
		}

		public void LoadScene(IScene scene, SceneMove move)
		{
			m_initialized = false;
			m_scene = scene;
			gameObject.SetActive(true);

			m_move = move;
			if (move != SceneMove.None)
			{
				var rt = canvas.GetComponent<RectTransform>();
				Vector3 pos = new Vector3(move == SceneMove.Left ? 
					6.4f : -6.4f, 0);
				m_startPos = Root.localPosition;
				Root.localPosition = pos;
				transform.localPosition = (pos * -100f);
			}
		}

		void OnLoaderMoveDone()
		{
			gameObject.SetActive(false);

			if (m_moveLoader != null)
			{
				m_moveLoader.Kill();
				m_moveLoader = null;
			}
		}

		void Update()
		{
			if (m_scene != null)
			{
				if (!m_initialized)
				{
					m_scene.Init();
					m_initialized = true;
					m_delay = 0.5f;
				}

				if (m_scene.IsLoaded() && m_moveLoader == null)
				{
					m_delay -= Time.deltaTime;
					if (m_delay > 0)
						return;

					GameManager.Instance.OnSceneLoaded();

					if (m_move != SceneMove.None)
					{
						m_moveLoader = HOTween.To(Root, 1,
							new TweenParms()
								.Prop("localPosition", m_startPos)
								.Ease(EaseType.Linear)
								.OnComplete(OnLoaderMoveDone));
					}
					else
					{
						OnLoaderMoveDone();
					}
				}
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets
{
	class FrameAnimator : MonoBehaviour
	{
		public Sprite[] Sprites;

		private SpriteRenderer m_renderer;
		private int m_fps = 60;
		private float m_time;

		void Awake( )
		{
			m_renderer = GetComponent<SpriteRenderer>();
			m_time = 0;
		}


		void Update( )
		{
			if (Sprites.Length > 1)
			{
				if (m_time > 1f)
					m_time = 0;

				m_time += Time.deltaTime;
				
				float t = (m_fps / Sprites.Length) * (1f / m_fps);

				int index = Mathf.CeilToInt( m_time / t );
				if (index >= Sprites.Length)
					index = 0;

				m_renderer.sprite = Sprites[index];
			}
		}
	}
}

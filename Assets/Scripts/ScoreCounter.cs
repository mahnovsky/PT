using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class ScoreCounter
	{
		private readonly float DELAY = 1f;
		private Text	m_scoreText;
		private int		m_currentScore;
		private int		m_score;
		private float	m_time;
		private float	m_currentTime;
		private float	m_delay;

		public void Init(Text text)
		{
			m_scoreText = text;

			text.text = "Score: 0";

			m_time = 0.08f;
			m_currentTime = m_time;
		}

		public void AddScore(int score)
		{
			m_currentScore = m_score;
			m_score += score;
			m_delay = DELAY;
		}

		public void Update()
		{
			m_delay -= Time.deltaTime;
			if (m_delay > 0)
				return;

			int delta = m_score - m_currentScore;
			m_currentTime -= Time.deltaTime;
			if (delta > 0 && m_currentTime < 0)
			{
				++m_currentScore;

				m_scoreText.text = "Score: " + m_currentScore.ToString();

				m_currentTime = m_time;
			}
		}
	}
}

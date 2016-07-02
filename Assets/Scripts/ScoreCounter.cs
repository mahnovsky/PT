using System;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class ScoreCounter : EntityComponent
	{
		private readonly float DELAY = 1f;
		private Text	m_scoreText;
		private int		m_currentScore;
		private int		m_score;
		private float	m_time;
		private float	m_currentTime;
		private float	m_delay;

		public void OnMatch(List<Coin> coins)
		{
			int total = 5 * coins.Count;
			AddScore ( total );

			int index = Mathf.FloorToInt ( ( float )coins.Count / 2 );
			var pos = coins[index].transform.position;//Camera.main.WorldToScreenPoint (  );
			PopupLabelGenerator.Instance.Print (
				total.ToString ( ), pos, Vector2.up * 100, 2f, 1f );
		}

		public override void Load( JSONObject obj )
		{
			var stObj = obj.GetField("stepTime");
			if (stObj != null)
			{
				m_time = stObj.n;

				m_currentTime = m_time;
			}
		}

		public override void Init()
		{
			m_delay = DELAY;
			GameController.Instance.OnUpdate += Update;
			GameController.Instance.board.OnMatch += OnMatch;
		}

		public void SetText(Text text)
		{
			m_scoreText = text;

			text.text = "Score: 0";
		}

		public void AddScore(int score)
		{
			m_currentScore = m_score;
			m_score += score;
			m_delay = DELAY;
		}

		public override void Refresh()
		{
			m_currentScore = 0;
			m_score = 0;
			m_delay = DELAY;
			m_scoreText.text = "Score: 0";
		}

		public override void Free( )
		{
			GameController.Instance.OnUpdate -= Update;
			GameController.Instance.board.OnMatch -= OnMatch;
		}

		void Update()
		{
			m_delay -= Time.deltaTime;
			if (m_delay > 0)
				return;

			int delta = m_score - m_currentScore;
			m_currentTime -= Time.deltaTime;
			if (delta > 0 && m_currentTime < 0)
			{
				++m_currentScore;

				m_scoreText.text = "Score: " + m_currentScore;

				m_currentTime = m_time;
			}
		}
	}
}

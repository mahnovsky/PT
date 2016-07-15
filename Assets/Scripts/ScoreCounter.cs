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
		private int		m_currentScore;
		private int		m_score;
		private float	m_time;
		private float	m_currentTime;
		private float	m_delay;

		public event Action<int> OnScoreChange; 

		public int Score
		{
			get { return m_score; }
		}

		public void OnMatch(List<Coin> coins)
		{
			int total = (coins.Count - 2) * 50;
			AddScore ( total );

			int index = Mathf.FloorToInt ( ( float )coins.Count / 2 );
			var pos = coins[index].transform.position ;
			PopupLabelGenerator.Instance.Print (
				total.ToString ( ), pos, Vector2.up, 2f, 1f );
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

		public override void Start()
		{
			m_delay = DELAY;
			GameController.Instance.OnUpdate += Update;
			GameController.Instance.board.OnMatch += OnMatch;

			Refresh();
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

			if (OnScoreChange != null)
				OnScoreChange.Invoke(m_currentScore);
		}

		public override void Free( )
		{
			GameController.Instance.OnUpdate		-= Update;
			GameController.Instance.board.OnMatch	-= OnMatch;
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

				if (OnScoreChange != null)
					OnScoreChange.Invoke(m_currentScore);

				m_currentTime = m_time;
			}
		}
	}
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts;

public class UITopPanel : MonoBehaviour
{
	public Text m_scoreLabel;
	public Text m_levelLabel;

	public Text TotalScore;
	public Text CurrentScore;

	public Text TotalWindows;
	public Text CurrentWindows;
	
	private void Start()
	{
		GameController.Instance.LevelList.
			OnLevelChange += OnLevelChange;

		OnLevelChange(GameController.CurrentLevel);
	}

	void OnLevelChange (Level level) 
	{
		var scoreComp = level.GetComponent<ScoreCounter> ();
		if (scoreComp != null)
			scoreComp.OnScoreChange += OnScoreUpdate;

		var goal = level.GetComponent<GoalController>();
		if ( goal != null )
		{
			if ( goal.TotalWindows > 0 )
			{
				TotalWindows.text = goal.TotalWindows.ToString();

				goal.OnWindowsChange += OnWindowsUpdate;
			}

			if (goal.TotalScore > 0)
			{
				TotalScore.text = goal.TotalScore.ToString();
			}

			OnWindowsUpdate(0);
			OnScoreUpdate(0);
		}

		m_levelLabel.text = level.Number.ToString();
	}

	void OnWindowsUpdate( int nWindows )
	{
		if (CurrentWindows != null)
		{
			CurrentWindows.text = nWindows.ToString();
		}
	}

	void OnScoreUpdate(int nScore)
	{
		if (m_scoreLabel != null) 
		{
			string score = nScore.ToString();
			m_scoreLabel.text = score;
			CurrentScore.text = score;
		}
	}
}

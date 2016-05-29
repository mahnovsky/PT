using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Assets.Scripts;

public class UITopPanel : MonoBehaviour 
{
	public Text m_scoreLabel;
	public Text m_levelLabel;
	// Use this for initialization
	void Start () 
	{
		/*var scoreTransform = transform.Find ("Score");
		var levelTransform = transform.Find ("Level");

		m_scoreLabel = scoreTransform.GetComponent<Text> ();
		m_levelLabel = levelTransform.GetComponent<Text> ();*/

		var level = GameController.CurrentLevel;
		var scoreComp = level.GetComponent<ScoreCounter> ();
		if (scoreComp != null)
			scoreComp.SetText( m_scoreLabel );

		m_levelLabel.text = "Level: " + level.Number;
	}

	void OnScoreUpdate(int nScore)
	{
		if (m_scoreLabel != null) 
		{
			m_scoreLabel.text = "Score: " + nScore;
		}
	}
}

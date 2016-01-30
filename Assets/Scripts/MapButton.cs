using UnityEngine;
using System.Collections;

public class MapButton : MonoBehaviour {

	public Sprite [] progress;

	// Use this for initialization
	void Start () {
		m_pressed = false;
	}
	
	void OnMouseDown() {
		m_pressed = true;
	}

	void OnMouseUp() {
		if (m_pressed) {
			Application.LoadLevel ("Game");
			GameController.LevelNum = Level;
		}

		m_pressed = false;
	}

	public int Level {
		get { return m_level; }
		set { m_level = value; }
	}

	private int m_level;
	private bool m_pressed;
}

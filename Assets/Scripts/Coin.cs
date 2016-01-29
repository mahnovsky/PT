using UnityEngine;
using System.Collections;

public enum eCoinState {
	Created,
	Deleted,
	Idle,
	Moved
}

public class Coin : MonoBehaviour 
{
	private SpriteRenderer 	m_spriteRenderer;

	public void init(int placeId, int x, int y, int coinId, Sprite sp) {

		print ("[Coin] init placeId: " + placeId);

		Level currLevel = Game.Inst.CurrentLevel;

		m_spriteRenderer = GetComponent<SpriteRenderer> ();
		
		m_coinId = coinId;

		m_spriteRenderer.sprite = sp;

		updateLoc (placeId, x, y);
		Deleted = false;

		m_spriteRenderer.enabled = true;
		GetComponent<BoxCollider2D> ().enabled = true;
		refreshPosition ();

		transform.localScale = new Vector3 (0.9f, 0.9f);
	}
	
	public int CoinId {
		get { return m_coinId; }
		set { m_coinId = value; }
	}
	
	public int PlaceId {
		get { return m_placeId; }
	}
	
	public int XPos {
		get { return m_x; }
	}
	
	public int YPos {
		get { return m_y; }
	}

	public Point Position {
		get { return new Point(XPos, YPos); }
	}

	public float Delay {
		get { return m_delay; }
		set { m_delay = value; }
	}

	public Vector3 getRealPosition() {

		//Vector3 size = m_spriteRenderer.sprite.bounds.size;
		Vector3 scaleSize = new Vector3(1.7f, 1.7f);

		return new Vector3 (scaleSize.x * XPos, scaleSize.y * YPos) + (scaleSize / 2.8f);
	}

	public void refreshPosition() {

		transform.localPosition = getRealPosition ();
	}
	
	public void updateLoc(int newIndex, int x, int y) {

		m_placeId = newIndex;
		m_x = x;
		m_y = y;

		string name = m_spriteRenderer.sprite.name + "_" + PlaceId;
		gameObject.name = name;
	}
	
	public bool Deleted {
		get { return m_deleted; }
		set { m_deleted = value; }
	}

	public eCoinState State {
		get { return m_state; }
		set { changeState(value); }
	}

	private void changeState(eCoinState state) {

		if (state == m_state) {
			return;
		}

		if (m_state == eCoinState.Created && (state == eCoinState.Moved || state == eCoinState.Idle)) {
			m_state = state;
		}

		if (m_state == eCoinState.Deleted && state == eCoinState.Created) {
			m_state = state;
		}

		if (m_state == eCoinState.Idle && (state == eCoinState.Moved || state == eCoinState.Deleted)) {
			m_state = state;
		}

		if (m_state == eCoinState.Moved && state == eCoinState.Idle) {
			m_state = state;
		}

		if (m_state != state) {
			Debug.Log("State change fail m_state: " + m_state + ", state: " + state);
		}
	}

	public bool isNeighbour(Coin other) {
	
		int deltaX = Mathf.Abs(XPos - other.XPos);
		int deltaY = Mathf.Abs(YPos - other.YPos);

		return (deltaX == 1 && deltaY == 0) ||
			(deltaX == 0 && deltaY == 1);
	}

	void OnMouseDown() {

		if (State != eCoinState.Idle) {
			return;
		}
		Map m = GameObject.Find ("Map").GetComponent<Map>();

		m.Select = this;
	}

	void OnMouseUp() {

		Map m = GameObject.Find ("Map").GetComponent<Map>();
		
		m.Select = null;
	}

	void OnMouseOver() {
	
		if (State != eCoinState.Idle) {
			Debug.Log("Failed swap coin state: " + State);
			return;
		}

		Map m = GameObject.Find ("Map").GetComponent<Map>();

		if (m.Select == this || m.Select == null) {
			return;
		}
	
		if (isNeighbour (m.Select)) {

			m.swap(this, m.Select);
		}
		else {

			m.Select = null;
		}
	}

	public void setDrop(Map m, Vector3 from) {

		transform.localPosition = from;

		if (m_move == null) {
			m_move = new MoveAction (m, gameObject, 0.2f, getRealPosition (), "drop");
		} 
		else {
			m_move.refresh(getRealPosition(), 0.2f);

			m_move.init();
		}
		State = eCoinState.Moved;
	}

	public void dieWithDelay(float delay) {
		m_delay = delay;
		m_needDie = true;
	}

	void Update() {

		if (m_delay > 0f) {
			m_delay -= Time.deltaTime;
		} 
		else if (m_needDie) {
			State = eCoinState.Deleted;
			Map m = GameObject.Find("Map").GetComponent<Map>();
			m.RemoveList.Add(this);
			m_needDie = false;
		}

		if (m_delay <= 0f && m_move != null) {

			if (!m_move.Done) {

				if( !m_move.Started ) {
					m_move.init();
				}

				m_move.update(Time.deltaTime);

				if (m_move.Done) {
					State = eCoinState.Idle;
				}
			}
		}
	}

	private bool m_needDie;
	private float m_delay;
	private MoveAction m_move;
	private eCoinState m_state;
	private int m_coinId;
	private int m_placeId;
	private int m_x;
	private int m_y;
	private bool m_deleted;
}

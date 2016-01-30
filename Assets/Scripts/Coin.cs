using UnityEngine;
using System.Collections;
using Holoville.HOTween;

public enum eCoinState {
	Created,
	Deleted,
	Idle,
	Moved
}

public class Coin : MonoBehaviour
{
	public static float coinWidth	= 205f * 0.01f;
	public static float coinHeight	= 205f * 0.01f;
	public static float border		= 5f * 0.01f;

	public void init(int placeId, int x, int y, int coinId, Sprite sp)
	{
		print ("[Coin] init placeId: " + placeId);
		
		Level currLevel = GameController.Instance.CurrentLevel;

		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer> ();
		
		m_coinId = coinId;

		spriteRenderer.sprite = sp;

		updateLoc (placeId, x, y);
		Deleted = false;
		State = eCoinState.Created;

		spriteRenderer.enabled = true;
		GetComponent<BoxCollider2D> ().enabled = true;
		refreshPosition ();

		Vector2 size = sp.bounds.size;

		transform.localScale = new Vector3(coinWidth / size.x, coinHeight / size.y);
	}

	public void changeCoinId(int nId)
	{
		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer> ();
		
		CoinId = nId;

		spriteRenderer.sprite = Level.currLevel().coinSprites[nId];
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

	public Vector3 getRealPosition()
	{
		Map m = GameController.Instance.map;
		
		Vector3 offset = new Vector3 (m.CoinOffset.x, m.CoinOffset.y);

		Vector3 pos = new Vector3 ((coinWidth + border / 2) * XPos, (coinHeight + border / 2) * YPos);

		return pos;
	}

	public void refreshPosition() {

		transform.localPosition = getRealPosition ();
	}
	
	public void updateLoc(int newIndex, int x, int y) {

		m_placeId = newIndex;
		m_x = x;
		m_y = y;

		SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer> ();
		string name = spriteRenderer.sprite.name + "_" + PlaceId;
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
		Map map = GameController.Instance.map;

		map.Select = this;
	}

	void OnMouseUp()
	{
		Map map = GameController.Instance.map;
		
		map.Select = null;

		if (Debug.isDebugBuild)
		{
			map.controller.OnCoinTap(this);
		}
	}

	void OnMouseOver() {
	
		if (State != eCoinState.Idle) {
			Debug.Log("Failed swap coin state: " + State);
			return;
		}

		Map map = GameController.Instance.map;

		if (map.Select == this || map.Select == null)
		{
			return;
		}
	
		if (isNeighbour (map.Select))
		{
			map.swap(this, map.Select);
		}
		else
		{
			map.Select = null;
		}
	}

	public void moveTo(Vector3 pos, float delay, string msg) {

		m_move = HOTween.To (transform, delay, new TweenParms()
		            .Prop ("localPosition", pos)
		            .Ease (EaseType.EaseInOutQuad)
		            .OnStepComplete (onActionDone));

		m_msg = msg;

		State = eCoinState.Moved;

		m_move.enabled = false;
	}

	public void moveToSpeedBased(Vector3 pos, float speed, string msg) {
		
		m_move = HOTween.To (transform, speed, new TweenParms()
		            .Prop ("localPosition", pos).SpeedBased() // Position tween (set as relative)
		            .Ease (EaseType.EaseInOutQuad) // Ease
		            .OnStepComplete (onActionDone));
		
		m_msg = msg;
		
		State = eCoinState.Moved;

		m_move.enabled = false;
	}

	public void moveToCell(float delay, string msg) {
		moveTo (getRealPosition (), delay, msg);
	}

	public void dieWithDelay(float delay) {
		m_delay = delay;
		m_needDie = true;
	}

	public void destroy() {

		State = eCoinState.Deleted;
		Map map = GameController.Instance.map;
		map.RemoveList.Add(this);

		GetComponent<SpriteRenderer>().enabled = false;
		GetComponent<BoxCollider2D>().enabled = false;
	}

	void Update() {

		if (m_delay > 0f) {
			m_delay -= Time.deltaTime;
		} 
		else if (m_needDie) {

			destroy();

			m_needDie = false;

			return;
		}

		if (m_delay <= 0 && m_move != null && !m_move.enabled)
		{
			Map map = GameController.Instance.map;
			map.onMoveBegin (this);

			m_move.enabled = true;
		}
	}

	public void onActionDone() {
		State = eCoinState.Idle;

		Map map = GameController.Instance.map;

		map.onMoveDone (this, m_msg);
	}

	public bool Disabled {
		get { return m_disabled; }
		set { m_disabled = value; }
	}

	private bool m_needDie;
	private float m_delay;
	private Tweener m_move;
	private eCoinState m_state;
	private int m_coinId;
	private int m_placeId;
	private int m_x;
	private int m_y;
	private bool m_deleted;
	private bool m_disabled = false;
	private string m_msg;
}
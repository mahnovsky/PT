using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public interface IActionListener {
	
	void onActionBegin (Action action);
	void onActionDone (Action action);
}

public abstract class Action  {

	public Action(IActionListener listener, GameObject target, object userData) {
		m_listener 	= listener;
		m_target 	= target;
		m_userData 	= userData;
	}

	public abstract void init();
	
	public abstract void update (float dt);

	public GameObject Target {
		get { return m_target; }
	}

	public IActionListener Listener {
		get { return m_listener; }
	}

	public bool Done {
		get { return m_done; }
		set { m_done = value; }
	}

	public object UserData {
		get { return m_userData; }
	}

	public bool Started {
		get { return m_started; }
		set { m_started = value; }
	}

	private bool 			m_started;
	private bool 			m_done;
	private IActionListener m_listener;
	private GameObject 		m_target;
	private object 			m_userData;
}

public class DelegateAction : Action {

	public DelegateAction(IActionListener listener, float delay, object userData) 
		:base(listener, null, userData) {

		m_delay = delay;
		m_time 	= 0;
	}

	public override void init() {

		Started = true;
		Listener.onActionBegin(this);
	}
	
	public override void update (float dt) {

		m_time += dt;
		float delta = m_delay - m_time;
		Debug.Log ("Delta " + delta);

		if (delta < 0.00001f) {

			Done = true;
			Listener.onActionDone(this);
		}
	}

	private float 		m_time;
	private float		m_delay;
	private object		m_argument;
}


public class MoveAction : Action {

	public MoveAction(IActionListener listener, GameObject target, float delay, Vector3 movePos, object userData)
		:base (listener, target, userData) {

		m_movePos 	= movePos;
		m_delay 	= delay;
		m_speed 	= 0f;
	}

	public MoveAction(IActionListener listener, GameObject target, Vector3 movePos, float speed, object userData)
	:base (listener, target, userData) {
		
		m_movePos 	= movePos;
		m_speed   	= speed;
		m_delay 	= 0f;
	}

	public void refresh(Vector3 to, float delay) {

		if (!Done) {
			Done = true;
			Listener.onActionDone(this);
		}

		m_movePos = to;
		Done = false;
		Started = false;
		m_delay = delay;
		m_speed = 0f;
	}

	public override void init() {

		Started = true;
		float distance = (m_movePos - Target.transform.localPosition).magnitude;

		if (isEqual (0f, m_delay)) {
			m_delay = distance / m_speed;
		}

		m_direction = (m_movePos - Target.transform.localPosition).normalized;

		if (isEqual (0f, m_speed)) {
			m_speed = distance / m_delay;
		}

		m_fspeed = m_direction * m_speed;

		Listener.onActionBegin (this);
	}

	public Vector3 vecAbs(Vector3 v) {

		return new Vector3 (Mathf.Abs (v.x), Mathf.Abs (v.y));
	}

	bool isEqual(float a, float b) {

		float epsilon = 0.000001f;

		if (a >= b - epsilon && a <= b + epsilon)
			return true;
		else
			return false;
	}

	public bool vecNot(Vector3 v1, Vector3 v2) {
	
		if (isEqual(v1.x, v2.x) && isEqual(v1.y, v2.y)) {
			return false;
		}
		return true;
	}

	public bool check(Vector3 v1, Vector3 v2) {

		return Mathf.Abs (v1.x) <= Mathf.Abs (v2.x) && 
			Mathf.Abs (v1.y) <= Mathf.Abs (v2.y);
	}

	public override void update(float dt) {

		Vector3 pos = Target.transform.localPosition;
		Vector3 deltaPos = m_movePos - pos;
		Vector3 deltaFrame = m_fspeed * dt;

		Target.transform.localPosition = pos + deltaFrame;
		m_delay -= dt;

		if ( check (deltaPos, deltaFrame) || m_delay <= 0) {

			Debug.Log ("Move to: " + m_movePos);
			Target.transform.localPosition = m_movePos;

			Done = true;
			Listener.onActionDone (this);
		}
	}

	private Vector3 	m_movePos;
	private	Vector3		m_direction;
	private float 		m_delay;
	private float	 	m_speed;
	private Vector3 	m_fspeed;
}

public class SequenceAction : Action {

	public SequenceAction(IActionListener listener) 
		:base(listener, null, null) {

		m_actions = new Queue<Action> ();
	}

	public override void init() {

		Started = true;
		Listener.onActionBegin (this);
	}

	public override void update(float dt) {

		if (m_current == null && m_actions.Count > 0) {

			Debug.Log ("Actions count: " + m_actions.Count);
			m_current = m_actions.Dequeue();
			m_current.init();
		}

		if (m_current != null) {

			m_current.update (dt);

			if (m_current.Done) {

				Debug.Log ("Actions count: " + m_actions.Count);
				m_current = null;
			}
		}
		else {

			Done = true;
			Listener.onActionDone (this);
		}
	}

	public SequenceAction addAction(Action action) {

		if (!m_actions.Contains (action)) {
			Debug.Log("add action");
			m_actions.Enqueue (action);
		}

		return this;
	}

	private Action 			m_current;
	private Queue<Action> 	m_actions;
}

public class ActionManager : MonoBehaviour {

	// Use this for initialization
	void Start () {

		m_actions 		= new List<Action> ();
		m_removeList 	= new List<Action> ();
	}
	
	// Update is called once per frame
	void Update () {

		if (m_actions == null) {
			return;
		}

		if (m_initActions != null) {

			foreach (var action in m_initActions) {
				action.init ();
				m_actions.Add (action);
			}

			if (m_initActions.Count > 0) {
				m_initActions.Clear();
			}
		}

		foreach (var action in m_actions) {

			action.update (Time.deltaTime);

			if (action.Done) {
				m_removeList.Add (action);
			}
		}

		foreach (var action in m_removeList) {
			m_actions.Remove(action);

			print ("action removed " + action);
		}

		if (m_removeList.Count > 0) {
			m_removeList.Clear ();
		}
	}


	public void runAction(Action action) {

		if (m_initActions == null) {
			m_initActions = new List<Action>();
		}

		if (!m_initActions.Contains (action)) {
			m_initActions.Add (action);
		} else {
			Debug.Log("bugaga");
		}
	}

	private List<Action> m_actions;
	private List<Action> m_initActions;
	private List<Action> m_removeList;
}

using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using Holoville.HOTween;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Assets.Scripts
{

	public class StateMachine<T>
	{
		public T CurrentState { get; private set; }

		public object UserData { get; private set; }

		private Dictionary<T, List<T>> m_stateToState;

		public Action<StateMachine<T>, T, T> OnStateChange { get; set; }

		public StateMachine(T defaultState, object userData)
		{
			UserData = userData;
			CurrentState = defaultState;
			m_stateToState = new Dictionary<T, List<T>>();
		}

		public void AddState(T state, List<T> toState)
		{
			m_stateToState[state] = toState;
		}

		public bool SetState(T state)
		{
			List<T> states;
			if (m_stateToState.TryGetValue(CurrentState, out states))
			{
				foreach (var state1 in states)
				{
					if (state1.Equals(state))
					{
						OnStateChange(this, CurrentState, state);
						CurrentState = state;

						return true;
					}
				}
			}

			return false;
		}
	}

	public enum eCoinState
	{
		MarkDelete,
		Deleted,
		Idle,
		Moved
	}

	public class Coin : MonoBehaviour
	{
		private static readonly ObjectPool<ParticleSystem> m_effectsPool;
		private ParticleSystem m_effect;
		public GameObject selector;

		private StateMachine<eCoinState> m_stateMachine;
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

		static Coin()
		{
			m_effectsPool = new ObjectPool<ParticleSystem>();
			m_effectsPool.MakeFunc = () =>
			{
				var obj = Instantiate(GameController.Instance.destroyEffect);

				return obj.GetComponentInChildren<ParticleSystem>();
			};
		}

		public StateMachine<eCoinState> StateMachine
		{
			get { return m_stateMachine; }
		}

		public void Init(int placeId, int x, int y, int coinId, Sprite sp)
		{
			print("[Coin] init placeId: " + placeId);
			SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

			m_coinId = coinId;

			spriteRenderer.sprite = sp;

			UpdateLoc(placeId, x, y);

			spriteRenderer.enabled = true;
			GetComponent<BoxCollider2D>().enabled = true;
			RefreshPosition();
			if (m_stateMachine != null)
			{
				m_stateMachine.SetState(eCoinState.Idle);
			}
			else
			{
				m_stateMachine = new StateMachine<eCoinState>(eCoinState.Idle, this);

				m_stateMachine.AddState(eCoinState.Idle,
					new List<eCoinState> {eCoinState.Moved, eCoinState.MarkDelete});

				m_stateMachine.AddState(eCoinState.Moved,
					new List<eCoinState> {eCoinState.Idle});

				m_stateMachine.AddState(eCoinState.MarkDelete,
					new List<eCoinState> {eCoinState.Deleted});

				m_stateMachine.AddState(eCoinState.Deleted,
					new List<eCoinState> {eCoinState.Idle});
			}
		}

		public void OnSpawn(Vector3 spawnOffset, float delay)
		{
			Vector3 pos = GetRealPosition();
			pos.y = spawnOffset.y;
			transform.localPosition = pos;
			var board = GameController.Instance.board;
			MoveToSpeedBased(GetRealPosition(), delay, board.dropSpeed, "drop");
		}

		public void ChangeCoinId(int nId)
		{
			SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();

			CoinId = nId;

			spriteRenderer.sprite = GameController.CoinSprites[nId];
		}

		public int CoinId
		{
			get { return m_coinId; }
			set { m_coinId = value; }
		}

		public int PlaceId
		{
			get { return m_placeId; }
		}

		public int XPos
		{
			get { return m_x; }
		}

		public int YPos
		{
			get { return m_y; }
		}

		public Point Position
		{
			get { return new Point(XPos, YPos); }
		}

		public float Delay
		{
			get { return m_delay; }
			set { m_delay = value; }
		}

		public Vector3 GetRealPosition()
		{
			Board m = GameController.Instance.board;

			return m.GetRealPosition(Position.X, Position.Y);
		}

		public void RefreshPosition()
		{
			transform.localPosition = GetRealPosition();
		}

		public void UpdateLoc(int newIndex, int x, int y)
		{
			m_placeId = newIndex;
			m_x = x;
			m_y = y;

			SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
			string name = spriteRenderer.sprite.name + "_" + PlaceId;
			gameObject.name = name;
		}

		public eCoinState State
		{
			get { return m_stateMachine.CurrentState; }
			set { ChangeState(value); }
		}

		private void ChangeState(eCoinState state)
		{
			if (state == m_stateMachine.CurrentState)
			{
				return;
			}

			if (!m_stateMachine.SetState(state))
			{
				Debug.Log("State change fail m_state: " + m_state + ", state: " + state);
			}
		}

		public bool IsNeighbour(Coin other)
		{

			int deltaX = Mathf.Abs(XPos - other.XPos);
			int deltaY = Mathf.Abs(YPos - other.YPos);

			return (deltaX == 1 && deltaY == 0) ||
			       (deltaX == 0 && deltaY == 1);
		}

		private void OnMouseDown()
		{
			if (State != eCoinState.Idle || GameManager.Pause)
			{
				return;
			}

			Board board = GameController.Instance.board;
			if (board.Select != null)
			{
				board.Select.selector.SetActive(false);
			}

			if (board.Select != null &&
			    board.Select != this &&
			    IsNeighbour(board.Select))
			{
				board.Swap(this, board.Select);
				board.Select = null;
			}
			else
			{
				board.Select = this;
				board.Focused = this;
			}
		}

		private void OnMouseUp()
		{
			if (GameManager.Pause)
			{
				return;
			}
			Board board = GameController.Instance.board;

			board.Focused = null;
			if (board.Select == this)
			{
				selector.SetActive(true);
			}

			if (Debug.isDebugBuild)
			{
				GameController.Instance.OnCoinTap(this);
			}
		}

		private void OnMouseOver()
		{
			if (State != eCoinState.Idle || GameManager.Pause)
			{
				return;
			}

			Board board = GameController.Instance.board;

			if (board.Focused == this || board.Focused == null)
			{
				return;
			}

			if (IsNeighbour(board.Focused))
			{
				board.Swap(this, board.Focused);
			}
			else
			{
				board.Focused = null;
			}

			if (board.Select != null)
			{
				board.Select.selector.SetActive(false);
				board.Select = null;
			}
		}

		public void MoveTo(Vector3 pos, float delay, string msg)
		{
			if (m_stateMachine.SetState(eCoinState.Moved))
			{
				m_move = HOTween.To(transform, delay,
					new TweenParms()
						.Prop("localPosition", pos)
						.Ease(EaseType.Linear)
						.OnComplete(OnActionDone));

				m_msg = msg;

				m_move.enabled = false;
			}
		}

		public void MoveToSpeedBased(Vector3 pos, float delay, float speed, string msg)
		{
			if (m_stateMachine.SetState(eCoinState.Moved))
			{
				m_move = HOTween.To(transform, speed,
					new TweenParms()
						.Prop("localPosition", pos)
						.SpeedBased()
						.Ease(EaseType.Linear)
						.Delay(delay)
						.OnComplete(OnActionDone));

				m_msg = msg;

				m_move.enabled = false;
			}
		}

		public void MoveToCell(float delay, string msg)
		{
			MoveTo(GetRealPosition(), delay, msg);
		}

		public void DieWithDelay(float delay)
		{
			if (m_delay > 0f || State != eCoinState.MarkDelete)
				return;

			m_delay = delay;
			Vector3 pos = transform.position;
			pos.z = -5;

			m_effect = m_effectsPool.MakeNew();
			if (m_effect != null)
			{
				m_effect.transform.position = pos;
				m_effect.Play();
			}
		}

		public void Destroy()
		{
			if (m_stateMachine.SetState(eCoinState.Deleted))
			{
				GetComponent<SpriteRenderer>().enabled = false;
				GetComponent<BoxCollider2D>().enabled = false;
				if (m_effect != null && !m_effect.IsAlive())
				{
					m_effect.Stop();
					m_effectsPool.Delete(m_effect);
					m_effect = null;
				}

				if (m_move != null)
				{
					m_move.Kill();

					m_move = null;
				}

				m_delay = 0f;

				Board board = GameController.Instance.board;

				board.FreeCoin(this);
			}
		}

		private void Update()
		{
			if (m_delay > 0f)
			{
				m_delay -= Time.deltaTime;
			}
			else if (State == eCoinState.MarkDelete)
			{
				Destroy();

				return;
			}

			if (m_delay <= 0 && m_move != null && !m_move.enabled)
			{
				m_move.enabled = true;
			}
		}

		public void OnActionDone()
		{
			if (m_stateMachine.SetState(eCoinState.Idle))
			{
				Board board = GameController.Instance.board;

				board.OnMoveDone(this, m_msg);
			}
		}

		public bool Disabled
		{
			get { return m_disabled; }
			set { m_disabled = value; }
		}
	}
}
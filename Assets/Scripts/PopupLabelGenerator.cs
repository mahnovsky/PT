using System;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts
{
	class PopupLabelGenerator : MonoBehaviour
	{
		private static readonly ObjectPool<PopupLabelInfo> m_labelPool;
		
		static PopupLabelGenerator()
		{
			m_labelPool = new ObjectPool<PopupLabelInfo>();
			m_labelPool.MakeFunc = () =>
			{
				var lp = Instance.LabelPrefab;
				var label = Instantiate(lp);
				label.transform.SetParent(Instance.transform);
				var labelInfo = new PopupLabelInfo
				{
					Label = label,
					MoveSpeed = new Vector2(0, 10),
					LifeTime = 2f,
					Delay = 1f,
					Free = false
				};

				return labelInfo;
			};
		}

		public PopupLabel LabelPrefab;

		public class PopupLabelInfo
		{
			public PopupLabel Label;
			public Vector2	MoveSpeed;
			public float	LifeTime;
			public float	Delay;
			public bool		Free;
		}

		private readonly List<PopupLabelInfo> m_labels = new List<PopupLabelInfo>(); 

		public static PopupLabelGenerator Instance { get; private set; }

		public void Print( String text, Vector3 startPos, 
			Vector2 moveSpeed, float lifeTime, float delay )
		{
			PopupLabelInfo labelInfo = m_labelPool.MakeNew ( );

			labelInfo.LifeTime = lifeTime;
			labelInfo.Delay = delay;
			labelInfo.MoveSpeed = moveSpeed;
			labelInfo.Label.Init ( text, startPos );
		}

		void Awake( )
		{
			if (Instance == null)
			{
				Instance = this;
				m_labelPool.EnableFunc = info =>
				{
					info.Label.gameObject.SetActive(false);
					m_labels.Add(info);
				};

				m_labelPool.DisableFunc = info =>
				{
					info.Label.gameObject.SetActive(false);
					m_labels.Remove(info);
				};
			}
			else
			{
				Destroy(gameObject);
			}
		}

		void Update( )
		{
			if (m_labels.Count == 0)
				return;

			List<PopupLabelInfo> labels = new List<PopupLabelInfo>(m_labels);
			foreach (var popupLabel in labels)
			{
				popupLabel.Delay -= Time.deltaTime;
				if (popupLabel.Delay < 0)
				{
					if (!popupLabel.Label.gameObject.activeSelf)
						popupLabel.Label.gameObject.SetActive(true);

					popupLabel.Label.AddPosition(popupLabel.MoveSpeed * Time.deltaTime);

					popupLabel.LifeTime -= Time.deltaTime;
				}

				if ( popupLabel.LifeTime < 0 )
				{
					m_labelPool.Delete(popupLabel);
				}
			}
		}
	}
}

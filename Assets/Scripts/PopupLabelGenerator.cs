using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
	class PopupLabelGenerator : MonoBehaviour
	{
		public PopupLabel LabelPrefab;

		public class PopupLabelInfo
		{
			public PopupLabel Label;
			public Vector2	MoveSpeed;
			public float	LifeTime;
			public float	Delay;
			public bool		Free;
		}

		private List<PopupLabelInfo> m_labels = new List<PopupLabelInfo>(); 
		private List<PopupLabelInfo> m_freeLabels = new List<PopupLabelInfo>(); 
		public static PopupLabelGenerator Instance { get; private set; }

		public void Print( String text, Vector3 startPos, 
			Vector2 moveSpeed, float lifeTime, float delay )
		{
			PopupLabelInfo labelInfo = FindFree();
			if (labelInfo != null)
			{
				labelInfo.Free = false;
				labelInfo.LifeTime = lifeTime;
				labelInfo.Delay = delay;
				labelInfo.MoveSpeed = moveSpeed;
				labelInfo.Label.Init( text, startPos );
			}
			else
			{
				var label = Instantiate(LabelPrefab, startPos, Quaternion.identity) as PopupLabel;
				label.Init( text, startPos );
				label.transform.parent = transform;
				labelInfo = new PopupLabelInfo
				{
					Label = label,
					MoveSpeed = moveSpeed,
					LifeTime = lifeTime,
					Delay = delay,
					Free = false
				};

				m_labels.Add(labelInfo);
			}
		}

		PopupLabelInfo FindFree( )
		{
			if ( m_freeLabels.Count > 0 )
			{
				var lastIndex = m_freeLabels.Count - 1;
				var pl = m_freeLabels[lastIndex];

				m_freeLabels.RemoveAt(lastIndex);

				return pl;
			}

			return null;
		}

		void Awake( )
		{
			if (Instance == null)
			{
				Instance = this;

				DontDestroyOnLoad( gameObject );
			}
			else
			{
				Destroy(gameObject);
			}
		}

		void Update( )
		{
			foreach (var popupLabel in m_labels)
			{
				if ( popupLabel.Free )
					continue;

				popupLabel.Delay -= Time.deltaTime;
				if (popupLabel.Delay < 0)
				{
					var go = popupLabel.Label.gameObject;
					if (!go.activeSelf)
						go.SetActive(true);

					popupLabel.Label.AddPosition(popupLabel.MoveSpeed * Time.deltaTime);

					popupLabel.LifeTime -= Time.deltaTime;
				}

				if ( popupLabel.LifeTime < 0 )
				{
					popupLabel.Free = true;
					m_freeLabels.Add(popupLabel);
				}
			}

			foreach (var popupLabelInfo in m_freeLabels)
			{
				popupLabelInfo.Label.gameObject.SetActive(false);
			}
		}
	}
}

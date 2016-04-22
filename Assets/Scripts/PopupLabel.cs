using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	class PopupLabel : MonoBehaviour
	{
		private RectTransform m_transform;

		public void Init( String text, Vector3 startPos )
		{
			var uiText = GetComponent<Text>();
			uiText.text = text;

			m_transform = GetComponent<RectTransform>();

			m_transform.position = startPos;
		}

		public void AddPosition( Vector3 addPos )
		{	
			m_transform.position += addPos;
		}

	}
}

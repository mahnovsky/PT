using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
	class Scaler : MonoBehaviour
	{
		public SpriteRenderer toSprite;
		public Vector2 Size;

		void Awake( )
		{
			var size = toSprite.sprite.bounds.size;

			var scaleX = size.x/ Size.x;

			var scaleY = size.y/Size.y;

			transform.localScale = new Vector3(scaleX, 0.5f);
		}

	}
}

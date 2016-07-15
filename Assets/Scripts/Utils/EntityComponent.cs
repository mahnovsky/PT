using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Utils
{
	public class EntityComponent
	{
		public Entity OwnerEntity { get; private set; }

		public virtual void Init( Entity owner )
		{
			OwnerEntity = owner;
		}

		public virtual void Start( )
		{
		}

		public virtual void Refresh( )
		{
		}

		public virtual void Free( )
		{
		}

		public virtual void Load( JSONObject obj )
		{
		}

		public virtual void Save( JSONObject obj )
		{
		}
	}
}

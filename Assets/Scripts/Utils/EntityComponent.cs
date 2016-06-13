using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Utils
{
	public class EntityComponent
	{
		public virtual void Init( )
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

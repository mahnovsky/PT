using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Utils
{
	public class Entity
	{
		private readonly Dictionary<string, EntityComponent>		m_components;
		private readonly Dictionary<string, Func<EntityComponent>>	m_componentCreators;

		public Entity( )
		{
			m_components = new Dictionary<string, EntityComponent>();
			m_componentCreators = new Dictionary<string, Func<EntityComponent>>();
		}

		public void Init( )
		{
			foreach (var pair in m_components)
			{
				pair.Value.Init (this);

				pair.Value.Start();
			}
		}

		public void RegistryComponent<T>( ) where T : EntityComponent, new()
		{
			m_componentCreators.Add ( typeof ( T ).Name, ( ) => new T ( ) );
		}

		public T GetComponent<T>( ) where T : EntityComponent
		{
			EntityComponent comp;
			if ( m_components.TryGetValue ( typeof ( T ).Name, out comp ) )
			{
				return ( T )comp;
			}

			return null;
		}

		public void AddComponent<T>() where T : EntityComponent, new()
		{
			EntityComponent comp = new T();
			comp.Init(this);
			m_components.Add ( typeof(T).Name, comp );
		}

		public virtual void Refresh( )
		{
			foreach ( var comp in m_components )
			{
				comp.Value.Refresh ( );
			}
		}

		public void Free( )
		{
			foreach ( var comp in m_components )
			{
				comp.Value.Free ( );
			}
		}

		public void Load( JSONObject obj )
		{
			for ( int i = 0; i < obj.list.Count; ++i )
			{
				string ckey = obj.keys[i];
				JSONObject j = obj.list[i];

				if ( j.type == JSONObject.Type.OBJECT )
				{
					Func<EntityComponent> creator;
					if ( m_componentCreators.TryGetValue ( ckey, out creator ) )
					{
						var comp = creator.Invoke ( );

						comp.Load(j);

						m_components.Add ( ckey, comp );
					}
				}
			}
		}
	}
}

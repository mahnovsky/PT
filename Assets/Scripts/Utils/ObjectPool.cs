using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Utils
{
	class ObjectPool<T> where T : class
	{
		class Object
		{
			public T Obj { get; set; }
			public int NextFree { get; set; }
			public int Index { get; set; }
		}

		private Dictionary<T, Object> m_map = new Dictionary<T, Object>(); 
		private List<Object>	m_objects = new List<Object>();
		private int				m_lastFree = -1;

		public Func<T> MakeFunc { get; set; }
		public Action<T> EnableFunc { get; set; }
		public Action<T> DisableFunc { get; set; } 

		public T MakeNew()
		{
			T inst = default(T);
			if (m_objects.Count <= 0 || m_lastFree < 0)
			{
				inst = MakeFunc();
				var obj = new Object {Obj = inst, NextFree = -1, Index = m_objects.Count};
				m_objects.Add(obj);
				m_map.Add(inst, obj);
			}
			else if (m_lastFree >= 0)
			{
				Object obj = m_objects[m_lastFree];
				inst = obj.Obj;
				m_lastFree = obj.NextFree;
				obj.NextFree = -1;
			}

			if (inst != null && EnableFunc != null)
			{
				EnableFunc.Invoke(inst);
			}

			return inst;
		}

		Object Find(T inst)
		{
			Object res;
			if (m_map.TryGetValue(inst, out res))
			{
				return res;
			}

			return null;
		}

		public void Delete(T inst)
		{
			Object obj = Find(inst);

			if ( obj != null )
			{
				if (m_lastFree >= 0)
				{
					obj.NextFree = m_lastFree;
					m_lastFree = obj.Index;
				}
				else
				{
					m_lastFree = obj.Index;
				}

				if (DisableFunc != null)
				{
					DisableFunc.Invoke(obj.Obj);
				}
			}
		}
	}
}

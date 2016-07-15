using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.Utils
{
	public interface IJsonReader
	{
		void Read(JSONObject obj);
	}

	public static class JsonHelper
	{
		public static int GetInt(this JSONObject obj, string fieldName)
		{
			JSONObject j = obj.GetField ( fieldName );
			if ( j != JSONObject.nullJO && j.IsNumber )
			{
				return ( int )j.n;
			}

			throw new Exception ( "Field " + fieldName + " not found." );
		}

		public static List<T> GetArray<T>( this JSONObject obj, string fieldName ) where T : IJsonReader, new()
		{
			JSONObject arrObj = obj.GetField ( fieldName );

			if ( arrObj != JSONObject.nullJO && arrObj.IsArray )
			{
				List<T> res = new List<T> ( );
				foreach ( JSONObject j in arrObj.list )
				{
					T elem = new T();
					
					elem.Read(j);

					res.Add(elem);
				}

				return res;
			}

			return null;
		}

		public static List<string> GetArray( this JSONObject obj, string fieldName )
		{
			JSONObject arrObj = obj.GetField ( fieldName );

			if ( arrObj != JSONObject.nullJO && arrObj.IsArray )
			{
				return arrObj.list.Select(j => j.str).ToList();
			}

			return null;
		}
	}
}

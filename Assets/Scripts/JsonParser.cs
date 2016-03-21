using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class JsonParser
{
	private readonly Dictionary<string, Action<string, JSONObject>> m_parseActions;
	private readonly Dictionary<string, JsonParser> m_parsers;
	public JsonParser Anonymous { get; set; }
	public Action<JSONObject> OnInit { get; set; }

	public JsonParser()
	{
		m_parseActions = new Dictionary<string, Action<string, JSONObject>>();
		m_parsers = new Dictionary<string, JsonParser>();
	}

	public void AddFunc( string key, Action<string, JSONObject> func )
	{
		m_parseActions.Add(key, func);
	}

	public void AddParser( string key, JsonParser parser )
	{
		m_parsers.Add(key, parser);
	}

	public void ParseObject(JSONObject obj)
	{
		if (obj.type != JSONObject.Type.OBJECT)
		{
			return;
		}

		if (OnInit != null)
		{
			OnInit(obj);
		}

		for ( int i = 0; i < obj.list.Count; ++i )
		{
			string ckey = (string) obj.keys[i];
			JSONObject j = (JSONObject) obj.list[i];
			Debug.Log( ckey );

			ParseField( ckey, j );
		}
	}

	public void ParseArray( JSONObject obj )
	{
		foreach(var j in obj.list)
		{
			ParseField("", j);
		}
	}

	void ParseField(string key, JSONObject obj)
	{
		Action<string, JSONObject> func;
		if ( m_parseActions.TryGetValue ( key, out func ) )
		{
			func ( key, obj );

			return;
		}

		if (obj.type == JSONObject.Type.OBJECT)
		{
			JsonParser parser;
			if (key == "" && Anonymous != null)
			{
				Anonymous.ParseObject(obj);
				return;
			}

			if (!m_parsers.TryGetValue(key, out parser))
			{
				ParseObject(obj);
				return;
			}

			if ( parser != null )
			{
				parser.ParseObject(obj);
			}
		}
	}
}

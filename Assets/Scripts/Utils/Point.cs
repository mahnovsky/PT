using System;
using System.Collections;
using System.Runtime.Serialization;
using Assets.Scripts.Utils;

[Serializable]
public class Point : IJsonReader
{	
	public Point()
	{
		m_x = 0;
		m_y = 0;
	}
	
	public Point(int x, int y)
	{
		m_x = x;
		m_y = y;
	}
	
	public int X
	{
		get { return m_x; }
		set { m_x = value; }
	}
	
	public int Y
	{
		get { return m_y; }
		set { m_y = value; }
	}

	public void Read( JSONObject obj )
	{
		X = obj.GetInt("x");
		Y = obj.GetInt("y");
	}

	
	public int m_x;
	public int m_y;
}

[Serializable]
public class Pointf
{	
	public Pointf()
	{
		m_x = 0;
		m_y = 0;
	}
	
	public Pointf(float x, float y)
	{
		m_x = x;
		m_y = y;
	}
	
	public float X
	{
		get { return m_x; }
		set { m_x = value; }
	}
	
	public float Y
	{
		get { return m_y; }
		set { m_y = value; }
	}
	
	public float m_x;
	public float m_y;
}

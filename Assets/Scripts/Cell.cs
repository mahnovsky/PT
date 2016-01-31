using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;


public enum NeighbourPos
{
	Left 	= 0, 
	Right 	= 1,
	Top 	= 2,
	Bottom 	= 3,
	Count 	= 4 
}

public class Cell
{
	public Cell(int index, Point pos, bool empty)
	{
		Index = index;
		Position = pos;
		Empty = empty;

		Neighbours = new Cell[(int)NeighbourPos.Count];
	}

	public int Index
	{
		get { return m_index; }
		private set { m_index = value; }
	}

	public Point Position
	{
		get { return m_position; }
		private set { m_position = value; }
	}

	public bool Empty
	{
		get { return m_empty; }
		set { m_empty = value; }
	}

	public Coin CoinRef
	{
		get { return m_coin; }
		set { m_coin = value; }
	}

	public void setNeighbour(NeighbourPos pos, Cell nb)
	{
		Neighbours [(int)pos] = nb; 
	}

	public Cell[] Neighbours
	{
		get { return m_neighbours; }
		private set { m_neighbours = value; }
	}

	public int getCoinCount (NeighbourPos pos)
	{
		if (Empty)
		{
			return 0;
		}

		Cell nb = Neighbours [(int)pos];

		if (nb == null)
		{
			return 0;
		}
		try
		{
			if (!nb.Empty && 
			    nb.CoinRef != null &&
			    nb.CoinRef.CoinId == CoinRef.CoinId)
			{
				return nb.getCoinCount(pos) + 1;
			}
		}
		catch(Exception exc)
		{
			Debug.Log("Exception: " + exc.Message);
		}

		return 0;
	}

	private Cell[] 	m_neighbours;
	private int 	m_index;
	private Point 	m_position;
	private Coin 	m_coin;
	private bool 	m_empty;
}
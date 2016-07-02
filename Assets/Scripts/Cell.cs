using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;


public enum NeighbourPos
{
	Left 	= 0, 
	Right 	= 1,
	Top 	= 2,
	Bottom 	= 3,
	Count 	= 4 
}

public class Cell : MonoBehaviour
{
	public static readonly float Width = 75;
	public static readonly float Height = 75;

	public void Init(int index, Point pos, bool empty)
	{
		Index = index;
		Position = pos;
		Empty = empty;

		Neighbours = new Cell[(int)NeighbourPos.Count];
		int offset = 0;
		if (Position.Y % 2 == 0)
			offset = 1;
		var two = (Position.X % 2) == offset;
		if (two)
		{
			var render = GetComponent<SpriteRenderer>();
			render.color = new Color(0, 0, 0, 100f / 255);
		}
	}

	public int Index { get; private set; }

	public Point Position { get; private set; }

	public bool Empty { get; set; }

	public Coin CoinRef { get; set; }

	public void SetNeighbour(NeighbourPos pos, Cell nb)
	{
		Neighbours [(int)pos] = nb; 
	}

	public Cell[] Neighbours { get; private set; }

	public int GetCoinCount (NeighbourPos pos)
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
				return nb.GetCoinCount(pos) + 1;
			}
		}
		catch(Exception exc)
		{
			Debug.Log("Exception: " + exc.Message);
		}

		return 0;
	}
}
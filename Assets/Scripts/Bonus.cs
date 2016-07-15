using UnityEngine;
using System.Collections;
using Assets.Scripts;

public class Bonus
{
	protected Coin m_coin;

	public Bonus( Coin coin )
	{
		m_coin = coin;
	}

	public virtual void Exec () {}
}

enum LineOrient
{
	Horizontal	= 0, 
	Vertical	= 1
}

public class BurnLineBonus : Bonus
{
	LineOrient 	m_orient;
	Point 	[]  m_offsets;

	public BurnLineBonus( Coin coin )
		:base(coin)
	{
	}

	Point GetStartPoint()
	{
		if (m_orient == LineOrient.Horizontal) 
		{
			return new Point (0, m_coin.YPos);
		}

		if (m_orient == LineOrient.Vertical) 
		{
			return new Point (m_coin.YPos, 0);
		}

		return new Point ();
	}

	public override void Exec ()
	{
		base.Exec ();

		var map = GameController.Instance.board;

		var count = m_orient == LineOrient.Horizontal ?
			map.Width : map.Height;

		Point pos = GetStartPoint ();
		Point step = m_offsets[(int)m_orient];
		for (int i = 0; i < count; ++i) 
		{
			map.getCoin (pos.X, pos.Y);

		}
	}
}

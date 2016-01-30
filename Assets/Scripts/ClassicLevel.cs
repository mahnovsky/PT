using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ClassicStrategy : GameStrategy {
	public ClassicStrategy(Map map, int [] matches) 
	: base(map) {
		m_matchesCount = matches;
	}
	
	public override Coin coinForIndex(bool init, int index) {
		
		return GameMap.createRandomCoin (index);
	}

	public override void onMatch(int cid, int count) {

		if (cid > m_matchesCount.Length || cid < 0) {
			return;
		}

		if (m_matchesCount [cid] > 0) {
			--m_matchesCount [cid];
		}

		bool done = false;
		foreach (int index in m_matchesCount) {
			if (index > 0) {
				done = true;
				break;
			}
		}

		if (!done) {
			GameController.Instance.OnLevelEnd();
		}
	} 

	int [] m_matchesCount;
}

public class ClassicLevel : Level {


	public int matchesCount = 2;

	public override void init() {
		LevelMode = Mode.Classic;
	}

	public override GameStrategy getStrategy(Map map) {

		int [] coins = new int[coinSprites.Length];
		for (int i = 0; i < coins.Length; ++i) {
			coins[i] = matchesCount;
		}

		return new ClassicStrategy(map, coins);
	}
}

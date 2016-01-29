using UnityEngine;
using System.Collections;

public class ClassicLevel : Level {


	public override void init() {
		LevelMode = Mode.Classic;
	}

	public override GameStrategy getStrategy(Map map) {
		return new GameStrategy(map);
	}
}

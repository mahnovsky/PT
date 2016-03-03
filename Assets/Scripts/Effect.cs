using UnityEngine;
using System.Collections;

public class Effect : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		var ps = GetComponent<ParticleSystem>();
		var rend = ps.GetComponent<Renderer>();

		rend.sortingLayerName = "Effects";
	}
}

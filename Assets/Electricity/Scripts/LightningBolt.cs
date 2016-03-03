using UnityEngine;
using System.Collections;

public class LightningBolt : MonoBehaviour
{
	void Start ()
	{
		var rend = GetComponent<Renderer>();

		Material newMat = rend.material;
		newMat.SetFloat("_StartSeed",Random.value*1000);
		rend.material = newMat;
	
		rend.sortingLayerName = "Effects";
	}
}


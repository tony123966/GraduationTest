using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlatformController : MonoBehaviour {

	public void SetPlatform<T>(T thisGameObject, int edgeCount, float radius, int rotateAngle) where T : Component
	{
		Vector2 center = thisGameObject.transform.position;
		float piDouble = Mathf.PI * 2;
		float sliceUnit = (piDouble / edgeCount);

		int count = 0;
		for (float i = 0; i < edgeCount; i ++)
		{
			float ansX = (radius * Mathf.Cos(-i * sliceUnit)); //求X座標
			float ansY = (radius * Mathf.Sin(-i * sliceUnit)); //求Y座標

			ansX = (Mathf.Cos(-rotateAngle) * ansX - Mathf.Sin(-rotateAngle) * ansY) + center.x;
			ansY = (Mathf.Sin(-rotateAngle) * ansX + Mathf.Cos(-rotateAngle) * ansY) + center.y;
		}
	}
}

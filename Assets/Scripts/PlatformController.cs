using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlatformController:MonoBehaviour
{

	public GameObject platform = null;

	//Platform**************************************************************************
	public enum PlatformType { };

	public float platformFrontWidth = 30;
	public float platformFrontLength = 40;

	public float platformHeight = 5;

	public Vector3 platformCenter;
	//**********************************************************************************
	[HideInInspector]
	public List<Vector3> topPointPosList = new List<Vector3>();
	[HideInInspector]
	public List<Vector3> bottomPointPosList=new List<Vector3>();

	public bool isCurvePlatform = true;
	public bool isStair = true;
	//***********************************************************************

	public void InitFunction(GameObject parent, Vector3 platformCenter)
	{

		this.platformCenter = platformCenter;

		platform = new GameObject("Platform");
		platform.transform.position = platformCenter;
		platform.transform.parent = parent.transform;
		//***********************************************************************
		CreatePlatform(platformCenter);
		if (isStair) 
		{
			for (int i = 0; i < topPointPosList.Count; i++)
			{
				CreateStair(i, platformFrontWidth * 0.8f, platformHeight, platformFrontWidth * 0.1f);
			}
		}
	}
	public void CreatePlatform(Vector3 pos)
	{

		GameObject platformBody = new GameObject("PlatformBody");
		platformBody.transform.position = pos;
		platformBody.transform.parent = platform.transform;
		MeshFilter meshFilter = platformBody.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = platformBody.AddComponent<MeshRenderer>();
		meshRenderer.material.color = Color.white;

		List<Vector3> controlPointPosList = new List<Vector3>();
		controlPointPosList.Clear();

		//初始值******************************************************************************
		float platformRadius = (platformFrontWidth / 2.0f) / Mathf.Sin(2f * Mathf.PI / ((int)MainController.Instance.sides * 2));
		//***********************************************************************************

		if (isCurvePlatform)
		{
			List<Vector3> localPosList = new List<Vector3>();
			localPosList.Add(new Vector3(15, 2, 15));
			localPosList.Add(new Vector3(16, 0, 16));
			localPosList.Add(new Vector3(13, -3, 13));
			localPosList.Add(new Vector3(18, -8, 18));
			controlPointPosList = MainController.Instance.CreateRegularCurveRingMesh(pos, localPosList, Vector3.up, (int)MainController.Instance.sides, 100, 360.0f / (int)MainController.Instance.sides / 2, meshFilter);
			platformHeight = Mathf.Abs(localPosList[0].y - localPosList[localPosList.Count - 1].y);
		}
		else
		{
			if (MainController.Instance.sides == MainController.FormFactorSideType.FourSide)
			{
				controlPointPosList = MainController.Instance.CreateCubeMesh(pos, platformFrontWidth, platformHeight, platformFrontLength, -90, meshFilter);
			}
			else
			{
				controlPointPosList = MainController.Instance.CreateRegularRingMesh(pos, (int)MainController.Instance.sides, platformRadius, platformHeight, 360.0f / (int)MainController.Instance.sides / 2, meshFilter);
			}
		}

		bottomPointPosList.Clear();
		topPointPosList.Clear();
		for (int i = 0; i < (int)MainController.Instance.sides; i++)
		{
			bottomPointPosList.Add(controlPointPosList[i]);
			//topPointPosList.Add(controlPointPosList[i + (int)MainController.Instance.sides]);
			topPointPosList.Add(controlPointPosList[(controlPointPosList.Count - 1) - ((int)(MainController.Instance.sides - 1)) + i]);
		}
	}
	public void CreateStair(int index, float width, float height, float length)
	{
		GameObject stair = new GameObject("Stair");
		stair.transform.position = platformCenter;
		stair.transform.parent = platform.transform;
		MeshFilter meshFilter = stair.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = stair.AddComponent<MeshRenderer>();
		meshRenderer.material.color = Color.white;
		Vector3 dir = Vector3.Cross(Vector3.up, topPointPosList[(index + 1) % topPointPosList.Count] - topPointPosList[index]).normalized;
		Vector3 pos = (topPointPosList[index] + topPointPosList[(index + 1) % topPointPosList.Count]) / 2.0f;
		pos += dir * length / 2.0f;
		pos.y = (topPointPosList[index].y + bottomPointPosList[index].y) / 2.0f;
		float rotateAngle = (Vector3.Dot(Vector3.right, dir) > 0 ?1:-1)* Vector3.Angle(dir, Vector3.forward);

		MainController.Instance.CreateStairMesh(pos, width, height, length, rotateAngle, meshFilter);
	}
}

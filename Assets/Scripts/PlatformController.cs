using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlatformController : Singleton<PlatformController>
{

	public GameObject platform = null;

	//Platform**************************************************************************
	public enum PlatformType { };

	public float platformFrontWidth = 10;
	public float platformFrontLength = 30;

	public float platformHeight=5;

	public Vector3 platformCenter = Vector3.zero;
	//**********************************************************************************
	public List<Vector3> topPointPosList;
	public List<Vector3> bottomPointPosList;


	//***********************************************************************

	public void InitFunction()
	{
		platform = new GameObject("Platform");
		platform.transform.position = platformCenter;
		platform.transform.parent = MainController.Instance.building.transform;
		//***********************************************************************
		CreatePlatform(platformCenter);
		CreateStair();
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
		switch (MainController.Instance.formFactorType)
		{

			case MainController.FormFactorType.RegularRing:

				//初始值******************************************************************************
				float platformRadius = (platformFrontWidth / 2.0f) / Mathf.Sin(2f * Mathf.PI / ((int)MainController.Instance.sides * 2));
				//float platformRadius = platformFrontWidth;

				//***********************************************************************************
				controlPointPosList = MainController.Instance.CreateRegularRingMesh(pos, (int)MainController.Instance.sides, platformRadius, platformHeight, 360.0f / (int)MainController.Instance.sides/2, meshFilter);
				break;
			case MainController.FormFactorType.FreeQuad:

				controlPointPosList = MainController.Instance.CreateCubeMesh(pos, platformFrontWidth, platformHeight, platformFrontLength, 0, meshFilter);
				break;
		}
		bottomPointPosList.Clear();
		topPointPosList.Clear();
		for (int i = 0; i < (int)MainController.Instance.sides; i++)
		{
			bottomPointPosList.Add(controlPointPosList[i]);
			topPointPosList.Add(controlPointPosList[i + (int)MainController.Instance.sides]);
		}
		topPointPosList.Reverse();
		bottomPointPosList.Reverse();;
	}
	public void CreateStair()
	{
		GameObject stair = new GameObject("Stair");
		stair.transform.position = platformCenter;
		stair.transform.parent = platform.transform;
		MeshFilter meshFilter = stair.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer = stair.AddComponent<MeshRenderer>();
		meshRenderer.material.color = Color.white;
		MainController.Instance.CreateStairMesh(new Vector3(50,0,0), 10, 10, 10, 0, meshFilter);

	}
}

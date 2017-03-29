using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlatformController : Singleton<PlatformController>
{

	public GameObject platform = null;
	MeshFilter meshFilter;
	MeshRenderer meshRenderer;

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
		meshFilter = platform.AddComponent<MeshFilter>();
		meshRenderer = platform.AddComponent<MeshRenderer>();
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
				controlPointPosList = MainController.Instance.CreateRegularRingMesh(platformCenter, (int)MainController.Instance.sides, platformRadius, platformHeight, 45,meshFilter,platform);
				break;
			case MainController.FormFactorType.FreeQuad:

				controlPointPosList = MainController.Instance.CreateCubeMesh(platformCenter, platformFrontWidth, platformHeight, platformFrontLength, 0, meshFilter);
				break;
		}
		bottomPointPosList.Clear();
		topPointPosList.Clear();
		for(int i=0;i<(int)MainController.Instance.sides;i++)
		{
			bottomPointPosList.Add(controlPointPosList[i]);
			topPointPosList.Add(controlPointPosList[i + (int)MainController.Instance.sides]);
		}
	}
	
}

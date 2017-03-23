using UnityEngine;
using System.Collections;

public class MainController : Singleton<MainController>
{
	public Vector3 buildingCenter = Vector3.zero;
	public  GameObject building;

	//FormFactor***********************************************************************
	public enum FormFactorType { ThreeSide = 3, FourSide = 4, FiveSide = 5, SixSide = 6, EightSide=8 };
	public FormFactorType sides = FormFactorType.ThreeSide;
	//**********************************************************************************


	// Use this for initialization

	private void Awake()
	{
		InitFunction();
	}
	public void InitFunction()
	{

		building = new GameObject("Building");
		building.transform.position = buildingCenter;

		PlatformController.Instance.InitFunction();
		BodyController.Instance.InitFunction();
		RoofController.Instance.InitFunction();
	}
	
}

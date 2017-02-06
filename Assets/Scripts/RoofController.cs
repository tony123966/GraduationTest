using UnityEngine;
using System.Collections;
using System.Collections.Generic;
struct RidgeStruct
{
	GameObject ridge;
	CatLine ridgeCatLine;
}
public class RoofController : MonoBehaviour {
	private static RoofController instance;
	public static RoofController Instance
	{
		get
		{
			return instance;
		}
	}
	//主脊***********************************************************************
	public List<GameObject> mainRidgeList = new List<GameObject>();
	//**********************************************************************************
	RoofController() 
	{ 
		instance=this;
	}
	public void InitFunction()
	{
		CreateRidge();
	}
	public void CreateRidge()
	{
	
	
	}
}

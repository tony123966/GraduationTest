using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public struct RidgeStruct
{
	public GameObject body;
	public CatLine ridgeCatLine;
}
public class RoofController : Singleton<RoofController>
{

	public GameObject roof = null;

	//主脊***********************************************************************
	public enum Zan_Jian_Ding_MainRidge_CP { Rafter_Start, an, EaveColumn_Purlin, Rafter_End };
	//public enum Wu_Dian_Ding_MainRidge_CP { Ridge_Purlin, an, EaveColumn_Purlin, Flying_Rafter };
	public List<RidgeStruct> mainRidgeList = new List<RidgeStruct>();
	public List<RidgeStruct> eaveList = new List<RidgeStruct>();
	//**********************************************************************************

	public void InitFunction()
	{
		MainController.Instance.allJijaHeight = MainController.Instance.eaveColumnHeight * 0.8f;
		MainController.Instance.eave2eaveColumnOffset = MainController.Instance.eaveColumnHeight*0.3f;
		MainController.Instance.roofTopCenter = MainController.Instance.bodyCenter + new Vector3(0, MainController.Instance.eaveColumnHeight / 2.0f + MainController.Instance.allJijaHeight, 0);

		MainController.Instance.tileLength = MainController.Instance.eaveColumnHeight*0.1f;

		roof = new GameObject("Roof");
		roof.transform.position = MainController.Instance.roofTopCenter;
		roof.transform.parent = MainController.Instance.building.transform;

		CreateAllMainRidge();
	}
	public GameObject CreateControlPoint(GameObject parentObj, Vector3 worldPos, string name = "ControlPoint")
	{
		GameObject newControlPoint = new GameObject(name);
		newControlPoint.transform.position = worldPos;
		newControlPoint.transform.parent = parentObj.transform;
		return newControlPoint;
	}
	public RidgeStruct CreateRidge(string name="MainRidge" )
	{
		RidgeStruct newRidgeStruct=new RidgeStruct();
		newRidgeStruct.body = new GameObject(name);
		newRidgeStruct.body.transform.parent=roof.transform;
		newRidgeStruct.ridgeCatLine = newRidgeStruct.body.AddComponent<CatLine>();

		return newRidgeStruct;
	}
	public RidgeStruct CreateEave(string name = "Eave") 
	{
		RidgeStruct newRidgeStruct = new RidgeStruct();
		newRidgeStruct.body = new GameObject(name);
		newRidgeStruct.body.transform.parent = roof.transform;
		newRidgeStruct.ridgeCatLine = newRidgeStruct.body.AddComponent<CatLine>();

		return newRidgeStruct;
	}
	public void CreateAllMainRidge()
	{
		switch (MainController.Instance.roofType) 
		{ 
			//攢尖頂
			case MainController.RoofType.Zan_Jian_Ding:

			//MainRidge
			mainRidgeList.Clear();
			for(int i=0;i<(int)MainController.Instance.sides;i++)
			{
				RidgeStruct newRidgeStruct=CreateRidge();
				mainRidgeList.Add(newRidgeStruct);
				//TopControlPoint
				Vector3 topControlPointPos=MainController.Instance.roofTopCenter;
				GameObject topControlPoint = CreateControlPoint(newRidgeStruct.body, topControlPointPos);
				//Connect2eaveColumnControlPoint
				Vector3 connect2eaveColumnControlPointPos = MainController.Instance.bodyController.eaveColumnList[i].topPos;
				GameObject connect2eaveColumnControlPoint = CreateControlPoint(newRidgeStruct.body, connect2eaveColumnControlPointPos);
				//DownControlPoint
				Vector3 eave2eaveColumnOffsetVector=Vector3.Normalize(new Vector3(MainController.Instance.bodyController.eaveColumnList[i].topPos.x - MainController.Instance.roofTopCenter.x, 0, MainController.Instance.bodyController.eaveColumnList[i].topPos.z - MainController.Instance.roofTopCenter.z))*MainController.Instance.eave2eaveColumnOffset;
				Vector3 downControlPointPos = MainController.Instance.bodyController.eaveColumnList[i].topPos + eave2eaveColumnOffsetVector;
				GameObject downControlPoint = CreateControlPoint(newRidgeStruct.body, downControlPointPos);
				//MidControlPoint
				Vector3 midControlPointPos = (topControlPointPos + connect2eaveColumnControlPointPos)/2.0f;
				GameObject midControlPoint = CreateControlPoint(newRidgeStruct.body, midControlPointPos);

				newRidgeStruct.ridgeCatLine.controlPointList.Add(topControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(midControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(connect2eaveColumnControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(downControlPoint);


				newRidgeStruct.ridgeCatLine.SetCatmullRom(MainController.Instance.tileLength);
			}

			//Eave
			eaveList.Clear();
			for(int i=0;i<(int)MainController.Instance.sides;i++)
			{
				RidgeStruct newRidgeStruct = CreateEave();
				mainRidgeList.Add(newRidgeStruct);
				//StartControlPoint
				Vector3 startControlPointPos = mainRidgeList[i].ridgeCatLine.controlPointList[mainRidgeList[i].ridgeCatLine.controlPointList.Count-1].transform.position;
				GameObject startControlPoint = CreateControlPoint(newRidgeStruct.body, startControlPointPos);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(startControlPoint);
			}	
			break;		
		
		}
	
	}
}

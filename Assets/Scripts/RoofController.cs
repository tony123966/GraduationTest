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
	//Roof**************************************************************************
	public enum RoofType { Zan_Jian_Ding = 0, Wu_Dian_Ding = 1, Lu_Ding = 2, Juan_Peng = 3 };//Zan_Jian_Ding攢尖頂, Wu_Dian_Ding廡殿頂,Lu_Ding盝頂,Juan_Peng卷棚
	public RoofType roofType = RoofType.Zan_Jian_Ding;


	public float tileLength;//瓦片長度
	public float allJijaHeight;//總舉架高度
	public float eave2eaveColumnOffset;//出檐長度

	public Vector3 roofTopCenter;
	//**********************************************************************************
	//public enum Zan_Jian_Ding_MainRidge_CP { Rafter_Start, angle, EaveColumn_Purlin, Rafter_End };
	public List<RidgeStruct> mainRidgeList = new List<RidgeStruct>();//主脊
	public List<RidgeStruct> eaveList = new List<RidgeStruct>();//檐出
	//**********************************************************************************
	private List<RidgeStruct> roofSurfaceTileRidgeList = new List<RidgeStruct>();
	float roofSurfaceTileWidth=0.1f;

	public void InitFunction()
	{
		//初始值******************************************************************************
		allJijaHeight = BodyController.Instance.eaveColumnHeight * 0.8f;
		eave2eaveColumnOffset = BodyController.Instance.eaveColumnHeight * 0.3f;
		roofTopCenter = BodyController.Instance.bodyCenter + new Vector3(0, BodyController.Instance.eaveColumnHeight / 2.0f + allJijaHeight, 0);

		tileLength = BodyController.Instance.eaveColumnHeight * 0.1f;
		//************************************************************************************
		roof = new GameObject("Roof");
		roof.transform.position = roofTopCenter;
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
	public RidgeStruct CreateRidgeSturct(string name)
	{
		RidgeStruct newRidgeStruct=new RidgeStruct();
		newRidgeStruct.body = new GameObject(name);
		newRidgeStruct.body.transform.parent=roof.transform;
		newRidgeStruct.ridgeCatLine = newRidgeStruct.body.AddComponent<CatLine>();

		return newRidgeStruct;
	}
	public void CreateAllMainRidge()
	{
		switch (roofType) 
		{ 
			//攢尖頂
			case RoofType.Zan_Jian_Ding:

			//MainRidge
			mainRidgeList.Clear();
			for(int i=0;i<(int)MainController.Instance.sides;i++)
			{
				RidgeStruct newRidgeStruct=CreateRidgeSturct("MainRidge");
				mainRidgeList.Add(newRidgeStruct);
				//TopControlPoint
				Vector3 topControlPointPos=roofTopCenter;
				GameObject topControlPoint = CreateControlPoint(newRidgeStruct.body, topControlPointPos, "TopControlPoint");
				//Connect2eaveColumnControlPoint
				Vector3 connect2eaveColumnControlPointPos = BodyController.Instance.eaveColumnList[i].topPos;
				GameObject connect2eaveColumnControlPoint = CreateControlPoint(newRidgeStruct.body, connect2eaveColumnControlPointPos, "Connect2eaveColumnControlPoint");
				//DownControlPoint
				Vector3 eave2eaveColumnOffsetVector = Vector3.Normalize(new Vector3(BodyController.Instance.eaveColumnList[i].topPos.x - roofTopCenter.x, 0, BodyController.Instance.eaveColumnList[i].topPos.z - roofTopCenter.z)) * eave2eaveColumnOffset;
				Vector3 downControlPointPos = BodyController.Instance.eaveColumnList[i].topPos + eave2eaveColumnOffsetVector;
				GameObject downControlPoint = CreateControlPoint(newRidgeStruct.body, downControlPointPos, "DownControlPoint");
				//MidControlPoint
				Vector3 midControlPointPos = (topControlPointPos + connect2eaveColumnControlPointPos)/2.0f;
				GameObject midControlPoint = CreateControlPoint(newRidgeStruct.body, midControlPointPos, "MidControlPoint");

				newRidgeStruct.ridgeCatLine.controlPointList.Add(topControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(midControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(connect2eaveColumnControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(downControlPoint);


				newRidgeStruct.ridgeCatLine.SetCatmullRom(tileLength);

				GameObject zzz = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				zzz.transform.position = downControlPointPos;
				zzz.GetComponent<MeshRenderer>().material.color = Color.blue;
				zzz.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

			}

			//Eave
			eaveList.Clear();
			for(int i=0;i<(int)MainController.Instance.sides;i++)
			{
				RidgeStruct newRidgeStruct = CreateRidgeSturct("Eave");
				eaveList.Add(newRidgeStruct);
				//StartControlPoint
				Vector3 startControlPointPos = mainRidgeList[i].ridgeCatLine.controlPointList[mainRidgeList[i].ridgeCatLine.controlPointList.Count-1].transform.position;
				GameObject startControlPoint = CreateControlPoint(newRidgeStruct.body, startControlPointPos, "StartControlPoint");
				//EndControlPoint
				Vector3 endControlPointPos = mainRidgeList[(i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.controlPointList[mainRidgeList[(i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.controlPointList.Count - 1].transform.position;
				GameObject endControlPoint = CreateControlPoint(newRidgeStruct.body, endControlPointPos,"EndControlPoint");
				//MidControlPoint
				Vector3 midControlPointPos = (startControlPointPos + endControlPointPos)/2.0f;
				GameObject midControlPoint = CreateControlPoint(newRidgeStruct.body, midControlPointPos,"MidControlPoint");

				newRidgeStruct.ridgeCatLine.controlPointList.Add(startControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(midControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(endControlPoint);

				newRidgeStruct.ridgeCatLine.SetCatmullRom(tileLength);
			}

			//MidPlaneCutRoofSurface
			roofSurfaceTileRidgeList.Clear();
			for (int i = 0; i < (int)MainController.Instance.sides; i++)
			{
				//FindMidRoofSurfaceMidPoint
				Vector3 v1 = mainRidgeList[i].ridgeCatLine.controlPointList[mainRidgeList[i].ridgeCatLine.controlPointList.Count - 1].transform.position - mainRidgeList[i].ridgeCatLine.controlPointList[0].transform.position;
				Vector3 v2 = mainRidgeList[(i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.controlPointList[mainRidgeList[(i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.controlPointList.Count - 1].transform.position - mainRidgeList[(i - 1+ (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.controlPointList[0].transform.position;
				float angle = Vector3.Angle(v1, v2);
				Vector3 midRoofSurfacePos = Quaternion.Euler(0, angle / 2.0f, 0) * mainRidgeList[i].ridgeCatLine.controlPointList[1].transform.position;
				Plane plane=new Plane();

				Vector3 midRoofSurfaceTopPoint=mainRidgeList[i].ridgeCatLine.controlPointList[0].transform.position;
				Vector3 midRoofSurfaceMidPoint=midRoofSurfacePos;
				Vector3 midRoofSurfaceDownPoint=eaveList[i].ridgeCatLine.controlPointList[1].transform.position;
				if(i==0){
				GameObject zzz = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				zzz.transform.position = midRoofSurfaceTopPoint;
				zzz.GetComponent<MeshRenderer>().material.color=Color.yellow;
				zzz.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				GameObject aaa = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				aaa.transform.position = midRoofSurfaceMidPoint;
				aaa.GetComponent<MeshRenderer>().material.color = Color.yellow;
				aaa.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				GameObject bbb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				bbb.transform.position = midRoofSurfaceDownPoint;
				bbb.GetComponent<MeshRenderer>().material.color = Color.yellow;
				bbb.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				}
				plane.Set3Points(midRoofSurfaceDownPoint, midRoofSurfaceMidPoint, midRoofSurfaceTopPoint);
				//plane.normal = Vector3.Cross((midRoofSurfaceMidPoint - midRoofSurfaceTopPoint), (midRoofSurfaceDownPoint - midRoofSurfaceTopPoint)).normalized;
				plane.normal = (eaveList[i].ridgeCatLine.controlPointList[0].transform.position - eaveList[i].ridgeCatLine.controlPointList[2].transform.position).normalized;
				//CalculateRoofSurfaceTileRidgeCount
				float maxDis2Plane=plane.GetDistanceToPoint(mainRidgeList[i].ridgeCatLine.controlPointList[mainRidgeList[i].ridgeCatLine.controlPointList.Count-1].transform.position);
/*
				float maxDis2Plane=float.MinValue;
				for(int j=0;j<mainRidgeList[i].ridgeCatLine.anchorInnerPointlist.Count;j++)
				{
					float dis=plane.GetDistanceToPoint(mainRidgeList[i].ridgeCatLine.anchorInnerPointlist[j]);
					if (dis >= maxDis2Plane) maxDis2Plane = dis;
				
				}*/
				int roofSurfaceTileRidgeCount = (int)Mathf.Abs(maxDis2Plane / roofSurfaceTileWidth);
				Debug.Log("roofSurfaceTileRidgeCount" + roofSurfaceTileRidgeCount);
				for(int n=0;n<roofSurfaceTileRidgeCount;n++)
				{
					RidgeStruct newRidgeStruct = CreateRidgeSturct("RoofSurfaceTileRidge");
					roofSurfaceTileRidgeList.Add(newRidgeStruct);
					plane.SetNormalAndPosition(plane.normal, eaveList[i].ridgeCatLine.controlPointList[2].transform.position + (roofSurfaceTileWidth * n) * plane.normal);

					GameObject ttttt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					ttttt.transform.parent = newRidgeStruct.body.transform;
					ttttt.transform.position = eaveList[i].ridgeCatLine.controlPointList[2].transform.position + (roofSurfaceTileWidth * n) * plane.normal;
					ttttt.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
					ttttt.GetComponent<MeshRenderer>().material.color = Color.green;


					Vector3 roofSurfaceTileRidgeUpPointPos = Vector3.zero;
					Vector3 roofSurfaceTileRidgeDownPointPos = Vector3.zero;
					//FindPointOnMainRidgeCloser2Plane
					float pointMinDis2Plane = float.MaxValue;
					for(int k=mainRidgeList[i].ridgeCatLine.innerPointList.Count-1;k>=0;k--)
					{
						float dis = Mathf.Abs(plane.GetDistanceToPoint(mainRidgeList[i].ridgeCatLine.innerPointList[k]));
						if (dis < pointMinDis2Plane)
						{
							pointMinDis2Plane = dis;
							roofSurfaceTileRidgeUpPointPos = mainRidgeList[i].ridgeCatLine.innerPointList[k];
						}
						else  break;
					
					}
					//FindPointOnEaveCloser2Plane
					pointMinDis2Plane = float.MaxValue;
					for (int h = 0; h < eaveList[i].ridgeCatLine.innerPointList.Count; h++)
					{
						float dis = Mathf.Abs(plane.GetDistanceToPoint(eaveList[i].ridgeCatLine.innerPointList[h]));
						if (dis < pointMinDis2Plane)
						{
							pointMinDis2Plane = dis;
							roofSurfaceTileRidgeDownPointPos = eaveList[i].ridgeCatLine.innerPointList[h];
						}
						else break;
					
					}
					//FindMidPointOnRoofSurfaceTileRidge





					GameObject roofSurfaceTileRidgeUpPoint =  GameObject.CreatePrimitive(PrimitiveType.Sphere);
					roofSurfaceTileRidgeUpPoint.transform.parent = newRidgeStruct.body.transform;
					roofSurfaceTileRidgeUpPoint.transform.position = roofSurfaceTileRidgeUpPointPos;
					roofSurfaceTileRidgeUpPoint.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
					GameObject roofSurfaceTileRidgeDownPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					roofSurfaceTileRidgeDownPoint.transform.parent = newRidgeStruct.body.transform;
					roofSurfaceTileRidgeDownPoint.transform.position = roofSurfaceTileRidgeDownPointPos;
					roofSurfaceTileRidgeDownPoint.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

				
				}
			}

			break;

		}
	
	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public struct RidgeStruct
{
	public GameObject body;
	public Dictionary<string, GameObject> controlPointDictionaryList;
	public CatLine ridgeCatLine;
}
public struct RoofSurfaceStruct
{
	public GameObject body;
	public List<RidgeStruct> rightRoofSurfaceTileRidgeList;
	public List<RidgeStruct> leftRoofSurfaceTileRidgeList;
	public RidgeStruct midRoofSurfaceTileRidge;
}
public class RoofController : Singleton<RoofController>
{

	public GameObject roof = null;
	//Roof**************************************************************************
	public enum RoofType { Zan_Jian_Ding = 0, Wu_Dian_Ding = 1, Lu_Ding = 2, Juan_Peng = 3 };//Zan_Jian_Ding攢尖頂, Wu_Dian_Ding廡殿頂,Lu_Ding盝頂,Juan_Peng卷棚
	public RoofType roofType = RoofType.Zan_Jian_Ding;


	public float roofSurfaceTileWidth = 0.1f;//瓦片長度
	public float allJijaHeight;//總舉架高度
	public float eave2eaveColumnOffset;//出檐長度

	public Vector3 roofTopCenter;
	//**********************************************************************************
	//public enum Zan_Jian_Ding_MainRidge_CP { Rafter_Start, angle, EaveColumn_Purlin, Rafter_End };
	public List<RidgeStruct> mainRidgeList = new List<RidgeStruct>();//主脊
	public List<RidgeStruct> eaveList = new List<RidgeStruct>();//檐出
	private enum EaveControlPointType { StartControlPoint , MidRControlPoint , MidControlPoint , MidLControlPoint   , EndControlPoint };
	private enum MainRidgeControlPointType { TopControlPoint , MidControlPoint , Connect2eaveColumnControlPoint , DownControlPoint  };
	private enum MidRoofSurfaceControlPointType { MidRoofSurfaceTopPoint , MidRoofSurfaceMidPoint , MidRoofSurfaceDownPoint  };

	//**********************************************************************************

	public List<RoofSurfaceStruct> roofSurfaceStructList = new List<RoofSurfaceStruct>();//屋面

	public void InitFunction()
	{
		//初始值******************************************************************************
		allJijaHeight = BodyController.Instance.eaveColumnHeight * 0.9f;
		eave2eaveColumnOffset = BodyController.Instance.eaveColumnHeight * 0.4f;
		roofTopCenter = BodyController.Instance.bodyCenter + new Vector3(0, BodyController.Instance.eaveColumnHeight / 2.0f + allJijaHeight, 0);

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
	public RidgeStruct CreateRidgeSturct(string name, GameObject parent)
	{
		RidgeStruct newRidgeStruct = new RidgeStruct();
		newRidgeStruct.controlPointDictionaryList = new Dictionary<string, GameObject>();
		newRidgeStruct.body = new GameObject(name);
		newRidgeStruct.body.transform.parent = parent.transform;
		newRidgeStruct.ridgeCatLine = newRidgeStruct.body.AddComponent<CatLine>();

		return newRidgeStruct;
	}
	public void ShowPos(Vector3 pos, GameObject parent,Color color, float localScale = 0.2f) 
	{ 
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		obj.transform.position = pos;
		obj.transform.parent = parent.transform;
		obj.transform.localScale = Vector3.one * localScale;
		obj.GetComponent<MeshRenderer>().material.color = color;
	}
	public float DisPoint2Line(Vector3 rayOrigin, Vector3 rayDir, Vector3 point)
	{
/*
		rayDir = rayDir.normalized;
		float distance = Vector3.Distance(rayOrigin, point);
		float angle = Vector3.Angle(rayDir + rayOrigin, point - rayOrigin);
		return (distance * Mathf.Sin(angle * Mathf.Deg2Rad));*/
		
				rayDir = rayDir.normalized;
				Ray ray = new Ray(rayOrigin, rayDir);
				return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
	}
	public void CreateAllMainRidge()
	{
		float anchorDis=0.0f;
		switch (roofType)
		{
			//攢尖頂
			case RoofType.Zan_Jian_Ding:
				#region  Zan_Jian_Ding

				float flyEaveHeightOffset =0.5f;
				float mainRidgeHeightOffset =-0.5f;
				float roofSurfaceHeightOffset = -0.1f;
				float eaveCurveHeightOffset = -flyEaveHeightOffset * 1.5f;

				//MainRidge
				mainRidgeList.Clear();
				for (int i = 0; i < (int)MainController.Instance.sides; i++)
				{
					RidgeStruct newRidgeStruct = CreateRidgeSturct("MainRidge", roof);
					mainRidgeList.Add(newRidgeStruct);
					//TopControlPoint
					Vector3 topControlPointPos = roofTopCenter;
					GameObject topControlPoint = CreateControlPoint(newRidgeStruct.body, topControlPointPos, MainRidgeControlPointType.TopControlPoint.ToString());
					newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.TopControlPoint.ToString(), topControlPoint);
					//Connect2eaveColumnControlPoint
					Vector3 connect2eaveColumnControlPointPos = BodyController.Instance.eaveColumnList[i].topPos;
					GameObject connect2eaveColumnControlPoint = CreateControlPoint(newRidgeStruct.body, connect2eaveColumnControlPointPos, MainRidgeControlPointType.Connect2eaveColumnControlPoint.ToString());
					newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.Connect2eaveColumnControlPoint.ToString(), connect2eaveColumnControlPoint);
					//DownControlPoint
					Vector3 eave2eaveColumnOffsetVector = Vector3.Normalize(new Vector3(BodyController.Instance.eaveColumnList[i].topPos.x - roofTopCenter.x, 0, BodyController.Instance.eaveColumnList[i].topPos.z - roofTopCenter.z)) * eave2eaveColumnOffset + flyEaveHeightOffset*Vector3.up;
					Vector3 downControlPointPos = BodyController.Instance.eaveColumnList[i].topPos + eave2eaveColumnOffsetVector;
					GameObject downControlPoint = CreateControlPoint(newRidgeStruct.body, downControlPointPos, MainRidgeControlPointType.DownControlPoint.ToString());
					newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.DownControlPoint.ToString(), downControlPoint);
					//MidControlPoint
					Vector3 midControlPointPos = (topControlPointPos + connect2eaveColumnControlPointPos) / 2.0f + mainRidgeHeightOffset*Vector3.up;
					GameObject midControlPoint = CreateControlPoint(newRidgeStruct.body, midControlPointPos, MainRidgeControlPointType.MidControlPoint.ToString());
					newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.MidControlPoint.ToString(), midControlPoint);

					newRidgeStruct.ridgeCatLine.controlPointList.Add(topControlPoint);
					newRidgeStruct.ridgeCatLine.controlPointList.Add(midControlPoint);
					newRidgeStruct.ridgeCatLine.controlPointList.Add(connect2eaveColumnControlPoint);
					newRidgeStruct.ridgeCatLine.controlPointList.Add(downControlPoint);

					GameObject aa = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					aa.transform.parent = newRidgeStruct.body.transform;
					aa.transform.position = topControlPointPos;
					aa.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
					aa.GetComponent<MeshRenderer>().material.color = Color.red;
					GameObject bb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					bb.transform.parent = newRidgeStruct.body.transform;
					bb.transform.position = midControlPointPos;
					bb.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
					bb.GetComponent<MeshRenderer>().material.color = Color.red;
					GameObject cc = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					cc.transform.parent = newRidgeStruct.body.transform;
					cc.transform.position = connect2eaveColumnControlPointPos;
					cc.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
					cc.GetComponent<MeshRenderer>().material.color = Color.red;
					GameObject dd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					dd.transform.parent = newRidgeStruct.body.transform;
					dd.transform.position = downControlPointPos;
					dd.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
					dd.GetComponent<MeshRenderer>().material.color = Color.red;

					newRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis);
				}

				//Eave
				eaveList.Clear();
				for (int i = 0; i < (int)MainController.Instance.sides; i++)
				{
					int nextIndex = (i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides;
					RidgeStruct newRidgeStruct = CreateRidgeSturct("Eave", roof);
					eaveList.Add(newRidgeStruct);
					//StartControlPoint
					Vector3 startControlPointPos = mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position;
					GameObject startControlPoint = CreateControlPoint(newRidgeStruct.body, startControlPointPos, EaveControlPointType.StartControlPoint.ToString());
					newRidgeStruct.controlPointDictionaryList.Add(EaveControlPointType.StartControlPoint.ToString(), startControlPoint);
					//EndControlPoint
					Vector3 endControlPointPos = mainRidgeList[nextIndex].controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position;
					GameObject endControlPoint = CreateControlPoint(newRidgeStruct.body, endControlPointPos, EaveControlPointType.EndControlPoint.ToString());
					newRidgeStruct.controlPointDictionaryList.Add(EaveControlPointType.EndControlPoint.ToString(), endControlPoint);
					//MidControlPoint
					Vector3 midControlPointPos = (startControlPointPos + endControlPointPos) / 2.0f + eaveCurveHeightOffset*Vector3.up;
					GameObject midControlPoint = CreateControlPoint(newRidgeStruct.body, midControlPointPos, EaveControlPointType.MidControlPoint.ToString());
					newRidgeStruct.controlPointDictionaryList.Add(EaveControlPointType.MidControlPoint.ToString(), midControlPoint);
					//MidRightControlPoint
					Vector3 midRControlPointPos = startControlPointPos - (startControlPointPos - endControlPointPos).normalized * eave2eaveColumnOffset + eaveCurveHeightOffset * Vector3.up;
					GameObject midRControlPoint = CreateControlPoint(newRidgeStruct.body, midRControlPointPos, EaveControlPointType.MidRControlPoint.ToString());
					newRidgeStruct.controlPointDictionaryList.Add(EaveControlPointType.MidRControlPoint.ToString(), midRControlPoint);
					//MidLeftControlPoint
					Vector3 midLControlPointPos = endControlPointPos + (startControlPointPos - endControlPointPos).normalized * eave2eaveColumnOffset + eaveCurveHeightOffset * Vector3.up;
					GameObject midLControlPoint = CreateControlPoint(newRidgeStruct.body, midLControlPointPos, EaveControlPointType.MidLControlPoint.ToString());
					newRidgeStruct.controlPointDictionaryList.Add(EaveControlPointType.MidLControlPoint.ToString(), midLControlPoint);

					newRidgeStruct.ridgeCatLine.controlPointList.Add(startControlPoint);
					newRidgeStruct.ridgeCatLine.controlPointList.Add(midRControlPoint);
					newRidgeStruct.ridgeCatLine.controlPointList.Add(midControlPoint);
					newRidgeStruct.ridgeCatLine.controlPointList.Add(midLControlPoint);
					newRidgeStruct.ridgeCatLine.controlPointList.Add(endControlPoint);

					newRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis);
					//修正
					int midRControlPointIndex = 0;
					float pointMinDis2Plane= float.MaxValue;
					while (midRControlPointIndex < newRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count)
					{
						float dis = Vector3.Magnitude(newRidgeStruct.ridgeCatLine.anchorInnerPointlist[midRControlPointIndex] - midRControlPointPos);
							if (dis < pointMinDis2Plane)
							{
								pointMinDis2Plane = dis;
								midRControlPointIndex++;
							}
							else { break; }
							
					}
					for (int n = midRControlPointIndex; n < newRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - midRControlPointIndex; n++)
					{
						float reviseHeight = midControlPointPos.y;
						newRidgeStruct.ridgeCatLine.anchorInnerPointlist[n] = new Vector3(newRidgeStruct.ridgeCatLine.anchorInnerPointlist[n].x, reviseHeight , newRidgeStruct.ridgeCatLine.anchorInnerPointlist[n].z);
					}

				}

				//RoofSurface
				//for (int i = 0; i < (int)MainController.Instance.sides; i++)
				for (int i = 0; i < 1; i++)
				{

					float pointMinDis2Plane = float.MaxValue;
					int nextIndex = (i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides;
					RoofSurfaceStruct newRoofSurfaceStruct = new RoofSurfaceStruct();
					newRoofSurfaceStruct.body = new GameObject("RoofSurface");
					newRoofSurfaceStruct.body.transform.parent = roof.transform;
					newRoofSurfaceStruct.rightRoofSurfaceTileRidgeList = new List<RidgeStruct>();
					newRoofSurfaceStruct.leftRoofSurfaceTileRidgeList = new List<RidgeStruct>();

					RidgeStruct newMidRidgeStruct = CreateRidgeSturct("MidRoofSurfaceTileRidge", newRoofSurfaceStruct.body);
					newRoofSurfaceStruct.midRoofSurfaceTileRidge = newMidRidgeStruct;

					roofSurfaceStructList.Add(newRoofSurfaceStruct);
					//FindMidRoofSurfaceMidPoint
					Vector2 v1 = new Vector2(mainRidgeList[nextIndex].controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position.x - mainRidgeList[nextIndex].controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position.x, mainRidgeList[nextIndex].controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position.z - mainRidgeList[nextIndex].controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position.z);
					Vector2 v2 = new Vector2(mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position.x - mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position.x, mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position.z - mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position.z);

					float angle = Vector2.Angle(v1, v2);

					//midRoofSurfaceTopPoint
					Vector3 midRoofSurfaceTopPointPos = mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position;
					GameObject midRoofSurfaceTopPoint = CreateControlPoint(newMidRidgeStruct.body, midRoofSurfaceTopPointPos, MidRoofSurfaceControlPointType.MidRoofSurfaceTopPoint.ToString());
					newMidRidgeStruct.controlPointDictionaryList.Add(MidRoofSurfaceControlPointType.MidRoofSurfaceTopPoint.ToString(), midRoofSurfaceTopPoint);		
					//midRoofSurfaceDownPoint
					Vector3 midRoofSurfaceDownPointPos = eaveList[i].controlPointDictionaryList[EaveControlPointType.MidControlPoint.ToString()].transform.position;
					GameObject midRoofSurfaceDownPoint = CreateControlPoint(newMidRidgeStruct.body, midRoofSurfaceDownPointPos, MidRoofSurfaceControlPointType.MidRoofSurfaceDownPoint.ToString());
					newMidRidgeStruct.controlPointDictionaryList.Add(MidRoofSurfaceControlPointType.MidRoofSurfaceDownPoint.ToString(), midRoofSurfaceDownPoint);
					//midRoofSurfaceMidPoint
					Vector3 midRoofSurfaceMidPointPos = (Quaternion.Euler(0, angle / 2.0f, 0) * (mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position)) + roofSurfaceHeightOffset * Vector3.up;
					GameObject midRoofSurfaceMidPoint = CreateControlPoint(newMidRidgeStruct.body, midRoofSurfaceMidPointPos, MidRoofSurfaceControlPointType.MidRoofSurfaceMidPoint.ToString());
					newMidRidgeStruct.controlPointDictionaryList.Add(MidRoofSurfaceControlPointType.MidRoofSurfaceMidPoint.ToString(), midRoofSurfaceMidPoint);



					//MidRoofSurfaceTileRidge
					newMidRidgeStruct.ridgeCatLine.controlPointList.Add(midRoofSurfaceTopPoint);
					newMidRidgeStruct.ridgeCatLine.controlPointList.Add(midRoofSurfaceMidPoint);
					newMidRidgeStruct.ridgeCatLine.controlPointList.Add(midRoofSurfaceDownPoint);
					newMidRidgeStruct.ridgeCatLine.SetLineNumberOfPoints(1000);
					newMidRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis);

					GameObject tttttt = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					tttttt.transform.position = midRoofSurfaceMidPointPos;
					tttttt.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
					tttttt.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 1);


					for (int nn = 0; nn < newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count; nn++)
					{
						GameObject hh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						hh.transform.position = newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist[nn];
						hh.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
						hh.GetComponent<MeshRenderer>().material.color = Color.blue;
					}

					//CutPlane
					Plane plane = new Plane();
					plane.Set3Points(midRoofSurfaceDownPointPos, midRoofSurfaceMidPointPos, midRoofSurfaceTopPointPos);
					//plane.normal = Vector3.Cross((midRoofSurfaceDownPoint - midRoofSurfaceTopPoint), (midRoofSurfaceMidPoint - midRoofSurfaceTopPoint)).normalized;
					plane.normal = (eaveList[i].controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position - eaveList[i].controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position).normalized;
					plane.SetNormalAndPosition(plane.normal, midRoofSurfaceTopPointPos);

					float roofSurfaceTileRidge2PlaneMaxDis = plane.GetDistanceToPoint(mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position + roofSurfaceTileWidth * plane.normal);
					float eaveColumn2PlaneDis = plane.GetDistanceToPoint(mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.Connect2eaveColumnControlPoint.ToString()].transform.position);
					float eaveColumn2MaxRoofSurfaceTileRidgeDis = roofSurfaceTileRidge2PlaneMaxDis - eaveColumn2PlaneDis;

					int roofSurfaceTileRidgeMaxCount = Mathf.FloorToInt(Mathf.Abs(roofSurfaceTileRidge2PlaneMaxDis / roofSurfaceTileWidth));

					int eaveColumn2MaxRoofSurfaceTileRidgeCount = Mathf.FloorToInt(Mathf.Abs(eaveColumn2MaxRoofSurfaceTileRidgeDis / roofSurfaceTileWidth));

					//紀錄前一次Index用於迴圈加速
					int roofSurfaceTileRidgeStartingIndex = 0;
					int roofSurface2MainRidgeStartingIndex_R = 0;
					int roofSurface2EaveStartingIndex_R = eaveList[i].ridgeCatLine.anchorInnerPointlist.Count/2;


					//檐柱後旋轉角


					//Right&LeftRoofSurfaceTileRidgeList
					for (int n = 1; n < roofSurfaceTileRidgeMaxCount; n++)
					{
						//Right*******************************
						RidgeStruct newRightRidgeStruct = CreateRidgeSturct("RightRoofSurfaceTileRidge", newRoofSurfaceStruct.body);
						newRoofSurfaceStruct.rightRoofSurfaceTileRidgeList.Add(newRightRidgeStruct);
						RidgeStruct newLeftRidgeStruct = CreateRidgeSturct("LeftRoofSurfaceTileRidge", newRoofSurfaceStruct.body);
						newRoofSurfaceStruct.leftRoofSurfaceTileRidgeList.Add(newLeftRidgeStruct);
						float planeOffset=roofSurfaceTileWidth * n;
						Vector3 planeOffsetVector = planeOffset * plane.normal;

						if (planeOffset > eaveColumn2PlaneDis)
						{
						//???
						/*	angle=000000;
							plane.normal =(Quaternion.Euler(0, angle, 0))*plane.normal;
							plane.SetNormalAndPosition(plane.normal, midRoofSurfaceTopPointPos);*/
							plane.SetNormalAndPosition(plane.normal, midRoofSurfaceTopPointPos + planeOffsetVector);
						}
						else plane.SetNormalAndPosition(plane.normal, midRoofSurfaceTopPointPos + planeOffsetVector);
						Vector3 roofSurfaceTileRidgeUpPointPos = Vector3.zero;
						Vector3 roofSurfaceTileRidgeDownPointPos = Vector3.zero;
						Vector3 roofSurfaceTileRidgeMidPointPos = Vector3.zero;
	
						//FindPointOnMainRidgeCloser2Plane
						pointMinDis2Plane = float.MaxValue;
						while( roofSurface2MainRidgeStartingIndex_R < mainRidgeList[i].ridgeCatLine.anchorInnerPointlist.Count)
						{
							float dis = Mathf.Abs(plane.GetDistanceToPoint(mainRidgeList[i].ridgeCatLine.anchorInnerPointlist[roofSurface2MainRidgeStartingIndex_R]));
							if (dis < pointMinDis2Plane)
							{
								pointMinDis2Plane = dis;
								roofSurface2MainRidgeStartingIndex_R++;
								
								
							}
							else { roofSurfaceTileRidgeUpPointPos = mainRidgeList[i].ridgeCatLine.anchorInnerPointlist[roofSurface2MainRidgeStartingIndex_R]; break; }
							
						}
						//FindPointOnEaveCloser2Plane
						pointMinDis2Plane = float.MaxValue;
						while(roofSurface2EaveStartingIndex_R < eaveList[i].ridgeCatLine.anchorInnerPointlist.Count)
						{
							float dis = Mathf.Abs(plane.GetDistanceToPoint(eaveList[i].ridgeCatLine.anchorInnerPointlist[roofSurface2EaveStartingIndex_R]));
							if (dis < pointMinDis2Plane)
							{
								pointMinDis2Plane = dis;
								roofSurface2EaveStartingIndex_R--;
						
								
							}
							else { roofSurfaceTileRidgeDownPointPos = eaveList[i].ridgeCatLine.anchorInnerPointlist[roofSurface2EaveStartingIndex_R]; break; }
						
						}
						GameObject roofSurfaceTileRidgeUpPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						roofSurfaceTileRidgeUpPoint.transform.parent = newRightRidgeStruct.body.transform;
						roofSurfaceTileRidgeUpPoint.transform.position = roofSurfaceTileRidgeUpPointPos;
						roofSurfaceTileRidgeUpPoint.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
						roofSurfaceTileRidgeUpPoint.GetComponent<MeshRenderer>().material.color = Color.yellow;
						GameObject roofSurfaceTileRidgeDownPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						roofSurfaceTileRidgeDownPoint.transform.parent = newRightRidgeStruct.body.transform;
						roofSurfaceTileRidgeDownPoint.transform.position = roofSurfaceTileRidgeDownPointPos;
						roofSurfaceTileRidgeDownPoint.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
						roofSurfaceTileRidgeDownPoint.GetComponent<MeshRenderer>().material.color = Color.yellow;
						//FindMidPointOnRoofSurfaceTileRidge

						#region Testing
						pointMinDis2Plane = float.MaxValue;
						while (roofSurfaceTileRidgeStartingIndex < newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count)
						{
							float dis = DisPoint2Line(roofSurfaceTileRidgeUpPointPos, plane.normal, newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist[roofSurfaceTileRidgeStartingIndex]);
							if (dis < pointMinDis2Plane)
							{
								pointMinDis2Plane = dis;
								roofSurfaceTileRidgeStartingIndex++;
							}
							else { break; }
						
						}
						Debug.Log("RR_roofSurfaceTileRidgeUpPointPos.y + roofSurface2MainRidgeStartingIndex_R " + roofSurfaceTileRidgeUpPointPos.y + " " + roofSurface2MainRidgeStartingIndex_R);
						int aaaaa = roofSurface2MainRidgeStartingIndex_R;

						float ratio = (float)(n-1) / (roofSurfaceTileRidgeMaxCount);
						float roofSurfaceTileRidgeHeight =((newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist[(newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - roofSurfaceTileRidgeStartingIndex) / 2 + roofSurfaceTileRidgeStartingIndex]).y) * (1.0f - ratio) + ((roofSurfaceTileRidgeUpPointPos.y + roofSurfaceTileRidgeDownPointPos.y) / 2.0f) * ratio;

						/****************************/
						#endregion
						roofSurfaceTileRidgeMidPointPos = new Vector3((roofSurfaceTileRidgeUpPointPos.x + roofSurfaceTileRidgeDownPointPos.x) / 2.0f, roofSurfaceTileRidgeHeight, (roofSurfaceTileRidgeUpPointPos.z + roofSurfaceTileRidgeDownPointPos.z) / 2.0f);

						newRightRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeUpPointPos);
						newRightRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeMidPointPos);
						newRightRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeDownPointPos);
						newRightRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis, 1);

						GameObject dfdf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						dfdf.transform.position = roofSurfaceTileRidgeMidPointPos;
						dfdf.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
						dfdf.GetComponent<MeshRenderer>().material.color = Color.red;


						for (int nn = 0; nn < newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count; nn++)
						{
							GameObject hh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
							hh.transform.position = newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[nn];
							hh.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
							hh.GetComponent<MeshRenderer>().material.color = Color.green;
						}
						//Left******************************

						//FindPointOnMainRidgeCloser2Plane

						roofSurfaceTileRidgeUpPointPos = mainRidgeList[nextIndex].ridgeCatLine.anchorInnerPointlist[aaaaa];
						Debug.Log("LL_roofSurfaceTileRidgeUpPointPos.y + roofSurface2MainRidgeStartingIndex_R " + roofSurfaceTileRidgeUpPointPos.y + " " + aaaaa);
						//FindPointOnEaveCloser2Plane

						roofSurfaceTileRidgeDownPointPos = eaveList[i].ridgeCatLine.anchorInnerPointlist[eaveList[i].ridgeCatLine.anchorInnerPointlist.Count- roofSurface2EaveStartingIndex_R];
			
						roofSurfaceTileRidgeUpPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						roofSurfaceTileRidgeUpPoint.transform.parent = newLeftRidgeStruct.body.transform;
						roofSurfaceTileRidgeUpPoint.transform.position = roofSurfaceTileRidgeUpPointPos;
						roofSurfaceTileRidgeUpPoint.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
						roofSurfaceTileRidgeUpPoint.GetComponent<MeshRenderer>().material.color = Color.yellow;
						roofSurfaceTileRidgeDownPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						roofSurfaceTileRidgeDownPoint.transform.parent = newLeftRidgeStruct.body.transform;
						roofSurfaceTileRidgeDownPoint.transform.position = roofSurfaceTileRidgeDownPointPos;
						roofSurfaceTileRidgeDownPoint.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
						roofSurfaceTileRidgeDownPoint.GetComponent<MeshRenderer>().material.color = Color.yellow;

						roofSurfaceTileRidgeMidPointPos = new Vector3((roofSurfaceTileRidgeUpPointPos.x + roofSurfaceTileRidgeDownPointPos.x) / 2.0f, roofSurfaceTileRidgeHeight, (roofSurfaceTileRidgeUpPointPos.z + roofSurfaceTileRidgeDownPointPos.z) / 2.0f);

						newLeftRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeUpPointPos);
						newLeftRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeMidPointPos);
						newLeftRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeDownPointPos);
						newLeftRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis, 1);


						for (int nn = 0; nn < newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count; nn++)
						{
							GameObject hh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
							hh.transform.position = newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist[nn];
							hh.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
							hh.GetComponent<MeshRenderer>().material.color = Color.green;
						}
					}
				}
				#endregion
				break;
			case RoofType.Wu_Dian_Ding:
				#region  Wu_Dian_Ding

				#endregion
				break;
		}

	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public struct RidgeStruct
{
	public GameObject body;
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
	//**********************************************************************************

	public List<RoofSurfaceStruct> roofSurfaceStructList = new List<RoofSurfaceStruct>();//屋面

	public void InitFunction()
	{
		//初始值******************************************************************************
		allJijaHeight = BodyController.Instance.eaveColumnHeight * 0.8f;
		eave2eaveColumnOffset = BodyController.Instance.eaveColumnHeight * 0.3f;
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
	public RidgeStruct CreateRidgeSturct(string name,GameObject parent)
	{
		RidgeStruct newRidgeStruct=new RidgeStruct();
		newRidgeStruct.body = new GameObject(name);
		newRidgeStruct.body.transform.parent = parent.transform;
		newRidgeStruct.ridgeCatLine = newRidgeStruct.body.AddComponent<CatLine>();

		return newRidgeStruct;
	}
	public float DisPoint2Line(Vector3 rayOrigin, Vector3 rayDir, Vector3 point)
	{
		rayDir = rayDir.normalized;
		float distance = Vector3.Distance(rayOrigin, point);
		float angle = Vector3.Angle(rayDir + rayOrigin, point - rayOrigin);
		return (distance * Mathf.Sin(angle * Mathf.Deg2Rad));
/*
		rayDir = rayDir.normalized;
		Ray ray = new Ray(rayOrigin, rayDir);
		return Vector3.Cross(ray.direction, point - ray.origin).magnitude;*/
	}
	public void CreateAllMainRidge()
	{
		switch (roofType) 
		{ 
			//攢尖頂
			case RoofType.Zan_Jian_Ding:
			Vector3 flyEaveHeightOffset = new Vector3(0,1.0f,0);
			Vector3 mainRidgeHeightOffset = new Vector3(0,-0.5f,0);
			Vector3 eaveCurveOffset = new Vector3(0, -flyEaveHeightOffset.y * 1.2f, 0);
			//MainRidge
			mainRidgeList.Clear();
			for(int i=0;i<(int)MainController.Instance.sides;i++)
			{
				RidgeStruct newRidgeStruct=CreateRidgeSturct("MainRidge",roof);
				mainRidgeList.Add(newRidgeStruct);
				//TopControlPoint
				Vector3 topControlPointPos=roofTopCenter;
				GameObject topControlPoint = CreateControlPoint(newRidgeStruct.body, topControlPointPos, "TopControlPoint");
				//Connect2eaveColumnControlPoint
				Vector3 connect2eaveColumnControlPointPos = BodyController.Instance.eaveColumnList[i].topPos;
				GameObject connect2eaveColumnControlPoint = CreateControlPoint(newRidgeStruct.body, connect2eaveColumnControlPointPos, "Connect2eaveColumnControlPoint");
				//DownControlPoint
				Vector3 eave2eaveColumnOffsetVector = Vector3.Normalize(new Vector3(BodyController.Instance.eaveColumnList[i].topPos.x - roofTopCenter.x, 0, BodyController.Instance.eaveColumnList[i].topPos.z - roofTopCenter.z)) * eave2eaveColumnOffset+flyEaveHeightOffset;
				Vector3 downControlPointPos = BodyController.Instance.eaveColumnList[i].topPos + eave2eaveColumnOffsetVector;
				GameObject downControlPoint = CreateControlPoint(newRidgeStruct.body, downControlPointPos, "DownControlPoint");
				//MidControlPoint
				Vector3 midControlPointPos = (topControlPointPos + connect2eaveColumnControlPointPos) / 2.0f +mainRidgeHeightOffset;
				GameObject midControlPoint = CreateControlPoint(newRidgeStruct.body, midControlPointPos, "MidControlPoint");

				newRidgeStruct.ridgeCatLine.controlPointList.Add(topControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(midControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(connect2eaveColumnControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(downControlPoint);

				GameObject aa = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				aa.transform.parent = newRidgeStruct.body.transform;
				aa.transform.position = topControlPointPos;
				aa.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				aa.GetComponent<MeshRenderer>().material.color = Color.red;
				GameObject bb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				bb.transform.parent = newRidgeStruct.body.transform;
				bb.transform.position = midControlPointPos;
				bb.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				bb.GetComponent<MeshRenderer>().material.color = Color.red;
				GameObject cc = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				cc.transform.parent = newRidgeStruct.body.transform;
				cc.transform.position = connect2eaveColumnControlPointPos;
				cc.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				cc.GetComponent<MeshRenderer>().material.color = Color.red;
				GameObject dd = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				dd.transform.parent = newRidgeStruct.body.transform;
				dd.transform.position = downControlPointPos;
				dd.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				dd.GetComponent<MeshRenderer>().material.color = Color.red;

				newRidgeStruct.ridgeCatLine.SetCatmullRom(0.005f);
			}

			//Eave
			eaveList.Clear();
			for(int i=0;i<(int)MainController.Instance.sides;i++)
			{
				RidgeStruct newRidgeStruct = CreateRidgeSturct("Eave", roof);
				eaveList.Add(newRidgeStruct);
				//StartControlPoint
				Vector3 startControlPointPos = mainRidgeList[i].ridgeCatLine.controlPointList[mainRidgeList[i].ridgeCatLine.controlPointList.Count-1].transform.position;
				GameObject startControlPoint = CreateControlPoint(newRidgeStruct.body, startControlPointPos, "StartControlPoint");
				//EndControlPoint
				Vector3 endControlPointPos = mainRidgeList[(i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.controlPointList[mainRidgeList[(i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.controlPointList.Count - 1].transform.position;
				GameObject endControlPoint = CreateControlPoint(newRidgeStruct.body, endControlPointPos,"EndControlPoint");
				//MidControlPoint

				Vector3 midControlPointPos = (startControlPointPos + endControlPointPos) / 2.0f + eaveCurveOffset;
				GameObject midControlPoint = CreateControlPoint(newRidgeStruct.body, midControlPointPos,"MidControlPoint");

				newRidgeStruct.ridgeCatLine.controlPointList.Add(startControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(midControlPoint);
				newRidgeStruct.ridgeCatLine.controlPointList.Add(endControlPoint);

				newRidgeStruct.ridgeCatLine.SetCatmullRom(0.005f);

			}

			//RoofSurface

			for (int i = 0; i < (int)MainController.Instance.sides; i++)
			{
				RoofSurfaceStruct newRoofSurfaceStruct=new RoofSurfaceStruct();
				newRoofSurfaceStruct.body = new GameObject("RoofSurface");
				newRoofSurfaceStruct.body.transform.parent=roof.transform;
				newRoofSurfaceStruct.rightRoofSurfaceTileRidgeList = new List<RidgeStruct>();
				newRoofSurfaceStruct.leftRoofSurfaceTileRidgeList = new List<RidgeStruct>();

				RidgeStruct newMidRidgeStruct = CreateRidgeSturct("MidRoofSurfaceTileRidge", newRoofSurfaceStruct.body);
				newRoofSurfaceStruct.midRoofSurfaceTileRidge=newMidRidgeStruct;

				roofSurfaceStructList.Add(newRoofSurfaceStruct);
				//FindMidRoofSurfaceMidPoint
				Vector3 v1 = mainRidgeList[i].ridgeCatLine.controlPointList[mainRidgeList[i].ridgeCatLine.controlPointList.Count - 1].transform.position - mainRidgeList[i].ridgeCatLine.controlPointList[0].transform.position;
				Vector3 v2 = mainRidgeList[(i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.controlPointList[mainRidgeList[(i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.controlPointList.Count - 1].transform.position - mainRidgeList[(i - 1+ (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.controlPointList[0].transform.position;
				float angle = Vector3.Angle(v1, v2);
				Vector3 midRoofSurfacePos = Quaternion.Euler(0, angle / 2.0f, 0) * mainRidgeList[i].ridgeCatLine.controlPointList[1].transform.position;

				//midRoofSurfaceTopPoint
				Vector3 midRoofSurfaceTopPointPos =mainRidgeList[i].ridgeCatLine.controlPointList[0].transform.position;
				GameObject midRoofSurfaceTopPoint = CreateControlPoint(newMidRidgeStruct.body, midRoofSurfaceTopPointPos, "MidRoofSurfaceTopPoint");
				//midRoofSurfaceMidPoint
				Vector3 midRoofSurfaceMidPointPos=midRoofSurfacePos;
				GameObject midRoofSurfaceMidPoint = CreateControlPoint(newMidRidgeStruct.body, midRoofSurfaceMidPointPos, "MidRoofSurfaceMidPoint");
				//midRoofSurfaceDownPoint
				Vector3 midRoofSurfaceDownPointPos=eaveList[i].ridgeCatLine.controlPointList[1].transform.position;
				GameObject midRoofSurfaceDownPoint = CreateControlPoint(newMidRidgeStruct.body, midRoofSurfaceDownPointPos, "MidRoofSurfaceDownPos");
				Plane plane=new Plane();
				plane.Set3Points(midRoofSurfaceDownPointPos, midRoofSurfaceMidPointPos, midRoofSurfaceTopPointPos);
				//plane.normal = Vector3.Cross((midRoofSurfaceDownPoint - midRoofSurfaceTopPoint), (midRoofSurfaceMidPoint - midRoofSurfaceTopPoint)).normalized;
				plane.normal = (eaveList[i].ridgeCatLine.controlPointList[0].transform.position - eaveList[i].ridgeCatLine.controlPointList[2].transform.position).normalized;
				//MidRoofSurfaceTileRidge
				newMidRidgeStruct.ridgeCatLine.controlPointList.Add(midRoofSurfaceTopPoint);
				newMidRidgeStruct.ridgeCatLine.controlPointList.Add(midRoofSurfaceMidPoint);
				newMidRidgeStruct.ridgeCatLine.controlPointList.Add(midRoofSurfaceDownPoint);
				newMidRidgeStruct.ridgeCatLine.SetCatmullRom(0.005f);

				GameObject aa = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				aa.transform.parent = newMidRidgeStruct.body.transform;
				aa.transform.position = midRoofSurfaceTopPointPos;
				aa.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				aa.GetComponent<MeshRenderer>().material.color = Color.blue;
				GameObject bb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				bb.transform.parent = newMidRidgeStruct.body.transform;
				bb.transform.position = midRoofSurfaceMidPointPos;
				bb.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				bb.GetComponent<MeshRenderer>().material.color = Color.blue;
				GameObject cc = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				cc.transform.parent = newMidRidgeStruct.body.transform;
				cc.transform.position = midRoofSurfaceDownPointPos;
				cc.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
				cc.GetComponent<MeshRenderer>().material.color = Color.blue;
		
				float maxDis2Plane = plane.GetDistanceToPoint(mainRidgeList[i].ridgeCatLine.controlPointList[mainRidgeList[i].ridgeCatLine.controlPointList.Count - 1].transform.position);

				int roofSurfaceTileRidgeCount = Mathf.FloorToInt(Mathf.Abs(maxDis2Plane / roofSurfaceTileWidth));
				#region Testing
				int roofSurfaceTileRidgeStartingIndex = 0;
				#endregion
				//Right&LeftRoofSurfaceTileRidgeList
				for(int n=1;n<roofSurfaceTileRidgeCount;n++)
				{
				//Right*******************************
					RidgeStruct newRightRidgeStruct = CreateRidgeSturct("RightRoofSurfaceTileRidge", newRoofSurfaceStruct.body);
					newRoofSurfaceStruct.rightRoofSurfaceTileRidgeList.Add(newRightRidgeStruct);
					RidgeStruct newLeftRidgeStruct = CreateRidgeSturct("LeftRoofSurfaceTileRidge", newRoofSurfaceStruct.body);
					newRoofSurfaceStruct.leftRoofSurfaceTileRidgeList.Add(newLeftRidgeStruct);

					Vector3 planeOffset= (roofSurfaceTileWidth * n) * plane.normal;
					plane.SetNormalAndPosition(plane.normal, midRoofSurfaceDownPointPos + planeOffset);

					Vector3 roofSurfaceTileRidgeUpPointPos = Vector3.zero;
					Vector3 roofSurfaceTileRidgeDownPointPos = Vector3.zero;
					Vector3 roofSurfaceTileRidgeMidPointPos = Vector3.zero;
					//FindPointOnMainRidgeCloser2Plane
					float pointMinDis2Plane = float.MaxValue;
					for (int k = 0; k < mainRidgeList[i].ridgeCatLine.anchorInnerPointlist.Count; k++)
					{
						float dis = Mathf.Abs(plane.GetDistanceToPoint(mainRidgeList[i].ridgeCatLine.anchorInnerPointlist[k]));
						if (dis < pointMinDis2Plane)
						{
							pointMinDis2Plane = dis;
							roofSurfaceTileRidgeUpPointPos = mainRidgeList[i].ridgeCatLine.anchorInnerPointlist[k];
						}
						else  break;
					
					}
					//FindPointOnEaveCloser2Plane
					pointMinDis2Plane = float.MaxValue;
					for (int h = 0; h < eaveList[i].ridgeCatLine.anchorInnerPointlist.Count; h++)
					{
						float dis = Mathf.Abs(plane.GetDistanceToPoint(eaveList[i].ridgeCatLine.anchorInnerPointlist[h]));
						if (dis < pointMinDis2Plane)
						{
							pointMinDis2Plane = dis;
							roofSurfaceTileRidgeDownPointPos = eaveList[i].ridgeCatLine.anchorInnerPointlist[h];
						}
						else break;
					
					}
					GameObject roofSurfaceTileRidgeUpPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					roofSurfaceTileRidgeUpPoint.transform.parent = newRightRidgeStruct.body.transform;
					roofSurfaceTileRidgeUpPoint.transform.position = roofSurfaceTileRidgeUpPointPos;
					roofSurfaceTileRidgeUpPoint.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
					roofSurfaceTileRidgeUpPoint.GetComponent<MeshRenderer>().material.color = Color.yellow;
					GameObject roofSurfaceTileRidgeDownPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					roofSurfaceTileRidgeDownPoint.transform.parent = newRightRidgeStruct.body.transform;
					roofSurfaceTileRidgeDownPoint.transform.position = roofSurfaceTileRidgeDownPointPos;
					roofSurfaceTileRidgeDownPoint.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
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
					roofSurfaceTileRidgeMidPointPos = new Vector3((roofSurfaceTileRidgeUpPointPos.x + roofSurfaceTileRidgeDownPointPos.x) / 2.0f, (newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist[(newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - roofSurfaceTileRidgeStartingIndex) / 2 + roofSurfaceTileRidgeStartingIndex] + planeOffset).y, (roofSurfaceTileRidgeUpPointPos.z + roofSurfaceTileRidgeDownPointPos.z) / 2.0f);
					#endregion
					roofSurfaceTileRidgeMidPointPos = new Vector3((roofSurfaceTileRidgeUpPointPos.x + roofSurfaceTileRidgeDownPointPos.x) / 2.0f, (mainRidgeList[i].ridgeCatLine.controlPointList[mainRidgeList[i].ridgeCatLine.controlPointList.Count - 1].transform.position + mainRidgeList[i].ridgeCatLine.controlPointList[mainRidgeList[i].ridgeCatLine.controlPointList.Count - 2].transform.position).y / 2.0f, (roofSurfaceTileRidgeUpPointPos.z + roofSurfaceTileRidgeDownPointPos.z) / 2.0f) + (mainRidgeHeightOffset) * ((float)n / roofSurfaceTileRidgeCount);

					newRightRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeUpPointPos);
					newRightRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeMidPointPos);
					newRightRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeDownPointPos);
					newRightRidgeStruct.ridgeCatLine.SetCatmullRom(0.005f, 1);

					GameObject dfdf = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					dfdf.transform.position = roofSurfaceTileRidgeMidPointPos;
					dfdf.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
					dfdf.GetComponent<MeshRenderer>().material.color = Color.green;
					

										for(int nn=0;nn<newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count;nn++)
										{
											GameObject hh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
											hh.transform.position = newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[nn];
											hh.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
											hh.GetComponent<MeshRenderer>().material.color = Color.green;
										}
					//Left******************************
					planeOffset = (roofSurfaceTileWidth * n) * plane.normal;
					plane.SetNormalAndPosition(plane.normal, midRoofSurfaceDownPointPos - planeOffset);

					 roofSurfaceTileRidgeUpPointPos = Vector3.zero;
					 roofSurfaceTileRidgeDownPointPos = Vector3.zero;
					 roofSurfaceTileRidgeMidPointPos = Vector3.zero;
					//FindPointOnMainRidgeCloser2Plane
					 pointMinDis2Plane = float.MaxValue;
					 for (int k = 0; k < mainRidgeList[(i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.anchorInnerPointlist.Count; k++)
					{
						float dis = Mathf.Abs(plane.GetDistanceToPoint(mainRidgeList[(i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.anchorInnerPointlist[k]));
						if (dis < pointMinDis2Plane)
						{
							pointMinDis2Plane = dis;
							roofSurfaceTileRidgeUpPointPos = mainRidgeList[(i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides].ridgeCatLine.anchorInnerPointlist[k];
						}
						else break;

					}
					//FindPointOnEaveCloser2Plane
					pointMinDis2Plane = float.MaxValue;
					for (int h = 0; h < eaveList[i].ridgeCatLine.anchorInnerPointlist.Count; h++)
					{
						float dis = Mathf.Abs(plane.GetDistanceToPoint(eaveList[i].ridgeCatLine.anchorInnerPointlist[h]));
						if (dis < pointMinDis2Plane)
						{
							pointMinDis2Plane = dis;
							roofSurfaceTileRidgeDownPointPos = eaveList[i].ridgeCatLine.anchorInnerPointlist[h];
						}
						else break;

					}
					 roofSurfaceTileRidgeUpPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					 roofSurfaceTileRidgeUpPoint.transform.parent = newLeftRidgeStruct.body.transform;
					roofSurfaceTileRidgeUpPoint.transform.position = roofSurfaceTileRidgeUpPointPos;
					roofSurfaceTileRidgeUpPoint.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
					roofSurfaceTileRidgeUpPoint.GetComponent<MeshRenderer>().material.color = Color.yellow;
					 roofSurfaceTileRidgeDownPoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					 roofSurfaceTileRidgeDownPoint.transform.parent = newLeftRidgeStruct.body.transform;
					roofSurfaceTileRidgeDownPoint.transform.position = roofSurfaceTileRidgeDownPointPos;
					roofSurfaceTileRidgeDownPoint.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
					roofSurfaceTileRidgeDownPoint.GetComponent<MeshRenderer>().material.color = Color.yellow;
					#region Testing
					roofSurfaceTileRidgeMidPointPos = new Vector3((roofSurfaceTileRidgeUpPointPos.x + roofSurfaceTileRidgeDownPointPos.x) / 2.0f, (newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist[(newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - roofSurfaceTileRidgeStartingIndex) / 2 + roofSurfaceTileRidgeStartingIndex] + planeOffset).y, (roofSurfaceTileRidgeUpPointPos.z + roofSurfaceTileRidgeDownPointPos.z) / 2.0f);
					#endregion
					roofSurfaceTileRidgeMidPointPos = new Vector3((roofSurfaceTileRidgeUpPointPos.x + roofSurfaceTileRidgeDownPointPos.x) / 2.0f, (mainRidgeList[i].ridgeCatLine.controlPointList[mainRidgeList[i].ridgeCatLine.controlPointList.Count - 1].transform.position + mainRidgeList[i].ridgeCatLine.controlPointList[mainRidgeList[i].ridgeCatLine.controlPointList.Count - 2].transform.position).y / 2.0f, (roofSurfaceTileRidgeUpPointPos.z + roofSurfaceTileRidgeDownPointPos.z) / 2.0f) + (mainRidgeHeightOffset) * ((float)n / roofSurfaceTileRidgeCount);

					newLeftRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeUpPointPos);
					newLeftRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeMidPointPos);
					newLeftRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeDownPointPos);
					newLeftRidgeStruct.ridgeCatLine.SetCatmullRom(0.005f, 1);
/*

					GameObject xzxzx = GameObject.CreatePrimitive(PrimitiveType.Sphere);
					xzxzx.transform.position = roofSurfaceTileRidgeMidPointPos;
					xzxzx.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
					xzxzx.GetComponent<MeshRenderer>().material.color = Color.green;*/

									for (int nn = 0; nn < newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count; nn++)
									{
										GameObject hh = GameObject.CreatePrimitive(PrimitiveType.Sphere);
										hh.transform.position = newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist[nn];
										hh.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
										hh.GetComponent<MeshRenderer>().material.color = Color.green;
									}




				}


			}

			break;

		}
	
	}
}

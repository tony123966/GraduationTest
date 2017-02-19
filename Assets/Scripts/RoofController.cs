using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public struct RidgeStruct
{
	public GameObject body;
	public Dictionary<string, GameObject> controlPointDictionaryList;
	public CatLine ridgeCatLine;
	public List<Vector3> tilePosList;
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
	//RoofType************************************************************************************
	public enum RoofType { Zan_Jian_Ding = 0, Wu_Dian_Ding = 1, Lu_Ding = 2, Juan_Peng = 3 };//Zan_Jian_Ding攢尖頂, Wu_Dian_Ding廡殿頂,Lu_Ding盝頂,Juan_Peng卷棚
	public RoofType roofType = RoofType.Zan_Jian_Ding;


	public float roofSurfaceTileWidth = 0.1f;//瓦片長度
	public float roofSurfaceTileHeight = 0.3f;//瓦片高度
	public float allJijaHeight;//總舉架高度
	public float eave2eaveColumnOffset;//出檐長度

	public Vector3 roofTopCenter;
	//Parameter**********************************************************************************
	private enum EaveControlPointType { StartControlPoint, MidRControlPoint, MidControlPoint, MidLControlPoint, EndControlPoint };
	private enum MainRidgeControlPointType { TopControlPoint, MidControlPoint, Connect2eaveColumnControlPoint, DownControlPoint };
	private enum MidRoofSurfaceControlPointType { MidRoofSurfaceTopPoint, MidRoofSurfaceMidPoint, MidRoofSurfaceDownPoint };

	float anchorDis = 0f;
	private float flyEaveHeightOffset = 0.5f;
	private float mainRidgeHeightOffset = -0.5f;
	private float roofSurfaceHeightOffset = -0.1f;
	private float eaveCurveHeightOffset = -0.75f;
	//Model*************************************************************************************
	public GameObject roofSurfaceObj;
	//******************************************************************************************

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

		CreateRoof();
	}
	GameObject CreateControlPoint(GameObject parentObj, Vector3 worldPos, string name = "ControlPoint")
	{
		GameObject newControlPoint = new GameObject(name);
		newControlPoint.transform.position = worldPos;
		newControlPoint.transform.parent = parentObj.transform;
		return newControlPoint;
	}
	RidgeStruct CreateRidgeSturct(string name, GameObject parent)
	{
		RidgeStruct newRidgeStruct = new RidgeStruct();
		newRidgeStruct.controlPointDictionaryList = new Dictionary<string, GameObject>();
		newRidgeStruct.tilePosList = new List<Vector3>();
		newRidgeStruct.body = new GameObject(name);
		newRidgeStruct.body.transform.parent = parent.transform;
		newRidgeStruct.ridgeCatLine = newRidgeStruct.body.AddComponent<CatLine>();

		return newRidgeStruct;
	}
	void CreateRoofSurfaceTile(GameObject cloneObj, RidgeStruct list, RidgeStruct eaveList, RidgeStruct newMidRidgeStruct,int dir)
	{
		Vector3 quaternionVector = Vector3.zero;

		for (int p = 0; p < list.tilePosList.Count; p++)
		{
			Vector3 v1 = (eaveList.controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position - eaveList.controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position) * dir;
			Vector3 v2 = (list.tilePosList[p] - newMidRidgeStruct.tilePosList[p]);
			float angle = Vector3.Angle(v1, v2);

			if (list.tilePosList.Count > 1)
			{
				if (p < list.tilePosList.Count - 1) quaternionVector = list.tilePosList[p + 1] - list.tilePosList[p];
				
			}
			else//修正
			{
				if (list.ridgeCatLine.anchorInnerPointlist.Count > 0) quaternionVector = list.ridgeCatLine.anchorInnerPointlist[0] - list.ridgeCatLine.anchorInnerPointlist[list.ridgeCatLine.anchorInnerPointlist.Count - 1];

			}
			GameObject clone = Instantiate(cloneObj, list.tilePosList[p], cloneObj.transform.rotation) as GameObject;
			clone.transform.rotation = Quaternion.AngleAxis(angle * dir, quaternionVector.normalized) * Quaternion.LookRotation(quaternionVector);
		}
	}
	void ShowPos(Vector3 pos, GameObject parent, Color color, float localScale = 0.2f)
	{
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		obj.transform.position = pos;
		obj.transform.parent = parent.transform;
		obj.transform.localScale = Vector3.one * localScale;
		obj.GetComponent<MeshRenderer>().material.color = color;
	}
	float DisPoint2Line(Vector3 rayOrigin, Vector3 rayDir, Vector3 point)
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
	int FindNearestPointInList2Plane(Plane plane, RidgeStruct list, int starIndex, int endIndex)
	{
		float pointMinDis2Plane = float.MaxValue;
		while (true)
		{
			float dis = Mathf.Abs(plane.GetDistanceToPoint(list.ridgeCatLine.anchorInnerPointlist[starIndex]));
			if ((dis < pointMinDis2Plane))
			{
				pointMinDis2Plane = dis;
				starIndex += (endIndex - starIndex) > 0 ? 1 : -1;
			}
			else
			{
				return starIndex;
			}
		}

	}
	int FindNearestPointInList2Point(Vector3 point, RidgeStruct list, int starIndex, int endIndex)
	{
		float pointMinDis2Plane = float.MaxValue;
		while (true)
		{
			float dis = Vector3.Magnitude((list.ridgeCatLine.anchorInnerPointlist[starIndex] - point));
			if (dis <= pointMinDis2Plane)
			{
				pointMinDis2Plane = dis;
				starIndex += (endIndex - starIndex) > 0 ? 1 : -1;
			}
			else
			{
				return starIndex;
			}
		}
	}
	float CurveInnerPointDis(RidgeStruct list)
	{
		float dis = 0;
		for (int i = 0; i < list.ridgeCatLine.anchorInnerPointlist.Count - 1; i++)
		{
			dis += Vector3.Distance(list.ridgeCatLine.anchorInnerPointlist[i], list.ridgeCatLine.anchorInnerPointlist[i + 1]);
		}
		return dis;
	}
	List<RidgeStruct> CreateMainRidge()
	{
		List<RidgeStruct> mainRidgeList = new List<RidgeStruct>();//主脊

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
			Vector3 eave2eaveColumnOffsetVector = Vector3.Normalize(new Vector3(BodyController.Instance.eaveColumnList[i].topPos.x - roofTopCenter.x, 0, BodyController.Instance.eaveColumnList[i].topPos.z - roofTopCenter.z)) * eave2eaveColumnOffset + flyEaveHeightOffset * Vector3.up;
			Vector3 downControlPointPos = BodyController.Instance.eaveColumnList[i].topPos + eave2eaveColumnOffsetVector;
			GameObject downControlPoint = CreateControlPoint(newRidgeStruct.body, downControlPointPos, MainRidgeControlPointType.DownControlPoint.ToString());
			newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.DownControlPoint.ToString(), downControlPoint);
			//MidControlPoint
			Vector3 midControlPointPos = (topControlPointPos + connect2eaveColumnControlPointPos) / 2.0f + mainRidgeHeightOffset * Vector3.up;
			GameObject midControlPoint = CreateControlPoint(newRidgeStruct.body, midControlPointPos, MainRidgeControlPointType.MidControlPoint.ToString());
			newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.MidControlPoint.ToString(), midControlPoint);

			newRidgeStruct.ridgeCatLine.controlPointList.Add(topControlPoint);
			newRidgeStruct.ridgeCatLine.controlPointList.Add(midControlPoint);
			newRidgeStruct.ridgeCatLine.controlPointList.Add(connect2eaveColumnControlPoint);
			newRidgeStruct.ridgeCatLine.controlPointList.Add(downControlPoint);


			ShowPos(topControlPointPos, newRidgeStruct.body, Color.red, 0.1f);
			ShowPos(midControlPointPos, newRidgeStruct.body, Color.red, 0.1f);
			ShowPos(connect2eaveColumnControlPointPos, newRidgeStruct.body, Color.red, 0.1f);
			ShowPos(downControlPointPos, newRidgeStruct.body, Color.red, 0.1f);

			newRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis);
		}
		return mainRidgeList;

	}
	List<RidgeStruct> CreateEave(List<RidgeStruct> mainRidgeList)
	{
		List<RidgeStruct> eaveList = new List<RidgeStruct>();//檐出

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
			Vector3 midControlPointPos = (startControlPointPos + endControlPointPos) / 2.0f + eaveCurveHeightOffset * Vector3.up;
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

			midRControlPointIndex = FindNearestPointInList2Point(midRControlPointPos, newRidgeStruct, midRControlPointIndex, newRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1);

			for (int n = midRControlPointIndex; n < newRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - midRControlPointIndex; n++)
			{
				float reviseHeight = midControlPointPos.y;
				newRidgeStruct.ridgeCatLine.anchorInnerPointlist[n] = new Vector3(newRidgeStruct.ridgeCatLine.anchorInnerPointlist[n].x, reviseHeight, newRidgeStruct.ridgeCatLine.anchorInnerPointlist[n].z);
			}

		}
		return eaveList;
	}
	private List<RoofSurfaceStruct> CreateRoofSurface(List<RidgeStruct> mainRidgeList, List<RidgeStruct> eaveList)
	{
		List<RoofSurfaceStruct> roofSurfaceStructList = new List<RoofSurfaceStruct>();//屋面
		for (int i = 0; i < (int)MainController.Instance.sides; i++)
		//for (int i = 0; i < 1; i++)
		{

			int nextIndex = (i - 1 + (int)MainController.Instance.sides) % (int)MainController.Instance.sides;
			RoofSurfaceStruct newRoofSurfaceStruct = new RoofSurfaceStruct();
			newRoofSurfaceStruct.body = new GameObject("RoofSurface");
			newRoofSurfaceStruct.body.transform.parent = roof.transform;
			newRoofSurfaceStruct.rightRoofSurfaceTileRidgeList = new List<RidgeStruct>();
			newRoofSurfaceStruct.leftRoofSurfaceTileRidgeList = new List<RidgeStruct>();

			RidgeStruct newMidRidgeStruct = CreateRidgeSturct("MidRoofSurfaceTileRidge", newRoofSurfaceStruct.body);
			newRoofSurfaceStruct.midRoofSurfaceTileRidge = newMidRidgeStruct;

			roofSurfaceStructList.Add(newRoofSurfaceStruct);
			/***************************************找兩個mainRidge中間垂直的roofSurfaceTileRidge***********************************************************/
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


			ShowPos(midRoofSurfaceMidPointPos, newMidRidgeStruct.body, new Color(1, 0, 1), 0.1f);

			/***************************************切割平面 用於切出其他垂直的roofSurfaceTileRidge(右側)***********************************************************/
			//VerCutPlane
			Plane verticalCutPlane = new Plane();
			//plane.Set3Points(midRoofSurfaceDownPointPos, midRoofSurfaceMidPointPos, midRoofSurfaceTopPointPos);
			//plane.normal = Vector3.Cross((midRoofSurfaceDownPoint - midRoofSurfaceTopPoint), (midRoofSurfaceMidPoint - midRoofSurfaceTopPoint)).normalized;
			verticalCutPlane.normal = (eaveList[i].controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position - eaveList[i].controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position).normalized;
			verticalCutPlane.SetNormalAndPosition(verticalCutPlane.normal, midRoofSurfaceTopPointPos);

			float roofSurfaceTileRidge2PlaneMaxDis_Ver = verticalCutPlane.GetDistanceToPoint(mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position + roofSurfaceTileWidth * verticalCutPlane.normal);
			float eaveColumn2PlaneDis_Ver = verticalCutPlane.GetDistanceToPoint(mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.Connect2eaveColumnControlPoint.ToString()].transform.position);
			float eaveColumn2MaxRoofSurfaceTileRidgeDis_Ver = roofSurfaceTileRidge2PlaneMaxDis_Ver - eaveColumn2PlaneDis_Ver;

			int roofSurfaceTileRidgeMaxCount_Ver = Mathf.FloorToInt(Mathf.Abs(roofSurfaceTileRidge2PlaneMaxDis_Ver / roofSurfaceTileWidth));

			int eaveColumn2MaxRoofSurfaceTileRidgeCount_Ver = Mathf.FloorToInt(Mathf.Abs(eaveColumn2MaxRoofSurfaceTileRidgeDis_Ver / roofSurfaceTileWidth));

			//紀錄前一次Index用於迴圈加速
			int roofSurfaceTileRidgeStartingIndex = 0;
			int roofSurface2MainRidgeStartingIndex_R = 0;
			int roofSurface2EaveStartingIndex_R = eaveList[i].ridgeCatLine.anchorInnerPointlist.Count / 2;


			//Right&LeftRoofSurfaceTileRidgeList
			for (int n = 1; n < roofSurfaceTileRidgeMaxCount_Ver; n++)
			{
				//Right
				RidgeStruct newRightRidgeStruct = CreateRidgeSturct("RightRoofSurfaceTileRidge", newRoofSurfaceStruct.body);
				newRoofSurfaceStruct.rightRoofSurfaceTileRidgeList.Add(newRightRidgeStruct);
				RidgeStruct newLeftRidgeStruct = CreateRidgeSturct("LeftRoofSurfaceTileRidge", newRoofSurfaceStruct.body);
				newRoofSurfaceStruct.leftRoofSurfaceTileRidgeList.Add(newLeftRidgeStruct);
				float planeOffset = roofSurfaceTileWidth * n;
				Vector3 planeOffsetVector = planeOffset * verticalCutPlane.normal;

				//檐柱後旋轉角

				/*	if (planeOffset > eaveColumn2PlaneDis)
					{
						v1 = midRoofSurfaceDownPointPos - midRoofSurfaceTopPointPos;
						 v2 =  mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position-midRoofSurfaceTopPointPos;
						angle = Vector2.Angle(v1, v2);

						angle = angle *n/ (eaveColumn2MaxRoofSurfaceTileRidgeCount);
						plane.normal =(Quaternion.Euler(0, angle, 0))*plane.normal;
						plane.SetNormalAndPosition(plane.normal, midRoofSurfaceTopPointPos);
						plane.SetNormalAndPosition(plane.normal, midRoofSurfaceTopPointPos + planeOffsetVector);
					}
					else */
				verticalCutPlane.SetNormalAndPosition(verticalCutPlane.normal, midRoofSurfaceTopPointPos + planeOffsetVector);
				Vector3 roofSurfaceTileRidgeUpPointPos = Vector3.zero;
				Vector3 roofSurfaceTileRidgeDownPointPos = Vector3.zero;

				//FindPointOnMainRidgeCloser2Plane
				int starIndex = FindNearestPointInList2Plane(verticalCutPlane, mainRidgeList[i], roofSurface2MainRidgeStartingIndex_R, mainRidgeList[i].ridgeCatLine.anchorInnerPointlist.Count - 1);

				roofSurfaceTileRidgeUpPointPos = mainRidgeList[i].ridgeCatLine.anchorInnerPointlist[starIndex];

				//FindPointOnEaveCloser2Plane
				starIndex = FindNearestPointInList2Plane(verticalCutPlane, eaveList[i], roofSurface2EaveStartingIndex_R, 0);

				roofSurfaceTileRidgeDownPointPos = eaveList[i].ridgeCatLine.anchorInnerPointlist[starIndex];

				/*

								ShowPos(roofSurfaceTileRidgeUpPointPos, newRightRidgeStruct.body, Color.yellow, 0.1f);

								ShowPos(roofSurfaceTileRidgeDownPointPos, newRightRidgeStruct.body, Color.yellow, 0.1f);*/

				//FindPointOnRoofSurfaceTileMidRidgeStartingIndex

				Plane roofSurfaceStartingIndexCutPlane = new Plane();
				Vector3 roofSurfaceCutPlaneNormal = new Vector3(midRoofSurfaceTopPointPos.x - midRoofSurfaceDownPointPos.x, 0, midRoofSurfaceTopPointPos.z - midRoofSurfaceDownPointPos.z).normalized;
				roofSurfaceStartingIndexCutPlane.SetNormalAndPosition(roofSurfaceCutPlaneNormal, roofSurfaceTileRidgeUpPointPos);

				roofSurfaceTileRidgeStartingIndex = FindNearestPointInList2Plane(roofSurfaceStartingIndexCutPlane, newMidRidgeStruct, roofSurfaceTileRidgeStartingIndex, newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1);


				float ratioA = (float)(n - 1) / (roofSurfaceTileRidgeMaxCount_Ver - 2);
				//Copy
				for (int m = roofSurfaceTileRidgeStartingIndex; m < newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count; m++)
				{
					float ratioB = (float)(m - roofSurfaceTileRidgeStartingIndex) / ((newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - roofSurfaceTileRidgeStartingIndex - 1));

					//修正
					Vector3 pos = newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist[m] + planeOffsetVector;
					float roofSurfaceTileRidgeHeight = (pos.y) * (1.0f - ratioA) + (roofSurfaceTileRidgeUpPointPos.y * (1.0f - ratioB) + roofSurfaceTileRidgeDownPointPos.y * ratioB) * ratioA;
					pos.y = roofSurfaceTileRidgeHeight;
					//Right
					newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Add(pos);
					//修正
					pos = newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist[m] - planeOffsetVector;
					pos.y = roofSurfaceTileRidgeHeight;
					//Left
					newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist.Add(pos);

				}

/*
				for (int nn = 0; nn < newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count; nn++)
				{

					ShowPos(newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[nn], newMidRidgeStruct.body, Color.green, 0.05f);
				}

				for (int nn = 0; nn < newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count; nn++)
				{

					ShowPos(newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist[nn], newMidRidgeStruct.body, Color.green, 0.05f);
				}*/
				/***************************************用AnchorLength取roofSurfaceTileRidge上的瓦片************************************************************/

				newMidRidgeStruct.tilePosList = newMidRidgeStruct.ridgeCatLine.CalculateAnchorPosByList(newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist, roofSurfaceTileHeight);

				newRightRidgeStruct.tilePosList = newRightRidgeStruct.ridgeCatLine.CalculateAnchorPosByList(newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist, roofSurfaceTileHeight);

				newLeftRidgeStruct.tilePosList = newLeftRidgeStruct.ridgeCatLine.CalculateAnchorPosByList(newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist, roofSurfaceTileHeight);
				//計算上仰傾斜角度
				v1 = (mainRidgeList[nextIndex].controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position - mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position);

				v2 = (newMidRidgeStruct.tilePosList[0] - newRightRidgeStruct.tilePosList[0]);

				 angle = Vector3.Angle(v1, v2);

				 CreateRoofSurfaceTile(roofSurfaceObj, newMidRidgeStruct, eaveList[i], newMidRidgeStruct,0);
				 CreateRoofSurfaceTile(roofSurfaceObj, newRightRidgeStruct, eaveList[i], newMidRidgeStruct,1);
				 CreateRoofSurfaceTile(roofSurfaceObj, newLeftRidgeStruct, eaveList[i], newMidRidgeStruct,-1);

			}


		}
		return roofSurfaceStructList;
	}
	public void CreateRoof()
	{
		switch (roofType)
		{
			//攢尖頂
			case RoofType.Zan_Jian_Ding:
				#region  Zan_Jian_Ding

				//MainRidge
				List<RidgeStruct> mainRidgeList = CreateMainRidge();//主脊

				//Eave
				List<RidgeStruct> eaveList = CreateEave(mainRidgeList);//檐出

				//RoofSurface
				List<RoofSurfaceStruct> roofSurfaceStructList = CreateRoofSurface(mainRidgeList, eaveList);//屋面
				#endregion
				break;
			case RoofType.Wu_Dian_Ding:
				#region  Wu_Dian_Ding

				#endregion
				break;
		}

	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
struct ModelStruct
{
	public GameObject model;
	public Vector3 rotation;
	public Vector3 scale;

	public ModelStruct(GameObject model, Vector3 rotation, Vector3 scale)
	{
		this.model = model;
		this.rotation = rotation;
		this.scale = scale;
	}
}
public struct RidgeStruct
{
	public GameObject body;
	public Dictionary<string, GameObject> controlPointDictionaryList;
	public CatLine ridgeCatLine;
	public List<Vector3> tilePosList;


	/*public RidgeStruct(GameObject body, Dictionary<string, GameObject> controlPointDictionaryList, CatLine ridgeCatLine, List<Vector3> tilePosList) 
	{
		 this.body=body;
		 this.controlPointDictionaryList = controlPointDictionaryList;
		 this.ridgeCatLine = ridgeCatLine;
		 this.tilePosList = tilePosList;
	}
	public RidgeStruct(RidgeStruct a)
	{
		this.body = a.body;
		this.controlPointDictionaryList = a.controlPointDictionaryList;
		this.ridgeCatLine = a.ridgeCatLine;
		this.tilePosList = a.tilePosList;
	}*/
}
struct RoofSurfaceStruct
{
	public GameObject body;
	public List<RidgeStruct> rightRoofSurfaceTileRidgeList;
	public List<RidgeStruct> leftRoofSurfaceTileRidgeList;
	public RidgeStruct midRoofSurfaceTileRidge;
}
struct RoofSurfaceModelStruct
{
	public ModelStruct roundTileModelStruct;
	public ModelStruct flatTileModelStruct;
	public ModelStruct eaveTileModelStruct;

	public RoofSurfaceModelStruct(ModelStruct roundTileModelStruct, ModelStruct flatTileModelStruct, ModelStruct eaveTileModelStruct)
	{
		this.roundTileModelStruct = roundTileModelStruct;
		this.flatTileModelStruct = flatTileModelStruct;
		this.eaveTileModelStruct = eaveTileModelStruct;
	}
}
public class RoofController : Singleton<RoofController>
{
	public GameObject roof = null;
	//RoofType************************************************************************************
	public enum RoofType { Zan_Jian_Ding = 0, Wu_Dian_Ding = 1, Lu_Ding = 2, Juan_Peng = 3 };//Zan_Jian_Ding攢尖頂, Wu_Dian_Ding廡殿頂,Lu_Ding盝頂,Juan_Peng卷棚
	public RoofType roofType = RoofType.Zan_Jian_Ding;

	private float allJijaHeight;//總舉架高度
	private float eave2eaveColumnOffset;//出檐長度
	private float beamsHeight;//總梁柱高度

	public Vector3 roofTopCenter;
	//Parameter**********************************************************************************
	private enum EaveControlPointType { StartControlPoint, MidRControlPoint, MidControlPoint, MidLControlPoint, EndControlPoint };
	private enum MainRidgeControlPointType { TopControlPoint, MidControlPoint, Connect2eaveColumnControlPoint, DownControlPoint };
	private enum MidRoofSurfaceControlPointType { MidRoofSurfaceTopPoint, MidRoofSurfaceMidPoint, MidRoofSurfaceDownPoint };

	float anchorDis = 0f;
	public float flyEaveHeightOffset = 1.5f;
	public float mainRidgeHeightOffset = -3f;
	public float roofSurfaceHeightOffset = -1f;
	public float eaveCurveHeightOffset = -2f;
	public float roofSurfaceTileWidth = 0.6f;//瓦片長度
	public float roofSurfaceTileHeight = 0.6f;//瓦片高度
	//Model*************************************************************************************
	RoofSurfaceModelStruct roofSurfaceModelStruct;

	public GameObject roundTileModel;
	public Vector3 roundTileModelRotation = Vector3.zero;
	public Vector3 roundTileModelScale = Vector3.one;
	private ModelStruct roundTileModelStruct;
	public GameObject flatTileModel;
	public Vector3 flatTileModelRotation = Vector3.zero;
	public Vector3 flatTileModelScale = Vector3.one;
	private ModelStruct flatTileModelStruct;
	public GameObject eaveTileModel;
	public Vector3 eaveTileModelRotation = Vector3.zero;
	public Vector3 eaveTileModelScale = Vector3.one;
	private ModelStruct eaveTileModelStruct;
	//******************************************************************************************

	public void InitFunction()
	{
		//初始值******************************************************************************
		allJijaHeight = BodyController.Instance.eaveColumnHeight * 0.9f;
		eave2eaveColumnOffset = BodyController.Instance.eaveColumnHeight * 0.4f;
		beamsHeight = BodyController.Instance.eaveColumnHeight * 0.1f;
		roofTopCenter = BodyController.Instance.bodyCenter + new Vector3(0, BodyController.Instance.eaveColumnHeight / 2.0f + allJijaHeight, 0);


		roundTileModelStruct = new ModelStruct(roundTileModel, roundTileModelRotation, roundTileModelScale);
		flatTileModelStruct = new ModelStruct(flatTileModel, flatTileModelRotation, flatTileModelScale);
		eaveTileModelStruct = new ModelStruct(eaveTileModel, eaveTileModelRotation, eaveTileModelScale);

		roofSurfaceModelStruct = new RoofSurfaceModelStruct(roundTileModelStruct, flatTileModelStruct, eaveTileModelStruct);
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
	void CreateRoofSurfaceTile(RoofSurfaceModelStruct roofSurfaceModelStructGameObject,GameObject parent, RidgeStruct baseList,RidgeStruct refList, RidgeStruct eaveList , int dir)
	{
		if(refList.tilePosList.Count<1|| baseList.tilePosList.Count<1)return;

		Vector3 quaternionVector = Vector3.zero;
		Quaternion rotationVector;
		GameObject mainModel;
		GameObject flatTileModel;
		float flatTileModelHeightOffset = -0.25f;
		List<GameObject> flatTileModelList=new List<GameObject>();

		Vector3 v1 = (eaveList.controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position - eaveList.controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position) * dir;

		for (int p = 0; p < baseList.tilePosList.Count; p++)
		{
			int angleChange= ((refList.tilePosList[p].y >= baseList.tilePosList[p].y) ? -1 : 1);

			if (baseList.tilePosList.Count > 1)
			{
				if (p < baseList.tilePosList.Count - 1)
				{
					if ((p < refList.tilePosList.Count - 2)) quaternionVector = (quaternionVector + (baseList.tilePosList[p] - baseList.tilePosList[p + 1]) + (refList.tilePosList[p + 1] - refList.tilePosList[p + 2])) / 3.0f;
					else quaternionVector = (quaternionVector + (baseList.tilePosList[p] - baseList.tilePosList[p + 1])) / 2.0f;
				}
				else 
				{ 
					if ((p < refList.tilePosList.Count - 2)) quaternionVector = (quaternionVector + (refList.tilePosList[p+1] - refList.tilePosList[p + 2])) / 2.0f;
				}
			}
			else//如果baseList.tilePosList只有一個修正
			{
				if (baseList.ridgeCatLine.anchorInnerPointlist.Count > 0) quaternionVector = baseList.ridgeCatLine.anchorInnerPointlist[baseList.ridgeCatLine.anchorInnerPointlist.Count - 1] - baseList.ridgeCatLine.anchorInnerPointlist[0] ;
			}

			Vector3 v2 = (refList.tilePosList[p] - baseList.tilePosList[p]);
			float angle = Vector3.Angle(v1 , v2);
			//Quaternion rotationVector = Quaternion.AngleAxis(-angle * dir * ((baseList.tilePosList[p].y >= list.tilePosList[p].y)?-1:1), quaternionVector.normalized) * Quaternion.LookRotation(quaternionVector);
			rotationVector = Quaternion.AngleAxis(-angle * dir * angleChange, quaternionVector.normalized) * Quaternion.LookRotation(quaternionVector.normalized);

			//RoundTile&EaveTile
			if (p == 0)
			{
				mainModel = Instantiate(roofSurfaceModelStructGameObject.eaveTileModelStruct.model, baseList.tilePosList[0], roofSurfaceModelStructGameObject.eaveTileModelStruct.model.transform.rotation) as GameObject;
				mainModel.transform.rotation = rotationVector * Quaternion.Euler(roofSurfaceModelStructGameObject.eaveTileModelStruct.rotation);
				mainModel.transform.GetChild(0).localScale = roofSurfaceModelStructGameObject.eaveTileModelStruct.scale;
				mainModel.transform.parent = parent.transform;
			}
			else
			{
				mainModel = Instantiate(roofSurfaceModelStructGameObject.roundTileModelStruct.model, baseList.tilePosList[p], roofSurfaceModelStructGameObject.roundTileModelStruct.model.transform.rotation) as GameObject;
				mainModel.transform.rotation = rotationVector * Quaternion.Euler(roofSurfaceModelStructGameObject.roundTileModelStruct.rotation);
				mainModel.transform.GetChild(0).localScale = roofSurfaceModelStructGameObject.roundTileModelStruct.scale;
				mainModel.transform.parent = parent.transform;
			}
			//FlatTile
			if (dir != 0)
			{
				Vector3 pos=(baseList.tilePosList[p] + refList.tilePosList[p]) / 2.0f + flatTileModelHeightOffset * (Vector3.Cross(quaternionVector, v2)).normalized * dir;
				flatTileModel = Instantiate(roofSurfaceModelStructGameObject.flatTileModelStruct.model, pos , roofSurfaceModelStructGameObject.flatTileModelStruct.model.transform.rotation) as GameObject;
				flatTileModel.transform.rotation = rotationVector * Quaternion.Euler(roofSurfaceModelStructGameObject.flatTileModelStruct.rotation);
				flatTileModel.transform.GetChild(0).localScale = roofSurfaceModelStructGameObject.flatTileModelStruct.scale;
				flatTileModel.transform.parent = mainModel.transform;
				flatTileModelList.Add(flatTileModel);

			}
		}

		//修正
/*
		for (int p = 1; p < flatTileModelList.Count-1; p++)
		{
			Quaternion rotation = Quaternion.Slerp(flatTileModelList[p - 1].transform.rotation, flatTileModelList[p + 1].transform.rotation, 1.0f / 2.0f);
			flatTileModelList[p].transform.rotation = rotation;
		}*/
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
	int FindNearestPointInList2Plane(Plane plane, RidgeStruct list, int startIndex, int endIndex)
	{
		float pointMinDis2Plane = float.MaxValue;
		while (true)
		{
			float dis = Mathf.Abs(plane.GetDistanceToPoint(list.ridgeCatLine.anchorInnerPointlist[startIndex]));
			if ((dis <= pointMinDis2Plane))
			{
				pointMinDis2Plane = dis;
				startIndex += (endIndex - startIndex) > 0 ? 1 : -1;
			}
			else
			{
				return startIndex;
			}
		}

	}
	int FindNearestPointInList2Point(Vector3 point, RidgeStruct list, int startIndex, int endIndex)
	{
		float pointMinDis2Plane = float.MaxValue;
		while (true)
		{
			float dis = Vector3.Magnitude((list.ridgeCatLine.anchorInnerPointlist[startIndex] - point));
			if (dis <= pointMinDis2Plane)
			{
				pointMinDis2Plane = dis;
				startIndex += (endIndex - startIndex) > 0 ? 1 : -1;
			}
			else
			{
				return startIndex;
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
			Vector3 connect2eaveColumnControlPointPos = BodyController.Instance.eaveColumnList[i].topPos+beamsHeight*Vector3.up;
			GameObject connect2eaveColumnControlPoint = CreateControlPoint(newRidgeStruct.body, connect2eaveColumnControlPointPos, MainRidgeControlPointType.Connect2eaveColumnControlPoint.ToString());
			newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.Connect2eaveColumnControlPoint.ToString(), connect2eaveColumnControlPoint);
			//DownControlPoint
			Vector3 eave2eaveColumnOffsetVector = Vector3.Normalize(new Vector3(BodyController.Instance.eaveColumnList[i].topPos.x - roofTopCenter.x, 0, BodyController.Instance.eaveColumnList[i].topPos.z - roofTopCenter.z)) * eave2eaveColumnOffset + flyEaveHeightOffset * Vector3.up;
			Vector3 downControlPointPos = BodyController.Instance.eaveColumnList[i].topPos + eave2eaveColumnOffsetVector+beamsHeight*Vector3.up;
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


			ShowPos(topControlPointPos, newRidgeStruct.body, Color.red, 1f);
			ShowPos(midControlPointPos, newRidgeStruct.body, Color.red, 1f);
			ShowPos(connect2eaveColumnControlPointPos, newRidgeStruct.body, Color.red, 1f);
			ShowPos(downControlPointPos, newRidgeStruct.body, Color.red, 1f);

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
		float threshold=0.5f;
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
			Vector3 midRoofSurfaceMidPointPos = (Quaternion.Euler(0, angle / 2.0f, 0) * (mainRidgeList[i].controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position)) + roofSurfaceHeightOffset * Vector3
			.up;
			GameObject midRoofSurfaceMidPoint = CreateControlPoint(newMidRidgeStruct.body, midRoofSurfaceMidPointPos, MidRoofSurfaceControlPointType.MidRoofSurfaceMidPoint.ToString());
			newMidRidgeStruct.controlPointDictionaryList.Add(MidRoofSurfaceControlPointType.MidRoofSurfaceMidPoint.ToString(), midRoofSurfaceMidPoint);

			//MidRoofSurfaceTileRidge
			newMidRidgeStruct.ridgeCatLine.controlPointList.Add(midRoofSurfaceTopPoint);
			newMidRidgeStruct.ridgeCatLine.controlPointList.Add(midRoofSurfaceMidPoint);
			newMidRidgeStruct.ridgeCatLine.controlPointList.Add(midRoofSurfaceDownPoint);
			newMidRidgeStruct.ridgeCatLine.SetLineNumberOfPoints(1000);
			newMidRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis);


			ShowPos(midRoofSurfaceMidPointPos, newMidRidgeStruct.body, new Color(1, 0, 1), 1f);
			/***************************************用AnchorLength取MidRoofSurfaceTileRidge上的瓦片************************************************************/

			newMidRidgeStruct.tilePosList = newMidRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist, newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count-1,0, roofSurfaceTileHeight);
			//計算上仰傾斜角度=0
			CreateRoofSurfaceTile(roofSurfaceModelStruct, newMidRidgeStruct.body, newMidRidgeStruct, newMidRidgeStruct, eaveList[i], 0);


			/***************************************由Eave上的AnchorPoint切割平面 用於切出其他垂直的roofSurfaceTileRidge(右側)***********************************************************/
			
				RidgeStruct eaveRightRidgeStruct = CreateRidgeSturct("EaveRightRidgeStruct", newRoofSurfaceStruct.body);
			for (int k = 0; k < eaveList[i].ridgeCatLine.anchorInnerPointlist.Count / 2; k++)
			{
				eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Add(eaveList[i].ridgeCatLine.anchorInnerPointlist[k]);
			}


			eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist = eaveRightRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist, eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count-1,0, roofSurfaceTileWidth);

			//eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.RemoveAt(0);

			//VerCutPlane
			Plane verticalCutPlane = new Plane();
			//plane.Set3Points(midRoofSurfaceDownPointPos, midRoofSurfaceMidPointPos, midRoofSurfaceTopPointPos);
			//plane.normal = Vector3.Cross((midRoofSurfaceDownPoint - midRoofSurfaceTopPoint), (midRoofSurfaceMidPoint - midRoofSurfaceTopPoint)).normalized;

			int roofSurfaceTileRidgeMaxCount_Ver = eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count;


			//紀錄前一次Index用於迴圈加速
			int roofSurfaceTileRidgeStartingIndex = 0;
			int roofSurface2MainRidgeStartingIndex_R = 0;

			RidgeStruct lastRightRidgeStruct = newMidRidgeStruct;
			RidgeStruct lastLeftRidgeStruct = newMidRidgeStruct;

			//Right&LeftRoofSurfaceTileRidgeList
			for (int n = 1; n < roofSurfaceTileRidgeMaxCount_Ver; n++)
			{
				verticalCutPlane.normal = Vector3.Project((eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n] - eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n-1]), (eaveList[i].controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position - eaveList[i].controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position)).normalized;
				verticalCutPlane.SetNormalAndPosition(verticalCutPlane.normal, eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n]);

				Vector3 planeOffsetVector = Vector3.Project((eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n] - eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[0]), (eaveList[i].controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position - eaveList[i].controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position));
				//Right
				RidgeStruct newRightRidgeStruct = CreateRidgeSturct("RightRoofSurfaceTileRidge", newRoofSurfaceStruct.body);
				newRoofSurfaceStruct.rightRoofSurfaceTileRidgeList.Add(newRightRidgeStruct);
				RidgeStruct newLeftRidgeStruct = CreateRidgeSturct("LeftRoofSurfaceTileRidge", newRoofSurfaceStruct.body);
				newRoofSurfaceStruct.leftRoofSurfaceTileRidgeList.Add(newLeftRidgeStruct);
				

				Vector3 roofSurfaceTileRidgeUpPointPos = Vector3.zero;
				Vector3 roofSurfaceTileRidgeDownPointPos = Vector3.zero;

				//FindPointOnMainRidgeCloser2Plane
				int starIndex = FindNearestPointInList2Plane(verticalCutPlane, mainRidgeList[i], roofSurface2MainRidgeStartingIndex_R, mainRidgeList[i].ridgeCatLine.anchorInnerPointlist.Count - 1);

				roofSurfaceTileRidgeUpPointPos = mainRidgeList[i].ridgeCatLine.anchorInnerPointlist[starIndex];

				//FindPointOnEaveCloser2Plane

				roofSurfaceTileRidgeDownPointPos = eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n];

				if(Vector3.Distance(roofSurfaceTileRidgeUpPointPos,roofSurfaceTileRidgeDownPointPos)<threshold) break;
/*

				ShowPos(roofSurfaceTileRidgeDownPointPos, newRightRidgeStruct.body, Color.yellow, 0.5f);
				ShowPos(roofSurfaceTileRidgeUpPointPos, newRightRidgeStruct.body, Color.yellow, 0.5f);*/



				//FindPointOnRoofSurfaceTileMidRidgeStartingIndex

				Plane roofSurfaceStartingIndexCutPlane = new Plane();
				Vector3 roofSurfaceCutPlaneNormal = new Vector3(midRoofSurfaceTopPointPos.x - midRoofSurfaceDownPointPos.x, 0, midRoofSurfaceTopPointPos.z - midRoofSurfaceDownPointPos.z).normalized;
				roofSurfaceStartingIndexCutPlane.SetNormalAndPosition(roofSurfaceCutPlaneNormal, roofSurfaceTileRidgeUpPointPos);

				roofSurfaceTileRidgeStartingIndex = FindNearestPointInList2Plane(roofSurfaceStartingIndexCutPlane, newMidRidgeStruct, roofSurfaceTileRidgeStartingIndex, newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1);


				float ratioA = (float)(n-1) / (roofSurfaceTileRidgeMaxCount_Ver-2);
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

				/***************************************用AnchorLength取roofSurfaceTileRidge上的瓦片************************************************************/

				newRightRidgeStruct.tilePosList = newRightRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist, newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count-1,0, roofSurfaceTileHeight);

				newLeftRidgeStruct.tilePosList = newLeftRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist, newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count-1,0, roofSurfaceTileHeight);


				//Debug.Log(lastRightRidgeStruct.body.name + "   " + lastRightRidgeStruct.tilePosList.Count);

				CreateRoofSurfaceTile(roofSurfaceModelStruct, newRightRidgeStruct.body, newRightRidgeStruct, lastRightRidgeStruct,eaveList[i], 1);
				CreateRoofSurfaceTile(roofSurfaceModelStruct, newLeftRidgeStruct.body, newLeftRidgeStruct, lastLeftRidgeStruct, eaveList[i], -1);


				lastRightRidgeStruct = newRightRidgeStruct;
				lastLeftRidgeStruct = newLeftRidgeStruct;

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

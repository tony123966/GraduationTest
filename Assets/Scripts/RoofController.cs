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

struct MainRidgeModelStruct
{
	public ModelStruct mainRidgeTileModelStruct;

	public MainRidgeModelStruct(ModelStruct mainRidgeTileModelStruct)
	{
		this.mainRidgeTileModelStruct = mainRidgeTileModelStruct;
	}
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
	private float Wu_Dian_DingMainRidgeWidth;//廡殿頂主脊長度
	private float Lu_DingMainRidgeOffset;
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
	public float roofSurfaceTileWidth = 0.6f;//屋面瓦片長度
	public float roofSurfaceTileHeight = 0.6f;//屋面瓦片高度
	public float mainRidgeTileHeight = 0.6f;//主脊瓦片高度
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

	MainRidgeModelStruct mainRidgeModelStruct;

	public GameObject mainRidgeTileModel;
	public Vector3 mainRidgeTileModelRotation = Vector3.zero;
	public Vector3 mainRidgeTileModelScale = Vector3.one;
	private ModelStruct mainRidgeTileModelStruct;
	//******************************************************************************************

	public void InitFunction()
	{
		//初始值******************************************************************************
		allJijaHeight = BodyController.Instance.eaveColumnHeight * 0.9f;
		eave2eaveColumnOffset = BodyController.Instance.eaveColumnHeight * 0.4f;
		beamsHeight = BodyController.Instance.eaveColumnHeight * 0.1f;
		roofTopCenter = BodyController.Instance.bodyCenter + new Vector3(0, BodyController.Instance.eaveColumnHeight / 2.0f + allJijaHeight, 0);
		Wu_Dian_DingMainRidgeWidth = BodyController.Instance.eaveColumnHeight * 0.3f;
		Lu_DingMainRidgeOffset = BodyController.Instance.eaveColumnHeight * 0.3f;


		roundTileModelStruct = new ModelStruct(roundTileModel, roundTileModelRotation, roundTileModelScale);
		flatTileModelStruct = new ModelStruct(flatTileModel, flatTileModelRotation, flatTileModelScale);
		eaveTileModelStruct = new ModelStruct(eaveTileModel, eaveTileModelRotation, eaveTileModelScale);
		roofSurfaceModelStruct = new RoofSurfaceModelStruct(roundTileModelStruct, flatTileModelStruct, eaveTileModelStruct);


		mainRidgeTileModelStruct = new ModelStruct(mainRidgeTileModel, mainRidgeTileModelRotation, mainRidgeTileModelScale);
		mainRidgeModelStruct = new MainRidgeModelStruct(mainRidgeTileModelStruct);
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
	RoofSurfaceStruct CreateRoofSurfaceSturct(string name, GameObject parent)
	{
		RoofSurfaceStruct newRoofSurfaceStruct = new RoofSurfaceStruct();
		newRoofSurfaceStruct.rightRoofSurfaceTileRidgeList = new List<RidgeStruct>();
		newRoofSurfaceStruct.leftRoofSurfaceTileRidgeList = new List<RidgeStruct>();
		newRoofSurfaceStruct.body = new GameObject(name);
		newRoofSurfaceStruct.body.transform.parent = parent.transform;
		newRoofSurfaceStruct.midRoofSurfaceTileRidge = new RidgeStruct();

		return newRoofSurfaceStruct;
	}
	/*
		void CreateMainRidgeTile(MainRidgeModelStruct mainRidgeModelStructGameObject, RidgeStruct mainRidgeStruct, RoofSurfaceStruct roofSurfaceStruct, GameObject parent)
		{
			/ ***************************************用AnchorLength取mainRidgeRidge上的瓦片************************************************************ /


			Debug.Log("mainRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count" + mainRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count);
			mainRidgeStruct.tilePosList = mainRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(mainRidgeStruct.ridgeCatLine.anchorInnerPointlist, mainRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1, 0, mainRidgeTileHeight);
			if (mainRidgeStruct.tilePosList.Count < 1) return;

			Vector3 quaternionVector = Vector3.zero;
			for (int p = 0; p < mainRidgeStruct.tilePosList.Count; p++)
			{

				if (p < mainRidgeStruct.tilePosList.Count - 1)
				{
					quaternionVector = (quaternionVector + (mainRidgeStruct.tilePosList[p] - mainRidgeStruct.tilePosList[p + 1])) / 2.0f;
				}

				Quaternion rotationVector = Quaternion.LookRotation(quaternionVector.normalized);

				GameObject mainModel = Instantiate(mainRidgeModelStructGameObject.mainRidgeTileModelStruct.model, mainRidgeStruct.tilePosList[p], mainRidgeModelStructGameObject.mainRidgeTileModelStruct.model.transform.rotation) as GameObject;
				mainModel.transform.rotation = rotationVector * Quaternion.Euler(mainRidgeModelStructGameObject.mainRidgeTileModelStruct.rotation);
				mainModel.transform.GetChild(0).localScale = mainRidgeModelStructGameObject.mainRidgeTileModelStruct.scale;
				mainModel.transform.parent = parent.transform;
			}

		}*/

	void CreateMainRidgeTile(MainRidgeModelStruct mainRidgeModelStructGameObject, RidgeStruct mainRidgeStruct, RoofSurfaceStruct roofSurfaceStruct, GameObject parent)
	{

		float mainRidgeTailHeightOffset = 0.2f;
		RidgeStruct baseList = CreateRidgeSturct("MainRidgeTileStruct", this.gameObject);
		baseList.ridgeCatLine.controlPointPosList.Add(roofSurfaceStruct.midRoofSurfaceTileRidge.tilePosList[roofSurfaceStruct.midRoofSurfaceTileRidge.tilePosList.Count - 1]);


		for (int i = 0; i < roofSurfaceStruct.rightRoofSurfaceTileRidgeList.Count; i += 3)
		{
			baseList.ridgeCatLine.controlPointPosList.Add(roofSurfaceStruct.rightRoofSurfaceTileRidgeList[i].tilePosList[roofSurfaceStruct.rightRoofSurfaceTileRidgeList[i].tilePosList.Count - 1]);
		}
		baseList.ridgeCatLine.controlPointPosList.Add(mainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position);

		baseList.ridgeCatLine.SetLineNumberOfPoints(100);
		baseList.ridgeCatLine.SetCatmullRom(0, 1);


		Plane plane = new Plane();
		Vector3 planeNormal = Vector3.Cross(mainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position - mainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position, Vector3.up).normalized;
		plane.SetNormalAndPosition(planeNormal, mainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position);

		for (int i = 0; i < baseList.ridgeCatLine.anchorInnerPointlist.Count; i++)
		{

			Vector3 offset = (plane.GetSide(baseList.ridgeCatLine.anchorInnerPointlist[i]) ? -1 : 1) * plane.normal * plane.GetDistanceToPoint(baseList.ridgeCatLine.anchorInnerPointlist[i]);
			baseList.ridgeCatLine.anchorInnerPointlist[i] += offset;

		}
		baseList.tilePosList = baseList.ridgeCatLine.CalculateAnchorPosByInnerPointList(baseList.ridgeCatLine.anchorInnerPointlist, baseList.ridgeCatLine.anchorInnerPointlist.Count - 1, 0, mainRidgeTileHeight);

		if (baseList.tilePosList.Count < 1) return;


		Vector3 quaternionVector = Vector3.zero;
		for (int p = 0; p < baseList.tilePosList.Count; p++)
		{

			if (baseList.tilePosList.Count > 1)
			{
				if (p == 0)
				{
					quaternionVector = (baseList.tilePosList[0] - baseList.tilePosList[1]);
				}
				else if ((p != (baseList.tilePosList.Count - 1)))
				{
					quaternionVector = (baseList.tilePosList[p - 1] - baseList.tilePosList[p + 1]);
				}

			}
			else//如果baseList.tilePosList只有一個修正
			{
				if (baseList.ridgeCatLine.anchorInnerPointlist.Count > 0) quaternionVector = baseList.ridgeCatLine.anchorInnerPointlist[baseList.ridgeCatLine.anchorInnerPointlist.Count - 1] - baseList.ridgeCatLine.anchorInnerPointlist[0];
			}
			Quaternion rotationVector = Quaternion.LookRotation(quaternionVector.normalized);

			Vector3 upVector = (Vector3.Cross(quaternionVector, plane.normal)).normalized;
			//Vector3 offset = Vector3.zero;

			GameObject mainModel = Instantiate(mainRidgeModelStructGameObject.mainRidgeTileModelStruct.model, baseList.tilePosList[p] + mainRidgeTailHeightOffset * upVector, mainRidgeModelStructGameObject.mainRidgeTileModelStruct.model.transform.rotation) as GameObject;
			mainModel.transform.rotation = rotationVector * Quaternion.Euler(mainRidgeModelStructGameObject.mainRidgeTileModelStruct.rotation);
			mainModel.transform.GetChild(0).localScale = mainRidgeModelStructGameObject.mainRidgeTileModelStruct.scale;
			mainModel.transform.parent = parent.transform;
		}

	}
	/*
		void CreateMainRidgeTile(MainRidgeModelStruct mainRidgeModelStructGameObject, List<RidgeStruct> mainRidgeStructList, List<RoofSurfaceStruct> roofSurfaceStructList)
		{
			Debug.Log("mainRidgeStructList.Count" + mainRidgeStructList.Count);
			Debug.Log("roofSurfaceStructList.Count" + roofSurfaceStructList.Count);
			if (mainRidgeStructList.Count < 1) return;
			Plane verticalCutPlane = new Plane();
			for(int i=0;i<mainRidgeStructList.Count;i++)
			{
				verticalCutPlane.normal = (roofSurfaceStructList[i].midRoofSurfaceTileRidge.controlPointDictionaryList[MidRoofSurfaceControlPointType.MidRoofSurfaceTopPoint.ToString()].transform.position - roofSurfaceStructList[i].midRoofSurfaceTileRidge.controlPointDictionaryList[MidRoofSurfaceControlPointType.MidRoofSurfaceDownPoint.ToString()].transform.position).normalized;
			
				Vector3 quaternionVector = Vector3.zero;
				float posHeight=0;
				for (int p = 0; p < mainRidgeStructList[i].tilePosList.Count; p++)
				{
					verticalCutPlane.SetNormalAndPosition(verticalCutPlane.normal, mainRidgeStructList[i].tilePosList[p]);

					int starIndex = FindNearestPointInList2Plane(verticalCutPlane, roofSurfaceStructList[i].midRoofSurfaceTileRidge.tilePosList, 0, roofSurfaceStructList[i].midRoofSurfaceTileRidge.tilePosList.Count-1);

					if (p < mainRidgeStructList[i].tilePosList.Count - 1)
					{
						quaternionVector = (quaternionVector + (mainRidgeStructList[i].tilePosList[p] - mainRidgeStructList[i].tilePosList[p + 1])) / 2.0f;
					}
					for(int k=0;k<roofSurfaceStructList[i].rightRoofSurfaceTileRidgeList.Count;k++)
					{
						if((roofSurfaceStructList[i].rightRoofSurfaceTileRidgeList[k].tilePosList.Count-1)>starIndex)
							posHeight=roofSurfaceStructList[i].rightRoofSurfaceTileRidgeList[k].tilePosList[starIndex].y;
						
						else 				
							break;
					}
					Quaternion rotationVector = Quaternion.LookRotation(quaternionVector.normalized);
					Debug.Log("posHeight" + posHeight);
					GameObject mainModel = Instantiate(mainRidgeModelStructGameObject.mainRidgeTileModelStruct.model, new Vector3(mainRidgeStructList[i].tilePosList[p].x, posHeight, mainRidgeStructList[i].tilePosList[p].z), mainRidgeModelStructGameObject.mainRidgeTileModelStruct.model.transform.rotation) as GameObject;
	
					mainModel.transform.rotation = rotationVector * Quaternion.Euler(mainRidgeModelStructGameObject.mainRidgeTileModelStruct.rotation);
					mainModel.transform.GetChild(0).localScale = mainRidgeModelStructGameObject.mainRidgeTileModelStruct.scale;
					mainModel.transform.parent = mainRidgeStructList[i].body.transform;
				}
		
			}
		}*/

	/*
		void CreateMainRidgeTile(MainRidgeModelStruct mainRidgeModelStructGameObject, List<RidgeStruct> mainRidgeStructList, List<RoofSurfaceStruct> roofSurfaceStructList)
		{

		}*/
	void CreateRoofSurfaceTile(RoofSurfaceModelStruct roofSurfaceModelStructGameObject, GameObject parent, RidgeStruct baseList, RidgeStruct refList, RidgeStruct midRidgeStruct, RidgeStruct eaveStructList, int dir)
	{
		if (refList.tilePosList.Count < 1 || baseList.tilePosList.Count < 1) return;

		Vector3 quaternionVector = Vector3.zero;
		Quaternion rotationVector = Quaternion.identity;
		GameObject mainModel;
		GameObject flatTileModel;
		float flatTileModelHeightOffset = -0.35f;
		float xAngle = 0;
		List<GameObject> flatTileModelList = new List<GameObject>();
		Vector3 v1 = Quaternion.AngleAxis(-Vector3.Angle(baseList.tilePosList[0] - eaveStructList.controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position, midRidgeStruct.tilePosList[0] - eaveStructList.controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position), (midRidgeStruct.tilePosList[0] - midRidgeStruct.tilePosList[midRidgeStruct.tilePosList.Count - 1]).normalized * dir) * (eaveStructList.controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position - eaveStructList.controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position) * dir;
		Vector3 v3 = (eaveStructList.controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position - eaveStructList.controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position) * dir;
		Vector3 v2 = Vector3.zero;
		for (int p = 0; p < baseList.tilePosList.Count; p++)
		{
			int angleChange = ((refList.tilePosList[p].y >= baseList.tilePosList[p].y) ? -1 : 1);

			if (baseList.tilePosList.Count > 1)
			{
				if (p == 0)
				{
					quaternionVector = (baseList.tilePosList[0] - baseList.tilePosList[1]);
				}
				else if ((p != (baseList.tilePosList.Count - 1)))
				{
					quaternionVector = (baseList.tilePosList[p - 1] - baseList.tilePosList[p + 1]);
				}

			}
			else//如果baseList.tilePosList只有一個修正
			{
				if (baseList.ridgeCatLine.anchorInnerPointlist.Count > 0) quaternionVector = baseList.ridgeCatLine.anchorInnerPointlist[baseList.ridgeCatLine.anchorInnerPointlist.Count - 1] - baseList.ridgeCatLine.anchorInnerPointlist[0];
			}

			v2 = (refList.tilePosList[p] - baseList.tilePosList[p]);
			xAngle = (Vector3.Angle(v3, v2)) * angleChange;
			rotationVector = Quaternion.AngleAxis(-xAngle * dir, quaternionVector.normalized) * Quaternion.LookRotation(quaternionVector.normalized);
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

			if (baseList.tilePosList.Count > 1)
			{
				if (p < baseList.tilePosList.Count - 1)
				{
					if ((p < refList.tilePosList.Count - 2)) quaternionVector = (quaternionVector + (baseList.tilePosList[p] - baseList.tilePosList[p + 1]) + (refList.tilePosList[p + 1] - refList.tilePosList[p + 2])) / 3.0f;
					else quaternionVector = (quaternionVector + (baseList.tilePosList[p] - baseList.tilePosList[p + 1])) / 2.0f;
				}
				else
				{
					if ((p < refList.tilePosList.Count - 2)) quaternionVector = (quaternionVector + (refList.tilePosList[p + 1] - refList.tilePosList[p + 2])) / 2.0f;
				}
			}
			else//如果baseList.tilePosList只有一個修正
			{
				if (baseList.ridgeCatLine.anchorInnerPointlist.Count > 0) quaternionVector = baseList.ridgeCatLine.anchorInnerPointlist[baseList.ridgeCatLine.anchorInnerPointlist.Count - 1] - baseList.ridgeCatLine.anchorInnerPointlist[0];
			}
			xAngle = (Vector3.Angle(v1, v2)) * angleChange;
			rotationVector = Quaternion.AngleAxis(-xAngle * dir, quaternionVector.normalized) * Quaternion.LookRotation(quaternionVector.normalized);
			//FlatTile
			if (dir != 0)
			{
				Vector3 upVector = (Vector3.Cross(quaternionVector, v2)).normalized * dir;
				Vector3 pos = (baseList.tilePosList[p] + refList.tilePosList[p]) / 2.0f + flatTileModelHeightOffset * upVector;
				flatTileModel = Instantiate(roofSurfaceModelStructGameObject.flatTileModelStruct.model, pos, roofSurfaceModelStructGameObject.flatTileModelStruct.model.transform.rotation) as GameObject;
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
	int FindNearestPointInList2Plane(Plane plane, List<Vector3> list, int startIndex, int endIndex)
	{
		float pointMinDis2Plane = float.MaxValue;
		while (true)
		{
			float dis = Mathf.Abs(plane.GetDistanceToPoint(list[startIndex]));
			if (dis < pointMinDis2Plane)
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
	int FindNearestPointInList2Point(Vector3 point, List<Vector3> list, int startIndex, int endIndex)
	{
		float pointMinDis2Plane = float.MaxValue;
		while (true)
		{
			float dis = Vector3.Magnitude((list[startIndex] - point));
			if (dis < pointMinDis2Plane)
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
	RidgeStruct CreateMainRidgeStruct(int eaveColumnListIndex, Vector3 topControlPointPos)
	{
	
			RidgeStruct newRidgeStruct = CreateRidgeSturct("MainRidge", roof);

			//TopControlPoint
			GameObject topControlPoint = CreateControlPoint(newRidgeStruct.body, topControlPointPos, MainRidgeControlPointType.TopControlPoint.ToString());
			newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.TopControlPoint.ToString(), topControlPoint);
			//Connect2eaveColumnControlPoint
			Vector3 connect2eaveColumnControlPointPos = BodyController.Instance.eaveColumnList[eaveColumnListIndex].topPos + beamsHeight * Vector3.up;
			GameObject connect2eaveColumnControlPoint = CreateControlPoint(newRidgeStruct.body, connect2eaveColumnControlPointPos, MainRidgeControlPointType.Connect2eaveColumnControlPoint.ToString());
			newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.Connect2eaveColumnControlPoint.ToString(), connect2eaveColumnControlPoint);
			//DownControlPoint
			Vector3 eave2eaveColumnOffsetVector = Vector3.Normalize(new Vector3(BodyController.Instance.eaveColumnList[eaveColumnListIndex].topPos.x - topControlPointPos.x, 0, BodyController.Instance.eaveColumnList[eaveColumnListIndex].topPos.z - topControlPointPos.z)) * eave2eaveColumnOffset + flyEaveHeightOffset * Vector3.up;
			Vector3 downControlPointPos = BodyController.Instance.eaveColumnList[eaveColumnListIndex].topPos + eave2eaveColumnOffsetVector + beamsHeight * Vector3.up;
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


/*
			ShowPos(topControlPointPos, newRidgeStruct.body, Color.red, 1f);
			ShowPos(midControlPointPos, newRidgeStruct.body, Color.red, 1f);
			ShowPos(connect2eaveColumnControlPointPos, newRidgeStruct.body, Color.red, 1f);
			ShowPos(downControlPointPos, newRidgeStruct.body, Color.red, 1f);*/

			newRidgeStruct.ridgeCatLine.SetLineNumberOfPoints(1000);
			newRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis);
		/*	for(int i=0;i<newRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count;i++)
			{
				ShowPos(newRidgeStruct.ridgeCatLine.anchorInnerPointlist[i], newRidgeStruct.body,Color.green);
			}*/
		
			return newRidgeStruct;

	}
	RidgeStruct CreateEaveStruct(RidgeStruct RightMainRidgeStruct,RidgeStruct LeftMainRidgeStruct)
	{

		RidgeStruct newRidgeStruct = CreateRidgeSturct("Eave", roof);


			//StartControlPoint
			Vector3 startControlPointPos = RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position;
			GameObject startControlPoint = CreateControlPoint(newRidgeStruct.body, startControlPointPos, EaveControlPointType.StartControlPoint.ToString());
			newRidgeStruct.controlPointDictionaryList.Add(EaveControlPointType.StartControlPoint.ToString(), startControlPoint);
			//EndControlPoint
			Vector3 endControlPointPos = LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position;
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

			newRidgeStruct.ridgeCatLine.SetLineNumberOfPoints(1000);
			newRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis);
			//修正
			int midRControlPointIndex = 0;

			midRControlPointIndex = FindNearestPointInList2Point(midRControlPointPos, newRidgeStruct.ridgeCatLine.anchorInnerPointlist, midRControlPointIndex, newRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1);

			for (int n = midRControlPointIndex; n < newRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - midRControlPointIndex; n++)
			{
				float reviseHeight = midControlPointPos.y;
				newRidgeStruct.ridgeCatLine.anchorInnerPointlist[n] = new Vector3(newRidgeStruct.ridgeCatLine.anchorInnerPointlist[n].x, reviseHeight, newRidgeStruct.ridgeCatLine.anchorInnerPointlist[n].z);
			}

		return newRidgeStruct;
	}
	private RoofSurfaceStruct CreateRoofSurface(RidgeStruct RightMainRidgeStruct,RidgeStruct LeftMainRidgeStruct, RidgeStruct eaveStruct)
	{
		RoofSurfaceStruct roofSurfaceStruct = new RoofSurfaceStruct();//屋面
		float threshold = 0.5f;
		//for (int i = 0; i < (int)MainController.Instance.sides; i++)

		RoofSurfaceStruct newRoofSurfaceStruct = CreateRoofSurfaceSturct("RoofSurface", roof);

		RidgeStruct newMidRidgeStruct = CreateRidgeSturct("MidRoofSurfaceTileRidge", newRoofSurfaceStruct.body);


		/***************************************找兩個mainRidge中間垂直的roofSurfaceTileRidge***********************************************************/
		//FindMidRoofSurfaceMidPoint
		Vector2 v1 = new Vector2(LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.Connect2eaveColumnControlPoint.ToString()].transform.position.x - LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position.x, LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.Connect2eaveColumnControlPoint.ToString()].transform.position.z - LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position.z);
		Vector2 v2 = new Vector2(RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.Connect2eaveColumnControlPoint.ToString()].transform.position.x - RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position.x, RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.Connect2eaveColumnControlPoint.ToString()].transform.position.z - RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position.z);

		float angle = Vector2.Angle(v1, v2);

		//midRoofSurfaceTopPoint
		Vector3 midRoofSurfaceTopPointPos = (RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position + LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position)/2.0f;
		GameObject midRoofSurfaceTopPoint = CreateControlPoint(newMidRidgeStruct.body, midRoofSurfaceTopPointPos, MidRoofSurfaceControlPointType.MidRoofSurfaceTopPoint.ToString());
		newMidRidgeStruct.controlPointDictionaryList.Add(MidRoofSurfaceControlPointType.MidRoofSurfaceTopPoint.ToString(), midRoofSurfaceTopPoint);
		//midRoofSurfaceDownPoint
		Vector3 midRoofSurfaceDownPointPos = eaveStruct.controlPointDictionaryList[EaveControlPointType.MidControlPoint.ToString()].transform.position;
		GameObject midRoofSurfaceDownPoint = CreateControlPoint(newMidRidgeStruct.body, midRoofSurfaceDownPointPos, MidRoofSurfaceControlPointType.MidRoofSurfaceDownPoint.ToString());
		newMidRidgeStruct.controlPointDictionaryList.Add(MidRoofSurfaceControlPointType.MidRoofSurfaceDownPoint.ToString(), midRoofSurfaceDownPoint);
		//midRoofSurfaceMidPoint
		//Vector3 midRoofSurfaceMidPointPos = (Quaternion.Euler(0, angle / 2.0f, 0) * (RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position)) + (roofSurfaceHeightOffset * Vector3.up) - (RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position - LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position) / 2.0f;
		Vector3 midRoofSurfaceMidPointPos = (RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position + LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position) / 2.0f + (roofSurfaceHeightOffset * Vector3.Cross((RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position - RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position), (LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position - RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position)).normalized);
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

		newMidRidgeStruct.tilePosList = newMidRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist, newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1, 0, roofSurfaceTileHeight);
		//計算上仰傾斜角度=0
		CreateRoofSurfaceTile(roofSurfaceModelStruct, newMidRidgeStruct.body, newMidRidgeStruct, newMidRidgeStruct, newMidRidgeStruct, eaveStruct, 0);


		/***************************************由Eave上的AnchorPoint切割平面 用於切出其他垂直的roofSurfaceTileRidge(右側)***********************************************************/

		RidgeStruct eaveRightRidgeStruct = CreateRidgeSturct("EaveRightRidgeStruct", newRoofSurfaceStruct.body);
		for (int k = 0; k < eaveStruct.ridgeCatLine.anchorInnerPointlist.Count / 2; k++)
		{
			eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Add(eaveStruct.ridgeCatLine.anchorInnerPointlist[k]);
		}


		eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist = eaveRightRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist, eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1, 0, roofSurfaceTileWidth);

		//eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.RemoveAt(0);

		//VerCutPlane
		Plane verticalCutPlane = new Plane();
		//plane.Set3Points(midRoofSurfaceDownPointPos, midRoofSurfaceMidPointPos, midRoofSurfaceTopPointPos);
		//plane.normal = Vector3.Cross((midRoofSurfaceDownPoint - midRoofSurfaceTopPoint), (midRoofSurfaceMidPoint - midRoofSurfaceTopPoint)).normalized;

		int roofSurfaceTileRidgeMaxCount_Ver = eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count;



		/*for(int m=0;m<mainRidgeStructList[i].ridgeCatLine.anchorInnerPointlist.Count;m++)
		{
			ShowPos(mainRidgeStructList[i].ridgeCatLine.anchorInnerPointlist[m], mainRidgeStructList[i].body, Color.yellow, 0.5f);
		}*/

		//紀錄前一次Index用於迴圈加速
		int roofSurfaceTileRidgeStartingIndex = 0;
		int roofSurface2MainRidgeStartingIndex_R = 0;

		RidgeStruct lastRightRidgeStruct = newMidRidgeStruct;
		RidgeStruct lastLeftRidgeStruct = newMidRidgeStruct;

		//Right&LeftRoofSurfaceTileRidgeList
		for (int n = 1; n < roofSurfaceTileRidgeMaxCount_Ver; n++)
		{
			verticalCutPlane.normal = Vector3.Project((eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n] - eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n - 1]), (eaveStruct.controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position - eaveStruct.controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position)).normalized;
			verticalCutPlane.SetNormalAndPosition(verticalCutPlane.normal, eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n]);

			Vector3 planeOffsetVector = Vector3.Project((eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n] - eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[0]), (eaveStruct.controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position - eaveStruct.controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position));
			//Right
			RidgeStruct newRightRidgeStruct = CreateRidgeSturct("RightRoofSurfaceTileRidge", newRoofSurfaceStruct.body);

			RidgeStruct newLeftRidgeStruct = CreateRidgeSturct("LeftRoofSurfaceTileRidge", newRoofSurfaceStruct.body);


			Vector3 roofSurfaceTileRidgeUpPointPos = Vector3.zero;
			Vector3 roofSurfaceTileRidgeDownPointPos = Vector3.zero;

			//FindPointOnMainRidgeCloser2Plane
			int starIndex = FindNearestPointInList2Plane(verticalCutPlane, RightMainRidgeStruct.ridgeCatLine.anchorInnerPointlist, roofSurface2MainRidgeStartingIndex_R, RightMainRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1);

			roofSurfaceTileRidgeUpPointPos = RightMainRidgeStruct.ridgeCatLine.anchorInnerPointlist[starIndex];

			//FindPointOnEaveCloser2Plane

			roofSurfaceTileRidgeDownPointPos = eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n];
		
			//翼角末端修正
			//if (Vector3.Distance(roofSurfaceTileRidgeUpPointPos, roofSurfaceTileRidgeDownPointPos) < threshold) break;


			/*		ShowPos(roofSurfaceTileRidgeDownPointPos, newRightRidgeStruct.body, Color.yellow, 0.5f);
					ShowPos(roofSurfaceTileRidgeUpPointPos, newRightRidgeStruct.body, Color.yellow, 0.5f);*/



			//FindPointOnRoofSurfaceTileMidRidgeStartingIndex

			Plane roofSurfaceStartingIndexCutPlane = new Plane();
			Vector3 roofSurfaceCutPlaneNormal = new Vector3(midRoofSurfaceTopPointPos.x - midRoofSurfaceDownPointPos.x, 0, midRoofSurfaceTopPointPos.z - midRoofSurfaceDownPointPos.z).normalized;
			roofSurfaceStartingIndexCutPlane.SetNormalAndPosition(roofSurfaceCutPlaneNormal, roofSurfaceTileRidgeUpPointPos);

			roofSurfaceTileRidgeStartingIndex = FindNearestPointInList2Plane(roofSurfaceStartingIndexCutPlane, newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist, roofSurfaceTileRidgeStartingIndex, newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1);

			float ratioA = ((float)(n - 1) / ((roofSurfaceTileRidgeMaxCount_Ver - 2)));
			//Copy
			for (int m = roofSurfaceTileRidgeStartingIndex; m < newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count; m++)
			{
				float ratioB = (float)(m - roofSurfaceTileRidgeStartingIndex) / ((newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - roofSurfaceTileRidgeStartingIndex - 1));

				//修正
				Vector3 pos = newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist[m] + planeOffsetVector;
				float roofSurfaceTileRidgeHeight = (pos.y) * (1.0f - ratioA) + ((roofSurfaceTileRidgeUpPointPos.y * (1.0f - ratioB) + roofSurfaceTileRidgeDownPointPos.y * ratioB) * ratioA);
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


			newRightRidgeStruct.tilePosList = newRightRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist, newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1, 0, roofSurfaceTileHeight);

			newLeftRidgeStruct.tilePosList = newLeftRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist, newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1, 0, roofSurfaceTileHeight);


			//Debug.Log(lastRightRidgeStruct.body.name + "   " + lastRightRidgeStruct.tilePosList.Count);

			CreateRoofSurfaceTile(roofSurfaceModelStruct, newRightRidgeStruct.body, newRightRidgeStruct, lastRightRidgeStruct, newMidRidgeStruct, eaveStruct, 1);
			CreateRoofSurfaceTile(roofSurfaceModelStruct, newLeftRidgeStruct.body, newLeftRidgeStruct, lastLeftRidgeStruct, newMidRidgeStruct, eaveStruct, -1);


			lastRightRidgeStruct = newRightRidgeStruct;
			lastLeftRidgeStruct = newLeftRidgeStruct;

			newRoofSurfaceStruct.rightRoofSurfaceTileRidgeList.Add(newRightRidgeStruct);
			newRoofSurfaceStruct.leftRoofSurfaceTileRidgeList.Add(newLeftRidgeStruct);
		}
		newRoofSurfaceStruct.midRoofSurfaceTileRidge = newMidRidgeStruct;

		//CreateMainRidgeTile(mainRidgeModelStruct, RightMainRidgeStruct, newRoofSurfaceStruct, RightMainRidgeStruct.body);

		return roofSurfaceStruct;
	}
	void CopyRoofFunction(GameObject roofObject,float angle,Vector3 rotationCenter,int times)
	{
		for (int i = 1; i < times; i++)
		{
			GameObject clone = Instantiate(roofObject, roofObject.transform.position, roofObject.transform.rotation) as GameObject;
			clone.transform.RotateAround(rotationCenter, Vector3.up, angle * i);
			clone.transform.parent=MainController.Instance.building.transform;
		}
	
	}
	public void CreateRoof()
	{
		RidgeStruct RightMainRidgeStruct;
		RidgeStruct LeftMainRidgeStruct;
		RidgeStruct RightMainRidgeStructA;
		RidgeStruct LeftMainRidgeStructA;
		RidgeStruct eaveStruct;
		RidgeStruct eaveStructA;
		RoofSurfaceStruct roofSurfaceStructList;
		RoofSurfaceStruct roofSurfaceStructListA;
		Vector3 offsetVector;
		switch (roofType)
		{
			//攢尖頂
			case RoofType.Zan_Jian_Ding:
				#region  Zan_Jian_Ding

				//主脊-MainRidge輔助線 
				 RightMainRidgeStruct = CreateMainRidgeStruct(0, roofTopCenter);
				 LeftMainRidgeStruct = CreateMainRidgeStruct(1, roofTopCenter);

				//Eave輔助線
				 eaveStruct = CreateEaveStruct(RightMainRidgeStruct,LeftMainRidgeStruct);//檐出

				//RoofSurface
				 roofSurfaceStructList = CreateRoofSurface(RightMainRidgeStruct,LeftMainRidgeStruct, eaveStruct);//屋面
			
				//複製出其他屋面
				CopyRoofFunction(roof, 360.0f / (int)MainController.Instance.sides, roofTopCenter, (int)MainController.Instance.sides);

				#endregion
				break;
			case RoofType.Wu_Dian_Ding:
				#region  Wu_Dian_Ding

				MainController.Instance.sides = MainController.FormFactorType.FourSide;

				offsetVector = (BodyController.Instance.eaveColumnList[0].topPos - BodyController.Instance.eaveColumnList[1].topPos).normalized * Wu_Dian_DingMainRidgeWidth;
				Vector3 rightRoofTopCenter = roofTopCenter + offsetVector;
				Vector3 leftRoofTopCenter=roofTopCenter - offsetVector;

				//主脊-MainRidge輔助線 
				 RightMainRidgeStruct = CreateMainRidgeStruct(0, rightRoofTopCenter);
				 LeftMainRidgeStruct = CreateMainRidgeStruct(1, leftRoofTopCenter);
				//Eave輔助線
				 eaveStruct = CreateEaveStruct(RightMainRidgeStruct,LeftMainRidgeStruct);//檐出

				//RoofSurface
				roofSurfaceStructList = CreateRoofSurface(RightMainRidgeStruct,LeftMainRidgeStruct, eaveStruct);//屋面

				//主脊-MainRidge輔助線 
				RightMainRidgeStructA = LeftMainRidgeStruct;
				 LeftMainRidgeStructA= CreateMainRidgeStruct(2, leftRoofTopCenter);
				//Eave輔助線
				 eaveStructA = CreateEaveStruct(RightMainRidgeStructA,LeftMainRidgeStructA);//檐出

				//RoofSurface
				 roofSurfaceStructListA = CreateRoofSurface(RightMainRidgeStructA,LeftMainRidgeStructA, eaveStructA);//屋面

				//複製出其他屋面
				CopyRoofFunction(roof, 180, roofTopCenter,2);
				#endregion
				break;

			case RoofType.Lu_Ding:
				#region  Lu_Ding

				MainController.Instance.sides = MainController.FormFactorType.FourSide;

				//主脊-MainRidge輔助線 
				offsetVector = (new Vector3(BodyController.Instance.eaveColumnList[0].topPos.x - roofTopCenter.x, 0, BodyController.Instance.eaveColumnList[0].topPos.z - roofTopCenter.z)).normalized * Lu_DingMainRidgeOffset;
				RightMainRidgeStruct = CreateMainRidgeStruct(0, roofTopCenter + offsetVector);

				offsetVector = (new Vector3(BodyController.Instance.eaveColumnList[1].topPos.x - roofTopCenter.x, 1, BodyController.Instance.eaveColumnList[1].topPos.z - roofTopCenter.z)).normalized * Lu_DingMainRidgeOffset;
				LeftMainRidgeStruct = CreateMainRidgeStruct(1, roofTopCenter + offsetVector);
				//Eave輔助線
				eaveStruct = CreateEaveStruct(RightMainRidgeStruct, LeftMainRidgeStruct);//檐出

				//RoofSurface
				roofSurfaceStructList = CreateRoofSurface(RightMainRidgeStruct, LeftMainRidgeStruct, eaveStruct);//屋面

				//主脊-MainRidge輔助線 
				RightMainRidgeStructA = LeftMainRidgeStruct;
				offsetVector = (new Vector3(BodyController.Instance.eaveColumnList[2].topPos.x - roofTopCenter.x, 2, BodyController.Instance.eaveColumnList[2].topPos.z - roofTopCenter.z)).normalized * Lu_DingMainRidgeOffset;
				LeftMainRidgeStructA = CreateMainRidgeStruct(2, roofTopCenter + offsetVector);
				//Eave輔助線
				eaveStructA = CreateEaveStruct(RightMainRidgeStructA, LeftMainRidgeStructA);//檐出

				//RoofSurface
				roofSurfaceStructListA = CreateRoofSurface(RightMainRidgeStructA, LeftMainRidgeStructA, eaveStructA);//屋面
				//複製出其他屋面
				CopyRoofFunction(roof, 180, roofTopCenter, 2);
				#endregion
				break;
		}

	}
}

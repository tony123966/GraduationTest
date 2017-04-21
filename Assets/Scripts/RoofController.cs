using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#region LeastSquare
class CurveFitting
{
	int degree = 2;

	List<float> a;
	public CurveFitting(List<float> x, List<float> y)
	{
		int i, j, k;
		int size = Mathf.Min(x.Count, y.Count);

		List<float> X = new List<float>();
		for (i = 0; i < 2 * degree + 1; i++)
		{
			X.Add(0);
			for (j = 0; j < size; j++)
				X[i] = X[i] + Mathf.Pow(x[j], i);        //consecutive positions of the array will store N,sigma(xi),sigma(xi^2),sigma(xi^3)....sigma(xi^2n)
		}

		List<List<float>> B = new List<List<float>>();//B is the Normal matrix(augmented) that will store the equations, 'a' is for value of the final coefficients
		for (i = 0; i < degree + 1; i++)
		{
			List<float> c = new List<float>();
			for (j = 0; j < degree + 2; j++)
			{
				c.Add(0);
			}
			B.Add(c);
		}

		a = new List<float>();
		for (i = 0; i < degree + 1; i++)
		{
			a.Add(0);
		}
		for (i = 0; i <= degree; i++)
			for (j = 0; j <= degree; j++)
				B[i][j] = X[i + j];

		List<float> Y = new List<float>();
		for (i = 0; i < degree + 1; i++)
		{
			Y.Add(0);
			for (j = 0; j < size; j++)
				Y[i] = Y[i] + Mathf.Pow(x[j], i) * y[j];        //consecutive positions will store sigma(yi),sigma(xi*yi),sigma(xi^2*yi)...sigma(xi^n*yi)
		}
		for (i = 0; i <= degree; i++)
			B[i][degree + 1] = Y[i];                //load the values of Y as the last column of B(Normal Matrix but augmented)
		degree = degree + 1;

		for (i = 0; i < degree; i++)                    //From now Gaussian Elimination starts(can be ignored) to solve the set of linear equations (Pivotisation)
			for (k = i + 1; k < degree; k++)
				if (B[i][i] < B[k][i])
					for (j = 0; j <= degree; j++)
					{
						float temp = B[i][j];
						B[i][j] = B[k][j];
						B[k][j] = temp;
					}

		for (i = 0; i < degree - 1; i++)            //loop to perform the gauss elimination
			for (k = i + 1; k < degree; k++)
			{
				float t = B[k][i] / B[i][i];
				for (j = 0; j <= degree; j++)
					B[k][j] = B[k][j] - t * B[i][j];    //make the elements below the pivot elements equal to zero or elimnate the variables
			}
		for (i = degree - 1; i >= 0; i--)                //back-substitution
		{                        //x is an array whose values correspond to the values of x,y,z..
			a[i] = B[i][degree];                //make the variable to be calculated equal to the rhs of the last equation
			for (j = 0; j < degree; j++)
				if (j != i)            //then subtract all the lhs values except the coefficient of the variable whose value                                   is being calculated
					a[i] = a[i] - B[i][j] * a[j];
			a[i] = a[i] / B[i][i];            //now finally divide the rhs by the coefficient of the variable to be calculated
		}
	}
	public float getY(float x)
	{
		float result = 0;
		for (int i = 0; i < degree; i++)
		{
			result += a[i] * Mathf.Pow(x, i);
		}
		return result;
	}

};
class LeastSquare
{
	float a, b;

	public LeastSquare(List<float> x, List<float> y)
	{
		float t1 = 0, t2 = 0, t3 = 0, t4 = 0;
		for (int i = 0; i < x.Count; ++i)
		{
			t1 += x[i] * x[i];
			t2 += x[i];
			t3 += x[i] * y[i];
			t4 += y[i];
		}
		a = (t3 * x.Count - t2 * t4) / (t1 * x.Count - t2 * t2);
		b = (t1 * t4 - t2 * t3) / (t1 * x.Count - t2 * t2);
	}

	public float getY(float x)
	{
		return a * x + b;
	}

};
class LeastSquareExponential
{
	float a, b;

	public LeastSquareExponential(List<float> x, List<float> y)
	{
		float t1 = 0, t2 = 0, t3 = 0, t4 = 0;
		int size = Mathf.Min(x.Count, y.Count);

		for (int i = 0; i < size; i++)
		{
			y[i] = Mathf.Log(y[i]);
		}
		for (int i = 0; i < size; ++i)
		{
			t1 = t1 + x[i];
			t2 = t2 + x[i] * x[i];
			t3 = t3 + y[i];
			t4 = t4 + x[i] * y[i];
		}
		a = ((t2 * t3 - t1 * t4) / (size * t2 - t1 * t1));
		b = ((size * t4 - t1 * t3) / (size * t2 - t1 * t1));
		a = Mathf.Exp(a);
	}
	public float getY(float x)
	{
		//y = ae^(bx)
		return a * Mathf.Exp(b * x);
	}

};
#endregion
public struct RidgeStruct//脊
{
	public GameObject body;
	public Dictionary<string, GameObject> controlPointDictionaryList;
	public CatLine ridgeCatLine;
	public List<Vector3> tilePosList;

}
public struct RoofSurfaceStruct//屋面
{
	public GameObject body;
	public List<RidgeStruct> rightRoofSurfaceTileRidgeList;
	public List<RidgeStruct> leftRoofSurfaceTileRidgeList;
	public RidgeStruct midRoofSurfaceTileRidge;
}
public struct MainRidgeModelStruct//主脊模型
{
	public ModelStruct mainRidgeTileModelStruct;

	public MainRidgeModelStruct(ModelStruct mainRidgeTileModelStruct)
	{
		this.mainRidgeTileModelStruct = mainRidgeTileModelStruct;
	}
}
public struct RoofSurfaceModelStruct//屋面模型
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
public class RoofController:MonoBehaviour
{
	public List<GameObject> roofList = new List<GameObject>();
	//RoofType************************************************************************************
	public enum RoofType { Zan_Jian_Ding = 0, Wu_Dian_Ding = 1, Lu_Ding = 2, Juan_Peng = 3 };//Zan_Jian_Ding攢尖頂, Wu_Dian_Ding廡殿頂,Lu_Ding盝頂,Juan_Peng卷棚
	public RoofType roofType = RoofType.Zan_Jian_Ding;
	private enum EaveControlPointType { StartControlPoint, MidRControlPoint, MidControlPoint, MidLControlPoint, EndControlPoint };//屋簷
	private enum MainRidgeControlPointType { TopControlPoint, MidControlPoint,Connect2EaveColumnControlPoint, DownControlPoint };//主脊
	private float allJijaHeight;//總舉架高度(總屋頂高度)
	private float eave2eaveColumnOffset;//主脊方向檐柱至檐出長度
	//private float eave2FlyEaveOffset;//
	private float beamsHeight;//總梁柱高度
	private float Wu_Dian_DingMainRidgeWidth;//廡殿頂主脊長度
	private float Lu_DingMainRidgeOffset;//盝頂垂脊長度
	public Vector3 roofTopCenter;
	public Vector3 doubleRoofTopCenter;
	//Parameter**********************************************************************************
	private enum MidRoofSurfaceControlPointType { MidRoofSurfaceTopPoint, MidRoofSurfaceMidPoint, MidRoofSurfaceDownPoint };//屋面

	public float flyEaveHeightOffset = 3.0f;//飛簷上翹程度
	public float mainRidgeOffset = -3f;//主脊曲線上翹程度
	public float roofSurfaceHeightOffset = -2f;//屋面曲線上翹程度
	public float eaveCurveHeightOffset = -4f;//屋簷高度
	public float roofSurfaceTileWidth = 0.7f;//屋面瓦片長度
	public float roofSurfaceTileHeight = 0.95f;//屋面瓦片高度
	public float mainRidgeTileHeight = 0.3f;//主脊瓦片高度

	public bool isDoubleRoof=true;
	//公式變數*************************************************************************************
	float anchorDis = 0f;//曲線innerPoint換算anchorPoint間距


	public void InitFunction(PlatformController platformController, BodyController bodyController)
	{
		//初始值******************************************************************************
		allJijaHeight = bodyController.eaveColumnHeight * 1.2f;
		eave2eaveColumnOffset = bodyController.eaveColumnHeight * 0.5f;

		beamsHeight = bodyController.eaveColumnHeight * 0.2f;
		roofTopCenter = bodyController.bodyCenter + new Vector3(0, bodyController.eaveColumnHeight / 2.0f + allJijaHeight, 0);
		doubleRoofTopCenter = bodyController.bodyCenter + new Vector3(0, bodyController.eaveColumnHeight / 2.0f + allJijaHeight/2.0f, 0);
		Wu_Dian_DingMainRidgeWidth = platformController.platformFrontWidth * 0.5f;

		Debug.Log("roofTopCenter" + roofTopCenter);
		//************************************************************************************


		CreateRoof( platformController, bodyController);
	}
	GameObject CreateControlPoint(GameObject parentObj, Vector3 worldPos, string name = "ControlPoint")//Create控制點
	{
		GameObject newControlPoint = new GameObject(name);
		newControlPoint.transform.position = worldPos;
		newControlPoint.transform.parent = parentObj.transform;
		return newControlPoint;
	}
	RidgeStruct CreateRidgeSturct(string name, GameObject parent)//初始 脊
	{
		RidgeStruct newRidgeStruct = new RidgeStruct();
		newRidgeStruct.controlPointDictionaryList = new Dictionary<string, GameObject>();
		newRidgeStruct.tilePosList = new List<Vector3>();
		newRidgeStruct.body = new GameObject(name);
		newRidgeStruct.body.transform.parent = parent.transform;
		newRidgeStruct.ridgeCatLine = newRidgeStruct.body.AddComponent<CatLine>();

		return newRidgeStruct;
	}
	RoofSurfaceStruct CreateRoofSurfaceSturct(string name, GameObject parent)//初始 脊
	{
		RoofSurfaceStruct newRoofSurfaceStruct = new RoofSurfaceStruct();
		newRoofSurfaceStruct.rightRoofSurfaceTileRidgeList = new List<RidgeStruct>();
		newRoofSurfaceStruct.leftRoofSurfaceTileRidgeList = new List<RidgeStruct>();
		newRoofSurfaceStruct.body = new GameObject(name);
		newRoofSurfaceStruct.body.transform.parent = parent.transform;
		newRoofSurfaceStruct.midRoofSurfaceTileRidge = new RidgeStruct();

		return newRoofSurfaceStruct;
	}
	RidgeStruct CreateMainRidgeTile(MainRidgeModelStruct mainRidgeModelStructGameObject, RidgeStruct mainRidgeStruct, RoofSurfaceStruct roofSurfaceStruct, RidgeStruct eaveRidgeStruct, GameObject parent)//主脊瓦片
	{
		Debug.Log("CreateMainRidgeTile");
		float mainRidgeTailHeightOffset = 0.0f;
		RidgeStruct baseList = CreateRidgeSturct("MainRidgeTileStruct", parent.gameObject);

		Vector3 planeNormal = Vector3.Cross(mainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position - mainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position, Vector3.up).normalized;

		baseList.ridgeCatLine.controlPointList = mainRidgeStruct.ridgeCatLine.controlPointList;
		baseList.ridgeCatLine.SetLineNumberOfPoints(10000);
		baseList.ridgeCatLine.SetCatmullRom(0);

		baseList.tilePosList = baseList.ridgeCatLine.CalculateAnchorPosByInnerPointList(baseList.ridgeCatLine.anchorInnerPointlist, baseList.ridgeCatLine.anchorInnerPointlist.Count - 1, 0, mainRidgeTileHeight);

		if (baseList.tilePosList.Count < 1) return baseList;
		Debug.Log("baseList.tilePosList.Count>=1");
		Vector3 quaternionVector = Vector3.zero;
		for (int p = 0; p < baseList.tilePosList.Count; p++)
		{

			if (baseList.tilePosList.Count > 1)
			{
				if (p == 0)
				{
					quaternionVector = (baseList.tilePosList[0] - baseList.tilePosList[1]);
				}
				else
				{
					if ((p < baseList.tilePosList.Count - 1))
						quaternionVector = (baseList.tilePosList[p - 1] - baseList.tilePosList[p + 1]);
				}

			}
			else//如果baseList.tilePosList只有一個修正
			{
				if (baseList.ridgeCatLine.anchorInnerPointlist.Count > 0) quaternionVector = baseList.ridgeCatLine.anchorInnerPointlist[baseList.ridgeCatLine.anchorInnerPointlist.Count - 1] - baseList.ridgeCatLine.anchorInnerPointlist[0];
			}
			Quaternion rotationVector = Quaternion.LookRotation(quaternionVector.normalized);

			Vector3 upVector = (Vector3.Cross(quaternionVector, planeNormal)).normalized;
			//Vector3 offset = Vector3.zero;
			//Debug.Log("p " + p + " rotation " + rotationVector * Quaternion.Euler(mainRidgeModelStructGameObject.mainRidgeTileModelStruct.rotation));
			GameObject mainModel = Instantiate(mainRidgeModelStructGameObject.mainRidgeTileModelStruct.model, baseList.tilePosList[p] + mainRidgeTailHeightOffset * upVector, mainRidgeModelStructGameObject.mainRidgeTileModelStruct.model.transform.rotation) as GameObject;
			mainModel.transform.rotation = rotationVector * Quaternion.Euler(mainRidgeModelStructGameObject.mainRidgeTileModelStruct.rotation);
			mainModel.transform.GetChild(0).localScale = mainRidgeModelStructGameObject.mainRidgeTileModelStruct.scale;
			mainModel.transform.parent = parent.transform;
		}
		return baseList;

	}

	List<List<GameObject>> CreateRoofSurfaceTile(RoofSurfaceModelStruct roofSurfaceModelStructGameObject, GameObject parent, RidgeStruct baseList, RidgeStruct refList, RidgeStruct midRidgeStruct, RidgeStruct eaveStructList, int dir, RidgeStruct mainRidgeStruct, Vector3 roofSurfaceTileRidgeUpPointPos)
	{
		List<List<GameObject>>allModelList=new List<List<GameObject>>();

		if (refList.tilePosList.Count < 1 || baseList.tilePosList.Count < 1) return allModelList;

		List<GameObject> eaveTileModelList = new List<GameObject>();
		List<GameObject> roundTileModelList = new List<GameObject>();
		List<GameObject> flatTileModelList = new List<GameObject>();

		Vector3 quaternionVector = Vector3.zero;
		Quaternion rotationVector = Quaternion.identity;

		float flatTileModelHeightOffset = -0.35f;

		Vector3 v1 = new Vector3(midRidgeStruct.controlPointDictionaryList[MidRoofSurfaceControlPointType.MidRoofSurfaceTopPoint.ToString()].transform.position.x - midRidgeStruct.controlPointDictionaryList[MidRoofSurfaceControlPointType.MidRoofSurfaceDownPoint.ToString()].transform.position.x, 0, midRidgeStruct.controlPointDictionaryList[MidRoofSurfaceControlPointType.MidRoofSurfaceTopPoint.ToString()].transform.position.z - midRidgeStruct.controlPointDictionaryList[MidRoofSurfaceControlPointType.MidRoofSurfaceDownPoint.ToString()].transform.position.z);
		Vector3 v3 = (eaveStructList.controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position - eaveStructList.controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position) * dir;
		Vector3 v2 = Vector3.zero;
		//修正
		Plane constraintPlaneA = new Plane();
		Plane constraintPlaneB = new Plane();

		int p = 0;
		int angleChange = 1;
		int dirChange = 0;
		int dirAngleChange = 1;
		Vector3 upVector = Vector3.up;
		Vector3 pos = Vector3.zero;
		float lastXAngle = 0;
		int lastDirAngleChange = 0;
		for (; p < baseList.tilePosList.Count; p++)
		{

			GameObject mainModel;
			List<Vector3> sumVector = new List<Vector3>();
			Vector3 sum = Vector3.zero;

			if (baseList.tilePosList.Count > 1)
			{

				if (p == 0)
				{
					sum = Vector3.zero;
					sumVector.Clear();
					quaternionVector = (baseList.tilePosList[0] - baseList.tilePosList[1]);
					if (refList.tilePosList.Count > 1) sumVector.Add(refList.tilePosList[0] - refList.tilePosList[1]);
				}
				else
				{
					sum = Vector3.zero;
					sumVector.Clear();

					if ((p < refList.tilePosList.Count - 1)) sumVector.Add((refList.tilePosList[p - 1] - refList.tilePosList[p + 1]));//refList p
					//if ((p < refList.tilePosList.Count - 2)) sumVector.Add((refList.tilePosList[p] - refList.tilePosList[p + 2]));//refList p+1
					//if ((p > 2) && (p < refList.tilePosList.Count)) sumVector.Add((refList.tilePosList[p - 2] - refList.tilePosList[p]));//refList p-1
					if ((p < baseList.tilePosList.Count - 1)) sumVector.Add((baseList.tilePosList[p - 1] - baseList.tilePosList[p + 1]));//baseList p
					if ((p < baseList.tilePosList.Count - 2)) sumVector.Add((baseList.tilePosList[p] - baseList.tilePosList[p + 2]));//baseList p+1

				}

			}
			else//如果baseList.tilePosList只有一個修正
			{
				sum = Vector3.zero;
				sumVector.Clear();
				if (baseList.ridgeCatLine.anchorInnerPointlist.Count > 0) quaternionVector = baseList.ridgeCatLine.anchorInnerPointlist[baseList.ridgeCatLine.anchorInnerPointlist.Count - 1] - baseList.ridgeCatLine.anchorInnerPointlist[0];
				if (refList.tilePosList.Count > 1) sumVector.Add(refList.tilePosList[0] - refList.tilePosList[1]);
			}

			for (int i = 0; i < sumVector.Count; i++)
			{
				sum += sumVector[i];
			}
			quaternionVector = (quaternionVector + sum) / (sumVector.Count + 1);

			dirAngleChange = (((Mathf.Sign(Vector3.Dot(v1.normalized, quaternionVector.normalized)) < 0)) ? 1 : -1);
			dirChange = (((Mathf.Sign(Vector3.Dot(v1.normalized, quaternionVector.normalized)) < 0)) ? 0 : 180);

			if (p < refList.tilePosList.Count) v2 = (refList.tilePosList[p] - baseList.tilePosList[p]);

			upVector = (Vector3.Cross(quaternionVector, v2)).normalized * dir;

			//if (p < refList.tilePosList.Count) angleChange = ((refList.tilePosList[p].y >= baseList.tilePosList[p].y) ? -1 : 1) * dirAngleChange;
			angleChange = ((Vector3.Dot(v3.normalized, upVector) <= 0) ? -1 : 1);
			float xAngle = (Vector3.Angle(v3, v2) * dir * angleChange + dirChange);
			if ((p != 0) && (lastDirAngleChange == dirAngleChange)) xAngle = (xAngle + lastXAngle) / 2.0f;

			lastXAngle = xAngle;
			lastDirAngleChange = dirAngleChange;


			rotationVector = Quaternion.AngleAxis(-xAngle, quaternionVector.normalized) * Quaternion.LookRotation(quaternionVector.normalized);

			//Vector3 constraintPlaneNormalB = Vector3.Cross(mainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position - mainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position, (Vector3.Cross(midRidgeStruct.controlPointDictionaryList[MidRoofSurfaceControlPointType.MidRoofSurfaceMidPoint.ToString()].transform.position - midRidgeStruct.controlPointDictionaryList[MidRoofSurfaceControlPointType.MidRoofSurfaceTopPoint.ToString()].transform.position, v2))).normalized;
			Vector3 constraintPlaneNormalA = Vector3.Cross(mainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position - mainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position, Vector3.up).normalized;
			//ShowPos(roofSurfaceTileRidgeUpPointPos, baseList.body, Color.black,0.8f);
			constraintPlaneA.SetNormalAndPosition(constraintPlaneNormalA, mainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position);
			constraintPlaneB.SetNormalAndPosition((baseList.tilePosList[p] - baseList.tilePosList[Mathf.Max(p - 1, 0)]).normalized, roofSurfaceTileRidgeUpPointPos);
			//if (constraintPlaneB.SameSide(baseList.tilePosList[p], baseList.tilePosList[Mathf.Max(p - 1, 0)]) && constraintPlaneA.SameSide(baseList.tilePosList[p], midRidgeStruct.controlPointDictionaryList[MidRoofSurfaceControlPointType.MidRoofSurfaceMidPoint.ToString()].transform.position))
			//if ( constraintPlaneA.SameSide(baseList.tilePosList[p], midRidgeStruct.controlPointDictionaryList[MidRoofSurfaceControlPointType.MidRoofSurfaceMidPoint.ToString()].transform.position))
			//if (constraintPlaneA.SameSide(baseList.tilePosList[p], baseList.tilePosList[Mathf.Max(p - 1, 0)]))
			//(Mathf.Abs(constraintPlane.GetDistanceToPoint(baseList.tilePosList[p])) > (roofSurfaceTileHeight/2.0f))&&
			//if (constraintPlane.SameSide(baseList.tilePosList[p], baseList.tilePosList[0]))

			//if (Vector3.Dot((baseList.tilePosList[p] - roofSurfaceTileRidgeUpPointPos), v1) == Vector3.Dot((baseList.tilePosList[Mathf.Max(p - 1, 0)] - roofSurfaceTileRidgeUpPointPos),v1))
			{
				//RoundTile&EaveTile
				if (p == 0)
				{
					GameObject eaveTileModel = Instantiate(roofSurfaceModelStructGameObject.eaveTileModelStruct.model, baseList.tilePosList[0], roofSurfaceModelStructGameObject.eaveTileModelStruct.model.transform.rotation) as GameObject;
					eaveTileModel.transform.rotation = rotationVector * Quaternion.Euler(roofSurfaceModelStructGameObject.eaveTileModelStruct.rotation);

					eaveTileModel.transform.GetChild(0).localScale = roofSurfaceModelStructGameObject.eaveTileModelStruct.scale;
					eaveTileModel.transform.parent = parent.transform;
					eaveTileModelList.Add(eaveTileModel);
					mainModel = eaveTileModel;
				}
				else
				{
					GameObject roundTileModel = Instantiate(roofSurfaceModelStructGameObject.roundTileModelStruct.model, baseList.tilePosList[p], roofSurfaceModelStructGameObject.roundTileModelStruct.model.transform.rotation) as GameObject;
					roundTileModel.transform.rotation = rotationVector * Quaternion.Euler(roofSurfaceModelStructGameObject.roundTileModelStruct.rotation);
					roundTileModel.transform.GetChild(0).localScale = roofSurfaceModelStructGameObject.roundTileModelStruct.scale;
					roundTileModel.transform.parent = parent.transform;
					roundTileModelList.Add(roundTileModel);
					mainModel = roundTileModel;
				}

				//FlatTile
				if (dir != 0)
				{
					upVector = (Vector3.Cross(quaternionVector, v2)).normalized * dir;

					if (p < refList.tilePosList.Count) pos = (baseList.tilePosList[p] + refList.tilePosList[p]) / 2.0f + flatTileModelHeightOffset * upVector;
					GameObject flatTileModel = Instantiate(roofSurfaceModelStructGameObject.flatTileModelStruct.model, pos, roofSurfaceModelStructGameObject.flatTileModelStruct.model.transform.rotation) as GameObject;
					flatTileModel.transform.rotation = rotationVector * Quaternion.Euler(roofSurfaceModelStructGameObject.flatTileModelStruct.rotation);
					flatTileModel.transform.GetChild(0).localScale = roofSurfaceModelStructGameObject.flatTileModelStruct.scale;
					flatTileModel.transform.parent = mainModel.transform;
					flatTileModelList.Add(flatTileModel);

				}
			}
			//else break;
		}
		allModelList.Add(eaveTileModelList);
		allModelList.Add(roundTileModelList);
		allModelList.Add(flatTileModelList);

		return allModelList;
	}
	public void AdvancedMerge(GameObject obj)
	{
		// All our children (and us)
		MeshFilter[] filters = obj.GetComponentsInChildren<MeshFilter>();

		// All the meshes in our children (just a big list)
		List<Material> materials = new List<Material>();
		MeshRenderer[] renderers = obj.GetComponentsInChildren<MeshRenderer>(); // <-- you can optimize this
		foreach (MeshRenderer renderer in renderers)
		{
			if (renderer.transform == transform)
				continue;
			Material[] localMats = renderer.sharedMaterials;
			foreach (Material localMat in localMats)
				if (!materials.Contains(localMat))
					materials.Add(localMat);
		}

		// Each material will have a mesh for it.
		List<Mesh> submeshes = new List<Mesh>();
		foreach (Material material in materials)
		{
			// Make a combiner for each (sub)mesh that is mapped to the right material.
			List<CombineInstance> combiners = new List<CombineInstance>();
			foreach (MeshFilter filter in filters)
			{
				if (filter.transform == transform) continue;
				// The filter doesn't know what materials are involved, get the renderer.
				MeshRenderer renderer = filter.GetComponent<MeshRenderer>();  // <-- (Easy optimization is possible here, give it a try!)
				if (renderer == null)
				{
					Debug.LogError(filter.name + " has no MeshRenderer");
					continue;
				}

				// Let's see if their materials are the one we want right now.
				Material[] localMaterials = renderer.sharedMaterials;
				for (int materialIndex = 0; materialIndex < localMaterials.Length; materialIndex++)
				{
					if (localMaterials[materialIndex] != material)
						continue;
					// This submesh is the material we're looking for right now.
					CombineInstance ci = new CombineInstance();
					ci.mesh = filter.sharedMesh;
					ci.subMeshIndex = materialIndex;
					ci.transform = Matrix4x4.identity;
					combiners.Add(ci);
				}
			}
			// Flatten into a single mesh.
			Mesh mesh = new Mesh();
			mesh.CombineMeshes(combiners.ToArray(), true);
			submeshes.Add(mesh);
		}

		// The final mesh: combine all the material-specific meshes as independent submeshes.
		List<CombineInstance> finalCombiners = new List<CombineInstance>();
		foreach (Mesh mesh in submeshes)
		{
			CombineInstance ci = new CombineInstance();
			ci.mesh = mesh;
			ci.subMeshIndex = 0;
			ci.transform = Matrix4x4.identity;
			finalCombiners.Add(ci);
		}
		Mesh finalMesh = new Mesh();
		finalMesh.CombineMeshes(finalCombiners.ToArray(), false);

		obj.AddComponent<MeshRenderer>();
		obj.AddComponent<MeshFilter>().sharedMesh = finalMesh;
	}
	public void MeshCombineInGameObjectList(List<GameObject> list,GameObject parent,Material materials) 
	{
		MeshFilter[] meshFilters = new MeshFilter[list.Count];
		for (int i = 0; i < list.Count; i++)
		{
			meshFilters[i] = list[i].GetComponentInChildren<MeshFilter>();
		}
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];

		for (int i = 0; i < meshFilters.Length; i++)
		{
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			meshFilters[i].gameObject.SetActive(false);
		}
		parent.AddComponent<MeshFilter>().mesh = new Mesh();
		parent.AddComponent<MeshRenderer>();
		parent.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
		parent.GetComponent<MeshRenderer>().material = materials;
	
	}
	public void MeshCombineInGameObjectList(GameObject obj,Material materials)
	{
		MeshFilter[] meshFilters = obj.GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];

		for (int i = 0; i < meshFilters.Length; i++)
		{
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			meshFilters[i].gameObject.SetActive(false);
		}
		obj.AddComponent<MeshFilter>().mesh = new Mesh();
		obj.AddComponent<MeshRenderer>();
		obj.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
		obj.GetComponent<MeshRenderer>().material = materials;
	}
	void ShowPos(Vector3 pos, GameObject parent, Color color, float localScale = 0.2f)
	{
		GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		obj.transform.position = pos;
		obj.transform.parent = parent.transform;
		obj.transform.localScale = Vector3.one * localScale;
		obj.GetComponent<MeshRenderer>().material.color = color;
	}
	int FindNearestPointInList2Plane(Plane plane, List<Vector3> list, int startIndex, int endIndex, float threshold = 0)
	{
		float pointMinDis2Plane = float.MaxValue;
		int index = startIndex;
		int dir = ((endIndex - startIndex) > 0 ? 1 : -1);
		for (int i = startIndex; ((dir == 1) ? (i < endIndex) : (i > endIndex)); i += dir)
		{
			float dis = Mathf.Abs(plane.GetDistanceToPoint(list[i]));
			if (threshold == 0)
			{
				if ((dis < pointMinDis2Plane))
				{
					pointMinDis2Plane = dis;
					index = i;
				}
			}
			else
			{
				if (dis < threshold)
				{
					if ((dis < pointMinDis2Plane))
					{
						pointMinDis2Plane = dis;
						index = i;
					}

				}

			}

		}
		return index;

	}

	int FindNearestPointInList2Plane(Plane plane, List<Vector3> list, int startIndex, int endIndex, Plane constraintPlane, Ray intersectionRay, float constraintDis = 0)
	{
		float pointMinDis2Plane = float.MaxValue;

		int index = startIndex;
		int dir = ((endIndex - startIndex) > 0 ? 1 : -1);

		for (int i = startIndex; ((dir == 1) ? (i < endIndex) : (i > endIndex)); i += dir)
		{
			float dis = Mathf.Abs(plane.GetDistanceToPoint(list[i]));
			float rayDistance = 0;
			if ((constraintPlane.Raycast(intersectionRay, out rayDistance)))
			{
				if (((rayDistance <= constraintDis) && (constraintDis != 0)) || (constraintDis == 0))
				{
					if ((dis < pointMinDis2Plane))
					{
						pointMinDis2Plane = dis;
						index = i;
					}
				}
			}
		}
		return index;

	}
	int FindNearestPointInList2Point(Vector3 point, List<Vector3> list, int startIndex, int endIndex)
	{
		float pointMinDis2Plane = float.MaxValue;

		int index = startIndex;
		int dir = ((endIndex - startIndex) > 0 ? 1 : -1);
		for (int i = startIndex; ((dir == 1) ? (i < endIndex) : (i > endIndex)); i += dir)
		{
			float dis = Vector3.Magnitude((list[i] - point));
			if (dis < pointMinDis2Plane)
			{
				pointMinDis2Plane = dis;
				index = i;
			}
		}
		return index;
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
	RidgeStruct CreateMainRidgeStruct(List<CylinderMesh> eaveColumnList, int eaveColumnListIndex, Vector3 topControlPointPos)
	{
		RidgeStruct newRidgeStruct = CreateRidgeSturct("MainRidge", this.gameObject);

		//TopControlPoint
		GameObject topControlPoint = CreateControlPoint(newRidgeStruct.body, topControlPointPos, MainRidgeControlPointType.TopControlPoint.ToString());
		newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.TopControlPoint.ToString(), topControlPoint);
		//Connect2eaveColumnControlPoint
		Vector3 connect2EaveColumnControlPointPos = eaveColumnList[eaveColumnListIndex].topPos + beamsHeight * Vector3.up;
		GameObject connect2EaveColumnControlPoint = CreateControlPoint(newRidgeStruct.body, connect2EaveColumnControlPointPos, MainRidgeControlPointType.Connect2EaveColumnControlPoint.ToString());
		newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.Connect2EaveColumnControlPoint.ToString(), connect2EaveColumnControlPoint);
		//DownControlPoint
		Vector3 eave2eaveColumnOffsetVector = Vector3.Normalize(new Vector3(eaveColumnList[eaveColumnListIndex].topPos.x - topControlPointPos.x, 0, eaveColumnList[eaveColumnListIndex].topPos.z - topControlPointPos.z)) * eave2eaveColumnOffset + flyEaveHeightOffset * Vector3.up;
		Vector3 downControlPointPos = eaveColumnList[eaveColumnListIndex].topPos + eave2eaveColumnOffsetVector + beamsHeight * Vector3.up;
		GameObject downControlPoint = CreateControlPoint(newRidgeStruct.body, downControlPointPos, MainRidgeControlPointType.DownControlPoint.ToString());
		newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.DownControlPoint.ToString(), downControlPoint);
		//float ratio=3.0f/4.0f;
		float ratio = 0.5f;
		//Vector3 midControlPointPos = (topControlPointPos * (1 - ratio) + downControlPointPos * ratio) + mainRidgeOffset * Vector3.up;
		Vector3 midControlPointPos = (topControlPointPos * (1 - ratio) + connect2EaveColumnControlPointPos * ratio) + mainRidgeOffset * Vector3.up;
		GameObject midControlPoint = CreateControlPoint(newRidgeStruct.body, midControlPointPos, MainRidgeControlPointType.MidControlPoint.ToString());
		newRidgeStruct.controlPointDictionaryList.Add(MainRidgeControlPointType.MidControlPoint.ToString(), midControlPoint);

		newRidgeStruct.ridgeCatLine.controlPointList.Add(topControlPoint);
		newRidgeStruct.ridgeCatLine.controlPointList.Add(midControlPoint);
		newRidgeStruct.ridgeCatLine.controlPointList.Add(connect2EaveColumnControlPoint);
		newRidgeStruct.ridgeCatLine.controlPointList.Add(downControlPoint);



		ShowPos(topControlPointPos, newRidgeStruct.body, Color.red, 1f);
		//ShowPos(connect2eaveColumnControlPointPos, newRidgeStruct.body, Color.red, 1f);
		ShowPos(downControlPointPos, newRidgeStruct.body, Color.red, 1f);

		newRidgeStruct.ridgeCatLine.SetLineNumberOfPoints(1000);
		newRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis);

		for (int i = 0; i < newRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count; i++)
		{
			//ShowPos(newRidgeStruct.ridgeCatLine.anchorInnerPointlist[i], newRidgeStruct.body, Color.green);
		}

		return newRidgeStruct;

	}
	RidgeStruct CreateEaveStruct(RidgeStruct RightMainRidgeStruct, RidgeStruct LeftMainRidgeStruct)
	{

		RidgeStruct newRidgeStruct = CreateRidgeSturct("Eave", this.gameObject);


		//StartControlPoint
		Vector3 startControlPointPos = RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position;
		GameObject startControlPoint = CreateControlPoint(newRidgeStruct.body, startControlPointPos, EaveControlPointType.StartControlPoint.ToString());
		newRidgeStruct.controlPointDictionaryList.Add(EaveControlPointType.StartControlPoint.ToString(), startControlPoint);
		//EndControlPoint
		Vector3 endControlPointPos = LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position;
		GameObject endControlPoint = CreateControlPoint(newRidgeStruct.body, endControlPointPos, EaveControlPointType.EndControlPoint.ToString());
		newRidgeStruct.controlPointDictionaryList.Add(EaveControlPointType.EndControlPoint.ToString(), endControlPoint);

		float eave2FlyEaveOffset = Vector3.Distance(startControlPointPos, endControlPointPos) * 0.1f;
		//MidControlPoint
		Vector3 midControlPointPos = (startControlPointPos + endControlPointPos) / 2.0f + eaveCurveHeightOffset * Vector3.up;
		GameObject midControlPoint = CreateControlPoint(newRidgeStruct.body, midControlPointPos, EaveControlPointType.MidControlPoint.ToString());
		newRidgeStruct.controlPointDictionaryList.Add(EaveControlPointType.MidControlPoint.ToString(), midControlPoint);
		//MidRightControlPoint
		Vector3 midRControlPointPos = startControlPointPos - (startControlPointPos - endControlPointPos).normalized * eave2FlyEaveOffset + eaveCurveHeightOffset * Vector3.up;
		GameObject midRControlPoint = CreateControlPoint(newRidgeStruct.body, midRControlPointPos, EaveControlPointType.MidRControlPoint.ToString());
		newRidgeStruct.controlPointDictionaryList.Add(EaveControlPointType.MidRControlPoint.ToString(), midRControlPoint);
		//MidLeftControlPoint
		Vector3 midLControlPointPos = endControlPointPos + (startControlPointPos - endControlPointPos).normalized * eave2FlyEaveOffset + eaveCurveHeightOffset * Vector3.up;
		GameObject midLControlPoint = CreateControlPoint(newRidgeStruct.body, midLControlPointPos, EaveControlPointType.MidLControlPoint.ToString());
		newRidgeStruct.controlPointDictionaryList.Add(EaveControlPointType.MidLControlPoint.ToString(), midLControlPoint);

		newRidgeStruct.ridgeCatLine.controlPointList.Add(startControlPoint);
		newRidgeStruct.ridgeCatLine.controlPointList.Add(midRControlPoint);
		newRidgeStruct.ridgeCatLine.controlPointList.Add(midControlPoint);
		newRidgeStruct.ridgeCatLine.controlPointList.Add(midLControlPoint);
		newRidgeStruct.ridgeCatLine.controlPointList.Add(endControlPoint);

		newRidgeStruct.ridgeCatLine.SetLineNumberOfPoints(100);
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
	private RoofSurfaceStruct CreateRoofSurfaceA(RidgeStruct RightMainRidgeStruct, RidgeStruct LeftMainRidgeStruct, RidgeStruct eaveStruct)
	{

		//for (int i = 0; i < (int)MainController.Instance.sides; i++)

		RoofSurfaceStruct newRoofSurfaceStruct = CreateRoofSurfaceSturct("RoofSurface", this.gameObject);

		RidgeStruct newMidRidgeStruct = CreateRidgeSturct("MidRoofSurfaceTileRidge", newRoofSurfaceStruct.body);


		/***************************************找兩個mainRidge中間垂直的roofSurfaceTileRidge***********************************************************/
		//FindMidRoofSurfaceMidPoint
		Vector2 v1 = new Vector2(LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position.x - LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position.x, LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position.z - LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position.z);
		Vector2 v2 = new Vector2(RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position.x - RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position.x, RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position.z - RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position.z);

		float angle = Vector2.Angle(v1, v2);

		//midRoofSurfaceTopPoint
		Vector3 midRoofSurfaceTopPointPos = (RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position + LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position) / 2.0f;
		GameObject midRoofSurfaceTopPoint = CreateControlPoint(newMidRidgeStruct.body, midRoofSurfaceTopPointPos, MidRoofSurfaceControlPointType.MidRoofSurfaceTopPoint.ToString());
		newMidRidgeStruct.controlPointDictionaryList.Add(MidRoofSurfaceControlPointType.MidRoofSurfaceTopPoint.ToString(), midRoofSurfaceTopPoint);
		//midRoofSurfaceDownPoint
		Vector3 midRoofSurfaceDownPointPos = eaveStruct.controlPointDictionaryList[EaveControlPointType.MidControlPoint.ToString()].transform.position;
		GameObject midRoofSurfaceDownPoint = CreateControlPoint(newMidRidgeStruct.body, midRoofSurfaceDownPointPos, MidRoofSurfaceControlPointType.MidRoofSurfaceDownPoint.ToString());
		newMidRidgeStruct.controlPointDictionaryList.Add(MidRoofSurfaceControlPointType.MidRoofSurfaceDownPoint.ToString(), midRoofSurfaceDownPoint);
		//midRoofSurfaceMidPoint
		//Vector3 midRoofSurfaceMidPointPos = (Quaternion.Euler(0, angle / 2.0f, 0) * (RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position)) + (roofSurfaceHeightOffset * Vector3.up) - (RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position - LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position) / 2.0f;
		//Vector3 midRoofSurfaceMidPointPos = (RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position + LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position) / 2.0f + (roofSurfaceHeightOffset * Vector3.Cross((RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position - RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position), (LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position - RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position)).normalized);
	
		//Vector3 midRoofSurfaceMidPointPos = (midRoofSurfaceTopPointPos + midRoofSurfaceDownPointPos) / 2.0f;
		//midRoofSurfaceMidPointPos.y = (RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position.y + LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.MidControlPoint.ToString()].transform.position.y) / 2.0f;
		//midRoofSurfaceMidPointPos+=((roofSurfaceHeightOffset) * Vector3.Cross((RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position - RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position), (LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position - LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position)).normalized);
		Vector3 midRoofSurfaceMidPointPos = (midRoofSurfaceTopPointPos + midRoofSurfaceDownPointPos) / 2.0f +((roofSurfaceHeightOffset) * Vector3.Cross((RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position - RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position), (LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position - LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position)).normalized);
		GameObject midRoofSurfaceMidPoint = CreateControlPoint(newMidRidgeStruct.body, midRoofSurfaceMidPointPos, MidRoofSurfaceControlPointType.MidRoofSurfaceMidPoint.ToString());
		newMidRidgeStruct.controlPointDictionaryList.Add(MidRoofSurfaceControlPointType.MidRoofSurfaceMidPoint.ToString(), midRoofSurfaceMidPoint);

		//MidRoofSurfaceTileRidge
		newMidRidgeStruct.ridgeCatLine.controlPointList.Add(midRoofSurfaceTopPoint);
		newMidRidgeStruct.ridgeCatLine.controlPointList.Add(midRoofSurfaceMidPoint);
		newMidRidgeStruct.ridgeCatLine.controlPointList.Add(midRoofSurfaceDownPoint);
		newMidRidgeStruct.ridgeCatLine.SetLineNumberOfPoints(1000);
		newMidRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis);


		//ShowPos(midRoofSurfaceMidPointPos, newMidRidgeStruct.body, new Color(1, 0, 1), 1f);
		/***************************************用AnchorLength取MidRoofSurfaceTileRidge上的瓦片************************************************************/

		newMidRidgeStruct.tilePosList = newMidRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist, newMidRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1, 0, roofSurfaceTileHeight);
		//計算上仰傾斜角度=0
		CreateRoofSurfaceTile(ModelController.Instance.roofSurfaceModelStruct, newMidRidgeStruct.body, newMidRidgeStruct, newMidRidgeStruct, newMidRidgeStruct, eaveStruct, 0, RightMainRidgeStruct, midRoofSurfaceTopPointPos);
		MeshCombineInGameObjectList(newMidRidgeStruct.body, Resources.Load("Models/Materials/RoofMat") as Material);
		/***************************************由Eave上的AnchorPoint切割平面 用於切出其他垂直的roofSurfaceTileRidge(右側)***********************************************************/

		RidgeStruct eaveRightRidgeStruct = CreateRidgeSturct("EaveRightRidgeStruct", newRoofSurfaceStruct.body);
		for (int k = 0; k < eaveStruct.ridgeCatLine.anchorInnerPointlist.Count / 2+1; k++)
		{
			eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Add(eaveStruct.ridgeCatLine.anchorInnerPointlist[k]);
		}


		eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist = eaveRightRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist, eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1, 0, roofSurfaceTileWidth);

		//VerCutPlane
		Plane verticalCutPlane = new Plane();
		Plane verticalMirrorPlane = new Plane();
		Plane surfacePlane = new Plane();
		Vector3 verticalCutPlaneNormal = ((eaveStruct.controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position - eaveStruct.controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position)).normalized;
		verticalMirrorPlane.SetNormalAndPosition(verticalCutPlaneNormal, midRoofSurfaceDownPointPos);

		Vector3 surfacePlaneNormal = Vector3.Cross(RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position - RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position, midRoofSurfaceDownPointPos - RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.TopControlPoint.ToString()].transform.position).normalized;
		surfacePlane.SetNormalAndPosition(surfacePlaneNormal, midRoofSurfaceMidPointPos);
		//紀錄前一次Index用於迴圈加速
		int roofSurfaceMidPointStartingIndex_R = 0;
		int roofSurfaceMidPointStartingIndex_R_A = 0;
		int roofSurface2MainRidgeStartingIndex_R = 0;

		RidgeStruct lastRightRidgeStruct = newMidRidgeStruct;
		RidgeStruct lastLeftRidgeStruct = newMidRidgeStruct;


		CatLine roofSurfaceMidPointLine = new CatLine();
		//roofSurfaceMidPointLine.controlPointPosList.Add(LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position);
		roofSurfaceMidPointLine.controlPointPosList.Add(eaveStruct.controlPointDictionaryList[EaveControlPointType.MidLControlPoint.ToString()].transform.position);
		roofSurfaceMidPointLine.controlPointPosList.Add(midRoofSurfaceMidPointPos);
		//roofSurfaceMidPointLine.controlPointPosList.Add(RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position);
		roofSurfaceMidPointLine.controlPointPosList.Add(eaveStruct.controlPointDictionaryList[EaveControlPointType.MidRControlPoint.ToString()].transform.position);
		roofSurfaceMidPointLine.SetLineNumberOfPoints(1000);
		roofSurfaceMidPointLine.SetCatmullRom(anchorDis, 1);

		CatLine roofSurfaceMidPointLineA = new CatLine();

		roofSurfaceMidPointLineA.controlPointPosList.Add(LeftMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position);
		roofSurfaceMidPointLineA.controlPointPosList.Add(midRoofSurfaceMidPointPos);
		roofSurfaceMidPointLineA.controlPointPosList.Add(RightMainRidgeStruct.controlPointDictionaryList[MainRidgeControlPointType.DownControlPoint.ToString()].transform.position);
		roofSurfaceMidPointLineA.SetLineNumberOfPoints(1000);
		roofSurfaceMidPointLineA.SetCatmullRom(anchorDis, 1);

		for (int f = 0; f < roofSurfaceMidPointLine.anchorInnerPointlist.Count; f++)
		{
			//ShowPos(roofSurfaceMidPointLine.anchorInnerPointlist[f], newMidRidgeStruct.body, Color.green, 0.1f);
		}
		//Right&LeftRoofSurfaceTileRidgeList
		for (int n = 1; n < eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count; n++)
		{

			verticalCutPlane.SetNormalAndPosition(verticalCutPlaneNormal, eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n]);

			Vector3 planeOffsetVector = Vector3.Project((eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n] - eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[0]), (eaveStruct.controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position - eaveStruct.controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position));
			//Right
			RidgeStruct newRightRidgeStruct = CreateRidgeSturct("RightRoofSurfaceTileRidge", newRoofSurfaceStruct.body);

			RidgeStruct newLeftRidgeStruct = CreateRidgeSturct("LeftRoofSurfaceTileRidge", newRoofSurfaceStruct.body);


			Vector3 roofSurfaceTileRidgeUpPointPos = Vector3.zero;
			Vector3 roofSurfaceTileRidgeMidPointPos = Vector3.zero;
			Vector3 roofSurfaceTileRidgeDownPointPos = Vector3.zero;

			Vector3 lastRoofSurfaceTileRidgeUpPointPos = Vector3.zero;
			Vector3 lastRoofSurfaceTileRidgeMidPointPos = Vector3.zero;
			Vector3 lastRoofSurfaceTileRidgeDownPointPos = Vector3.zero;


			//FindPointOnMainRidgeCloser2Plane

			roofSurface2MainRidgeStartingIndex_R = FindNearestPointInList2Plane(verticalCutPlane, RightMainRidgeStruct.ridgeCatLine.anchorInnerPointlist, roofSurface2MainRidgeStartingIndex_R, RightMainRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1, 0.01f);

			roofSurfaceTileRidgeUpPointPos = (roofSurface2MainRidgeStartingIndex_R != 0) ? RightMainRidgeStruct.ridgeCatLine.anchorInnerPointlist[roofSurface2MainRidgeStartingIndex_R] : midRoofSurfaceTopPointPos + planeOffsetVector;

			//FindPointOnEaveCloser2Plane
			roofSurfaceTileRidgeDownPointPos = eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[n];


			//FindPointOnRoofSurfaceMidPointLineCloser2Plane
			roofSurfaceMidPointStartingIndex_R = FindNearestPointInList2Plane(verticalCutPlane, roofSurfaceMidPointLine.anchorInnerPointlist, roofSurfaceMidPointStartingIndex_R, roofSurfaceMidPointLine.anchorInnerPointlist.Count - 1);

			roofSurfaceMidPointStartingIndex_R_A = FindNearestPointInList2Plane(verticalCutPlane, roofSurfaceMidPointLineA.anchorInnerPointlist, roofSurfaceMidPointStartingIndex_R_A, roofSurfaceMidPointLineA.anchorInnerPointlist.Count - 1);
			float ratioA = ((float)(n) / ((eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1)));
			float ratioC = ((float)Mathf.Abs(n - eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count / 2) / ((eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1 - eaveRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count / 2)));
			roofSurfaceTileRidgeMidPointPos = (roofSurfaceMidPointLineA.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R_A]) * (ratioA) + (roofSurfaceMidPointLine.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R]) * (1.0f - ratioA);


			//roofSurfaceTileRidgeMidPointPos = roofSurfaceMidPointLine.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R] * (1.0f - ratioA) + ((roofSurfaceTileRidgeUpPointPos + roofSurfaceTileRidgeDownPointPos) / 2.0f) * ratioA;
			roofSurfaceTileRidgeMidPointPos.y = (roofSurfaceMidPointLine.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R].y * (1.0f - ratioA) + roofSurfaceMidPointLineA.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R_A].y * ratioA) * (1.0f - ratioA) + ((roofSurfaceTileRidgeUpPointPos.y + roofSurfaceTileRidgeDownPointPos.y) / 2.0f) * ratioA;
			//roofSurfaceTileRidgeMidPointPos = (roofSurfaceMidPointLine.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R] + surfacePlane.GetDistanceToPoint(roofSurfaceMidPointLine.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R]) * ratioC * surfacePlaneNormal * ((surfacePlane.GetSide(roofSurfaceMidPointLine.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R])) ? -1 : 1)) * (1.0f - ratioA) + (((roofSurfaceTileRidgeUpPointPos + roofSurfaceTileRidgeDownPointPos) / 2.0f) * ratioA + roofSurfaceMidPointLine.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R] * (1.0f - ratioA)) * (ratioA);
			//roofSurfaceTileRidgeMidPointPos = (roofSurfaceMidPointLine.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R]) * (1.0f - ratioA) + (((roofSurfaceTileRidgeUpPointPos + roofSurfaceTileRidgeDownPointPos) / 2.0f) * ratioA + roofSurfaceMidPointLine.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R] * (1.0f - ratioA)) * (ratioA);
			//roofSurfaceTileRidgeMidPointPos = (roofSurfaceMidPointLine.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R] * ratioC + roofSurfaceTileRidgeDownPointPos * (1.0f - ratioC)) * (1.0f - ratioA) + (((roofSurfaceTileRidgeUpPointPos + roofSurfaceTileRidgeDownPointPos) / 2.0f) * ratioA + roofSurfaceMidPointLine.anchorInnerPointlist[roofSurfaceMidPointStartingIndex_R] * (1.0f - ratioA)) * (ratioA);
			//roofSurfaceTileRidgeMidPointPos = roofSurfaceTileRidgeMidPointPos + surfacePlane.GetDistanceToPoint(roofSurfaceTileRidgeMidPointPos) * ratioA * surfacePlaneNormal * ((surfacePlane.GetSide(roofSurfaceTileRidgeMidPointPos))?-1:1);

			//ShowPos(roofSurfaceTileRidgeUpPointPos, newLeftRidgeStruct.body, Color.white, 0.8f);

			//ShowPos(roofSurfaceTileRidgeMidPointPos, newLeftRidgeStruct.body, Color.white, 0.8f);

			//ShowPos(roofSurfaceTileRidgeDownPointPos, newLeftRidgeStruct.body, Color.white, 0.8f);

			newRightRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeUpPointPos);
			newRightRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeMidPointPos);
			newRightRidgeStruct.ridgeCatLine.controlPointPosList.Add(roofSurfaceTileRidgeDownPointPos);
			newRightRidgeStruct.ridgeCatLine.SetLineNumberOfPoints((int)(1000 * (1 - ratioA)));
			newRightRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis, 1);

			newLeftRidgeStruct.ridgeCatLine.controlPointPosList.Add(Mathf.Abs(2 * verticalMirrorPlane.GetDistanceToPoint(roofSurfaceTileRidgeUpPointPos)) * -verticalCutPlaneNormal + roofSurfaceTileRidgeUpPointPos);
			newLeftRidgeStruct.ridgeCatLine.controlPointPosList.Add(Mathf.Abs(2 * verticalMirrorPlane.GetDistanceToPoint(roofSurfaceTileRidgeMidPointPos)) * -verticalCutPlaneNormal + roofSurfaceTileRidgeMidPointPos);
			newLeftRidgeStruct.ridgeCatLine.controlPointPosList.Add(Mathf.Abs(2 * verticalMirrorPlane.GetDistanceToPoint(roofSurfaceTileRidgeDownPointPos)) * -verticalCutPlaneNormal + roofSurfaceTileRidgeDownPointPos);
			newLeftRidgeStruct.ridgeCatLine.SetLineNumberOfPoints((int)(1000 * (1 - ratioA)));
			newLeftRidgeStruct.ridgeCatLine.SetCatmullRom(anchorDis, 1);

			/***************************************用AnchorLength取roofSurfaceTileRidge上的瓦片************************************************************/
			/*

						for (int f = 0; f < newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count; f++)
						{
							ShowPos(newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist[f], newRightRidgeStruct.body, Color.green, 0.1f);
						}
			*/

			newRightRidgeStruct.tilePosList = newRightRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist, newRightRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1, 0, roofSurfaceTileHeight);

			newLeftRidgeStruct.tilePosList = newLeftRidgeStruct.ridgeCatLine.CalculateAnchorPosByInnerPointList(newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist, newLeftRidgeStruct.ridgeCatLine.anchorInnerPointlist.Count - 1, 0, roofSurfaceTileHeight);

			/*	for (int f = 0; f < newLeftRidgeStruct.tilePosList.Count; f++)
				{
					ShowPos(newLeftRidgeStruct.tilePosList[f], newLeftRidgeStruct.body, Color.blue, 0.3f);
				}
				for (int f = 0; f < newRightRidgeStruct.tilePosList.Count; f++)
				{
					ShowPos(newRightRidgeStruct.tilePosList[f], newRightRidgeStruct.body, Color.blue, 0.3f);
				}*/
			//Debug.Log(lastRightRidgeStruct.body.name + "   " + lastRightRidgeStruct.tilePosList.Count);




			CreateRoofSurfaceTile(ModelController.Instance.roofSurfaceModelStruct, newRightRidgeStruct.body, newRightRidgeStruct, lastRightRidgeStruct, newMidRidgeStruct, eaveStruct, 1, RightMainRidgeStruct, roofSurfaceTileRidgeUpPointPos);
			CreateRoofSurfaceTile(ModelController.Instance.roofSurfaceModelStruct, newLeftRidgeStruct.body, newLeftRidgeStruct, lastLeftRidgeStruct, newMidRidgeStruct, eaveStruct, -1, LeftMainRidgeStruct, Vector3.Reflect(roofSurfaceTileRidgeUpPointPos, (eaveStruct.controlPointDictionaryList[EaveControlPointType.EndControlPoint.ToString()].transform.position - eaveStruct.controlPointDictionaryList[EaveControlPointType.StartControlPoint.ToString()].transform.position).normalized));
			// 			Debug.Log("size" + size);
			// 			Debug.Log(" newRightRidgeStruct.tilePosList.Count" + newRightRidgeStruct.tilePosList.Count);
			// 			newLeftRidgeStruct.tilePosList.RemoveRange(size, newLeftRidgeStruct.tilePosList.Count - size);
			// 			newRightRidgeStruct.tilePosList.RemoveRange(size, newRightRidgeStruct.tilePosList.Count - size);
			// 			Debug.Log(" newRightRidgeStruct.tilePosList.Count" + newRightRidgeStruct.tilePosList.Count);

			//AdvancedMerge(newRightRidgeStruct.body);
			MeshCombineInGameObjectList(newRightRidgeStruct.body,Resources.Load("Models/Materials/RoofMat") as Material);
			MeshCombineInGameObjectList(newLeftRidgeStruct.body, Resources.Load("Models/Materials/RoofMat") as Material);



			lastRoofSurfaceTileRidgeUpPointPos = roofSurfaceTileRidgeUpPointPos;
			lastRoofSurfaceTileRidgeMidPointPos = roofSurfaceTileRidgeMidPointPos;
			lastRoofSurfaceTileRidgeDownPointPos = roofSurfaceTileRidgeDownPointPos;
			lastRightRidgeStruct = newRightRidgeStruct;
			lastLeftRidgeStruct = newLeftRidgeStruct;

			newRoofSurfaceStruct.rightRoofSurfaceTileRidgeList.Add(newRightRidgeStruct);
			newRoofSurfaceStruct.leftRoofSurfaceTileRidgeList.Add(newLeftRidgeStruct);
		}
		newRoofSurfaceStruct.midRoofSurfaceTileRidge = newMidRidgeStruct;

		return newRoofSurfaceStruct;
	}
	void CopyRoofFunction(GameObject cloneObject, float angle, Vector3 rotationCenter, int times, Vector3 offsetVector)
	{
		for (int i = 1; i < times; i++)
		{
			GameObject clone = Instantiate(cloneObject, cloneObject.transform.position, cloneObject.transform.rotation) as GameObject;
			clone.transform.RotateAround(rotationCenter, Vector3.up, angle * i);
			clone.transform.position += offsetVector;
			clone.transform.parent = this.transform;
		}

	}
	public void CreateRoof( PlatformController platformController, BodyController bodyController)
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

		int ColumnIndex_Zero = (bodyController.eaveColumnbayNumber > 0) ? 0 * bodyController.eaveColumnbayNumber : 0;
		int ColumnIndex_One = (bodyController.eaveColumnbayNumber > 0) ? 1 * bodyController.eaveColumnbayNumber : 1;
		int ColumnIndex_Two = (bodyController.eaveColumnbayNumber > 0) ? 2 * bodyController.eaveColumnbayNumber : 2;

		switch (roofType)
		{
			//攢尖頂
			case RoofType.Zan_Jian_Ding:
				#region  Zan_Jian_Ding

				if (MainController.Instance.sides == MainController.FormFactorSideType.FourSide)
				{
					//主脊-MainRidge輔助線 
					RightMainRidgeStruct = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_One, roofTopCenter);
					LeftMainRidgeStruct = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_Zero, roofTopCenter);

					//Eave輔助線
					eaveStruct = CreateEaveStruct(RightMainRidgeStruct, LeftMainRidgeStruct);//檐出

					//RoofSurface
					roofSurfaceStructList = CreateRoofSurfaceA(RightMainRidgeStruct, LeftMainRidgeStruct, eaveStruct);//屋面

					CreateMainRidgeTile(ModelController.Instance.mainRidgeModelStruct, RightMainRidgeStruct, roofSurfaceStructList, eaveStruct, RightMainRidgeStruct.body);
					//主脊-MainRidge輔助線 
					RightMainRidgeStructA = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_Two, roofTopCenter);
					LeftMainRidgeStructA = RightMainRidgeStruct;
					//Eave輔助線
					eaveStructA = CreateEaveStruct(RightMainRidgeStructA, LeftMainRidgeStructA);//檐出

					//RoofSurface
					roofSurfaceStructListA = CreateRoofSurfaceA(RightMainRidgeStructA, LeftMainRidgeStructA, eaveStructA);//屋面

					CreateMainRidgeTile(ModelController.Instance.mainRidgeModelStruct, RightMainRidgeStructA, roofSurfaceStructListA, eaveStructA, RightMainRidgeStructA.body);

					//複製出其他屋面
					CopyRoofFunction(this.gameObject, 180, roofTopCenter, 2, roofTopCenter - roofTopCenter);
				}
				else
				{
					//主脊-MainRidge輔助線 
					RightMainRidgeStruct = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_One, roofTopCenter);
					LeftMainRidgeStruct = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_Zero, roofTopCenter);

					//Eave輔助線
					eaveStruct = CreateEaveStruct(RightMainRidgeStruct, LeftMainRidgeStruct);//檐出

					//RoofSurface
					roofSurfaceStructList = CreateRoofSurfaceA(RightMainRidgeStruct, LeftMainRidgeStruct, eaveStruct);//屋面


					CreateMainRidgeTile(ModelController.Instance.mainRidgeModelStruct, RightMainRidgeStruct, roofSurfaceStructList, eaveStruct, RightMainRidgeStruct.body);

					//複製出其他屋面
					CopyRoofFunction(this.gameObject, 360.0f / (int)MainController.Instance.sides, roofTopCenter, (int)MainController.Instance.sides, roofTopCenter - roofTopCenter);

				}
				#endregion
				break;
			case RoofType.Wu_Dian_Ding:
				#region  Wu_Dian_Ding

				if (MainController.Instance.sides == MainController.FormFactorSideType.FourSide)
				{
					offsetVector = (bodyController.eaveColumnList[ColumnIndex_One].topPos - bodyController.eaveColumnList[1].topPos).normalized * Wu_Dian_DingMainRidgeWidth * 0.5f;
				Vector3 rightRoofTopCenter = roofTopCenter + offsetVector;
				Vector3 leftRoofTopCenter = roofTopCenter - offsetVector;

				//主脊-MainRidge輔助線 
				RightMainRidgeStruct = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_One, rightRoofTopCenter);
				LeftMainRidgeStruct = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_Zero, leftRoofTopCenter);
				//Eave輔助線
				eaveStruct = CreateEaveStruct(RightMainRidgeStruct, LeftMainRidgeStruct);//檐出
				//RoofSurface
				roofSurfaceStructList = CreateRoofSurfaceA(RightMainRidgeStruct, LeftMainRidgeStruct, eaveStruct);//屋面

				CreateMainRidgeTile(ModelController.Instance.mainRidgeModelStruct, RightMainRidgeStruct, roofSurfaceStructList, eaveStruct, RightMainRidgeStruct.body);

				Vector2 v1 = new Vector2(rightRoofTopCenter.x - bodyController.eaveColumnList[ColumnIndex_One].topPos.x, rightRoofTopCenter.z - bodyController.eaveColumnList[ColumnIndex_One].topPos.z);
				Vector2 v2 = new Vector2(leftRoofTopCenter.x - bodyController.eaveColumnList[ColumnIndex_Zero].topPos.x, leftRoofTopCenter.z - bodyController.eaveColumnList[ColumnIndex_Zero].topPos.z);

				float angle = Vector2.Angle(v1, v2);


				CopyRoofFunction(RightMainRidgeStruct.body, angle, rightRoofTopCenter, 2, leftRoofTopCenter - rightRoofTopCenter);


				//主脊-MainRidge輔助線 
				RightMainRidgeStructA = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_Two, rightRoofTopCenter);
				LeftMainRidgeStructA = RightMainRidgeStruct;
				//Eave輔助線
				eaveStructA = CreateEaveStruct(RightMainRidgeStructA, LeftMainRidgeStructA);//檐出

				//RoofSurface
				roofSurfaceStructListA = CreateRoofSurfaceA(RightMainRidgeStructA, LeftMainRidgeStructA, eaveStructA);//屋面


				CreateMainRidgeTile(ModelController.Instance.mainRidgeModelStruct, RightMainRidgeStructA, roofSurfaceStructListA, eaveStructA, RightMainRidgeStructA.body);


				//複製出其他屋面
				CopyRoofFunction(this.gameObject, 180, roofTopCenter, 2, roofTopCenter - roofTopCenter);
				}
				#endregion
				break;

			case RoofType.Lu_Ding:
				#region  Lu_Ding

				if (MainController.Instance.sides == MainController.FormFactorSideType.FourSide) 
				{
					//主脊-MainRidge輔助線 
					Lu_DingMainRidgeOffset = Vector3.Distance(bodyController.eaveColumnList[ColumnIndex_One].topPos, bodyController.eaveColumnList[ColumnIndex_Zero].topPos) * 0.5f;
					offsetVector = (new Vector3(bodyController.eaveColumnList[ColumnIndex_One].topPos.x - roofTopCenter.x, 0, bodyController.eaveColumnList[ColumnIndex_One].topPos.z - roofTopCenter.z)).normalized * Lu_DingMainRidgeOffset;
					RightMainRidgeStruct = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_One, roofTopCenter + offsetVector);

					offsetVector = (new Vector3(bodyController.eaveColumnList[ColumnIndex_Zero].topPos.x - roofTopCenter.x, 0, bodyController.eaveColumnList[ColumnIndex_Zero].topPos.z - roofTopCenter.z)).normalized * Lu_DingMainRidgeOffset;
					LeftMainRidgeStruct = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_Zero, roofTopCenter + offsetVector);
					//Eave輔助線
					eaveStruct = CreateEaveStruct(RightMainRidgeStruct, LeftMainRidgeStruct);//檐出

					//RoofSurface
					roofSurfaceStructList = CreateRoofSurfaceA(RightMainRidgeStruct, LeftMainRidgeStruct, eaveStruct);//屋面

					CreateMainRidgeTile(ModelController.Instance.mainRidgeModelStruct, RightMainRidgeStruct, roofSurfaceStructList, eaveStruct, RightMainRidgeStruct.body);
					//主脊-MainRidge輔助線 
					LeftMainRidgeStructA = RightMainRidgeStruct;
					offsetVector = (new Vector3(bodyController.eaveColumnList[ColumnIndex_Two].topPos.x - roofTopCenter.x, 0, bodyController.eaveColumnList[ColumnIndex_Two].topPos.z - roofTopCenter.z)).normalized * Lu_DingMainRidgeOffset;
					RightMainRidgeStructA = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_Two, roofTopCenter + offsetVector);
					//Eave輔助線
					eaveStructA = CreateEaveStruct(RightMainRidgeStructA, LeftMainRidgeStructA);//檐出

					//RoofSurface
					roofSurfaceStructListA = CreateRoofSurfaceA(RightMainRidgeStructA, LeftMainRidgeStructA, eaveStructA);//屋面

					CreateMainRidgeTile(ModelController.Instance.mainRidgeModelStruct, RightMainRidgeStructA, roofSurfaceStructListA, eaveStructA, RightMainRidgeStructA.body);
					//複製出其他屋面
					CopyRoofFunction(this.gameObject, 180.0f, roofTopCenter, 2, roofTopCenter - roofTopCenter);

	
				}
				else
				{
				//主脊-MainRidge輔助線
					Lu_DingMainRidgeOffset = Vector3.Distance(bodyController.eaveColumnList[ColumnIndex_One].topPos, bodyController.eaveColumnList[ColumnIndex_Zero].topPos) * 0.5f;
					offsetVector = (new Vector3(bodyController.eaveColumnList[ColumnIndex_One].topPos.x - roofTopCenter.x, 0, bodyController.eaveColumnList[ColumnIndex_One].topPos.z - roofTopCenter.z)).normalized * Lu_DingMainRidgeOffset;
					RightMainRidgeStruct = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_One, roofTopCenter + offsetVector);

					offsetVector = (new Vector3(bodyController.eaveColumnList[ColumnIndex_Zero].topPos.x - roofTopCenter.x, 0, bodyController.eaveColumnList[ColumnIndex_Zero].topPos.z - roofTopCenter.z)).normalized * Lu_DingMainRidgeOffset;
					LeftMainRidgeStruct = CreateMainRidgeStruct(bodyController.eaveColumnList, ColumnIndex_Zero, roofTopCenter + offsetVector);
				//Eave輔助線
				eaveStruct = CreateEaveStruct(RightMainRidgeStruct, LeftMainRidgeStruct);//檐出

				//RoofSurface
				roofSurfaceStructList = CreateRoofSurfaceA(RightMainRidgeStruct, LeftMainRidgeStruct, eaveStruct);//屋面

				CreateMainRidgeTile(ModelController.Instance.mainRidgeModelStruct, RightMainRidgeStruct, roofSurfaceStructList, eaveStruct, RightMainRidgeStruct.body);
				
				//複製出其他屋面
				CopyRoofFunction(this.gameObject, 360.0f / ((int)MainController.Instance.sides), roofTopCenter, (int)MainController.Instance.sides, roofTopCenter - roofTopCenter);
				}
				#endregion
				break;
		}
		

	}
}

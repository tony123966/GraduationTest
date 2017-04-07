using UnityEngine;
using System.Collections;
using System.Collections.Generic;
struct EaveColumnModelStruct
{
	public ModelStruct friezeModelStruct;
	public ModelStruct balustradeModelStruct;

	public EaveColumnModelStruct(ModelStruct friezeModelStruct, ModelStruct balustradeModelStruct)
	{
		this.friezeModelStruct = friezeModelStruct;
		this.balustradeModelStruct = balustradeModelStruct;
	}
}
struct GoldColumnModelStruct
{
	public ModelStruct windowModelStruct;

	public GoldColumnModelStruct(ModelStruct windowModelStruct)
	{
		this.windowModelStruct = windowModelStruct;
	}
}
public class BodyController : Singleton<BodyController>
{
	public GameObject body = null;
	//Body******************************************************************************
	const float CUN = 3.33f;
	public enum BodyType { Chuan_Dou = 0, Tai_Liang = 1 };//Chuan_Dou 穿斗式 ,Tai_Liang 抬梁式

	public BodyType bodyType = BodyType.Chuan_Dou;

	public float eaveColumnRatio2platformOffset;

	public float goldColumnRatio2platformOffset;

	public int goldColumnbayNumber = 3;//間數量
	public int eaveColumnbayNumber = 5;
	public float eaveColumnHeight;

	public float eaveColumnTopRadius = 1f;
	public float eaveColumnDownRadius = 1f;
	public Vector3 bodyCenter;

	//**********************************************************************************
	public float friezeWidth = 0.6f;//裝飾物長度
	public float balustradeWidth = 0.6f;//欄杆長度
	//**********************************************************************************
	EaveColumnModelStruct eaveColumnModelStruct;
	public GameObject friezeModel;
	public Vector3 friezeModelRotation = Vector3.zero;
	public Vector3 friezeModelScale = Vector3.one;
	private ModelStruct friezeModelStruct;
	public GameObject balustradeModel;
	public Vector3 balustradeModelRotation = Vector3.zero;
	public Vector3 balustradeModelScale = Vector3.one;
	private ModelStruct balustradeModelStruct;

	GoldColumnModelStruct goldColumnModelStruct;
	public GameObject windowModel;
	public Vector3 windowModelRotation = Vector3.zero;
	public Vector3 windowModelScale = Vector3.one;
	private ModelStruct windowModelStruct;
	//***********************************************************************
	public List<CylinderMesh> eaveColumnList = new List<CylinderMesh>();
	public List<CylinderMesh> goldColumnList = new List<CylinderMesh>();
	//***********************************************************************

	public void InitFunction()
	{
		//初始值******************************************************************************

		eaveColumnHeight = eaveColumnDownRadius * 11;

		eaveColumnRatio2platformOffset = (PlatformController.Instance.platformFrontWidth * 0.1f);
		goldColumnRatio2platformOffset = eaveColumnRatio2platformOffset * 2.5f;

		bodyCenter = PlatformController.Instance.platformCenter + new Vector3(0, PlatformController.Instance.platformHeight / 2.0f + eaveColumnHeight / 2.0f, 0);


		friezeModelStruct = new ModelStruct(friezeModel, friezeModelRotation, friezeModelScale);
		balustradeModelStruct = new ModelStruct(balustradeModel, balustradeModelRotation, balustradeModelScale);
		eaveColumnModelStruct = new EaveColumnModelStruct(friezeModelStruct, balustradeModelStruct);

		windowModelStruct = new ModelStruct(windowModel, windowModelRotation, windowModelScale);
		goldColumnModelStruct = new GoldColumnModelStruct(windowModelStruct);

		//************************************************************************************
		body = new GameObject("Body");
		body.transform.position = bodyCenter;
		body.transform.parent = MainController.Instance.building.transform;

		switch (bodyType)
		{
			#region Chuan_Dou
			case BodyType.Chuan_Dou:
				CreateRingColumn();
				CreateWall();
				CreateFrieze();
				CreateBalustrade();
				break;
			#endregion
		}
	}
	public void UpdateFunction()
	{

	}
	private CylinderMesh CreateColumn(Vector3 pos, float topRadius, float downRadius, float height, string name = "Column")
	{
		GameObject col = new GameObject(name);
		col.transform.position = pos;
		col.transform.parent = body.transform;
		col.AddComponent<CylinderMesh>();

		Vector3 topPos = pos + new Vector3(0, height / 2.0f, 0);
		Vector3 bottomPos = pos - new Vector3(0, height / 2.0f, 0);

		col.GetComponent<CylinderMesh>().CylinderInitSetting(pos, topPos, bottomPos, topRadius, downRadius);
		col.GetComponent<CylinderMesh>().SetMesh();
		return col.GetComponent<CylinderMesh>();
	}
	public void CreateRingColumn()
	{
		eaveColumnList.Clear();
		goldColumnList.Clear();
		for (int i = 0; i < PlatformController.Instance.topPointPosList.Count; i++)
		{
			Vector2 v = new Vector2(PlatformController.Instance.topPointPosList[i].x - PlatformController.Instance.platformCenter.x, PlatformController.Instance.topPointPosList[i].z - PlatformController.Instance.platformCenter.z);
			//eaveColumn
			v = v.normalized * eaveColumnRatio2platformOffset;
			Vector3 pos = PlatformController.Instance.topPointPosList[i] - new Vector3(v.x, 0, v.y) + new Vector3(0, eaveColumnHeight / 2.0f, 0);
			CylinderMesh newColumn = CreateColumn(pos, eaveColumnTopRadius, eaveColumnDownRadius, eaveColumnHeight, "EaveColumn");
			eaveColumnList.Add(newColumn);

			//eaveColumn_Bay
			int nextIndex = (i + 1) % PlatformController.Instance.topPointPosList.Count;
			Vector2 vNext = new Vector2(PlatformController.Instance.topPointPosList[nextIndex].x - PlatformController.Instance.platformCenter.x, PlatformController.Instance.topPointPosList[nextIndex].z - PlatformController.Instance.platformCenter.z);
			vNext = vNext.normalized * eaveColumnRatio2platformOffset;
			Vector3 posNext = PlatformController.Instance.topPointPosList[nextIndex] - new Vector3(vNext.x, 0, vNext.y) + new Vector3(0, eaveColumnHeight / 2.0f, 0);

			float disBetweenEaveColumn = Vector3.Distance(pos, posNext);
			float bayWidth = disBetweenEaveColumn / eaveColumnbayNumber;
			Vector3 bayDir = posNext - pos;

			for (int j = 1; j < eaveColumnbayNumber; j++)
			{
				Vector3 posT = bayDir.normalized * (j * bayWidth) + pos;
				CylinderMesh newColumnT = CreateColumn(posT, eaveColumnTopRadius, eaveColumnDownRadius, eaveColumnHeight, "EaveBayColumn");
				eaveColumnList.Add(newColumnT);
			}


			//goldColumn
			v = new Vector2(PlatformController.Instance.topPointPosList[i].x - PlatformController.Instance.platformCenter.x, PlatformController.Instance.topPointPosList[i].z - PlatformController.Instance.platformCenter.z);
			v = v.normalized * goldColumnRatio2platformOffset;
			Vector3 posZ = PlatformController.Instance.topPointPosList[i] - new Vector3(v.x, 0, v.y) + new Vector3(0, eaveColumnHeight / 2.0f, 0);
			CylinderMesh newColumnZ = CreateColumn(posZ, eaveColumnTopRadius, eaveColumnDownRadius, eaveColumnHeight, "GoldColumn");
			goldColumnList.Add(newColumnZ);
		}


	}
	public void CreateWall()
	{
		for (int i = 0; i < goldColumnList.Count; i++)
		{
			float dis = Vector3.Distance(goldColumnList[i].pos, goldColumnList[(i + 1) % goldColumnList.Count].pos) - eaveColumnDownRadius * 2;
			float width = dis / goldColumnbayNumber;
			Vector3 dir = goldColumnList[(i + 1) % goldColumnList.Count].pos - goldColumnList[i].pos;
			for (int j = 0; j < goldColumnbayNumber; j++)
			{
				GameObject wall = new GameObject("Wall");
				MeshFilter meshFilter = wall.AddComponent<MeshFilter>();
				MeshRenderer meshRenderer = wall.AddComponent<MeshRenderer>();
				wall.transform.parent = body.transform;
				meshRenderer.material.color = Color.white;
				float rotateAngle = (Vector3.Dot(Vector3.forward, dir) < 0 ? Vector3.Angle(dir, Vector3.right) : 180 - Vector3.Angle(dir, Vector3.right));
				Vector3 pos = dir.normalized * (width / 2.0f + j * width + eaveColumnDownRadius) + goldColumnList[i].pos;
				MainController.Instance.CreateTorusMesh(pos, width, eaveColumnHeight, 1, 0.3f, 0.3f, 0.6f, 1.0f, rotateAngle, meshFilter);
			}

			//CreateWindowModel




		}
	}
	public void CreateFrieze()
	{
		float heightOffset = 0.1f*eaveColumnHeight;

		for (int i = 0; i < eaveColumnList.Count; i++)
		{
			float width = friezeWidth;
			float dis = Vector3.Distance(eaveColumnList[i].pos, eaveColumnList[(i + 1) % eaveColumnList.Count].pos) - eaveColumnDownRadius * 2;
			float number = Mathf.FloorToInt(dis / width);
			Vector3 dir = eaveColumnList[(i + 1) % eaveColumnList.Count].pos - eaveColumnList[i].pos;
			float disDiff = (dis - width * number) / number;
			width = dis / number;

			float rotateAngle = (Vector3.Dot(Vector3.right, dir) > 0 ? Vector3.Angle(dir, Vector3.forward) : 180 - Vector3.Angle(dir, Vector3.forward));
			Debug.Log("rotateAngle" + rotateAngle);
			for (int j = 0; j < number; j++)
			{
				Vector3 pos = dir.normalized * (width / 2.0f + j * width + eaveColumnDownRadius) + eaveColumnList[i].bottomPos + heightOffset * Vector3.up;
				GameObject clone = Instantiate(eaveColumnModelStruct.friezeModelStruct.model, pos, eaveColumnModelStruct.friezeModelStruct.model.transform.rotation) as GameObject;
				clone.transform.rotation = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Quaternion.Euler(eaveColumnModelStruct.friezeModelStruct.rotation);
				clone.transform.GetChild(0).localScale = eaveColumnModelStruct.friezeModelStruct.scale;
				clone.transform.parent = body.transform;
			}
		}
	}
	public void CreateBalustrade()
	{
		float heightOffset = 0.9f * eaveColumnHeight;

		for (int i = 0; i < eaveColumnList.Count; i++)
		{
			float width = balustradeWidth;
			float dis = Vector3.Distance(eaveColumnList[i].pos, eaveColumnList[(i + 1) % eaveColumnList.Count].pos) - eaveColumnDownRadius * 2;
			float number = Mathf.FloorToInt(dis / width);
			Vector3 dir = eaveColumnList[(i + 1) % eaveColumnList.Count].pos - eaveColumnList[i].pos;
			float disDiff = (dis - width * number) / number;
			width = dis / number;

			float rotateAngle = (Vector3.Dot(Vector3.right, dir) > 0 ? Vector3.Angle(dir, Vector3.forward) : 180-Vector3.Angle(dir, Vector3.forward));
			for (int j = 0; j < number; j++)
			{
				Vector3 pos = dir.normalized * (width / 2.0f + j * width + eaveColumnDownRadius) + eaveColumnList[i].bottomPos + heightOffset * Vector3.up;
				GameObject clone = Instantiate(eaveColumnModelStruct.balustradeModelStruct.model, pos, eaveColumnModelStruct.balustradeModelStruct.model.transform.rotation) as GameObject;
				clone.transform.rotation = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Quaternion.Euler(eaveColumnModelStruct.balustradeModelStruct.rotation);
				clone.transform.GetChild(0).localScale = eaveColumnModelStruct.balustradeModelStruct.scale;
				clone.transform.parent = body.transform;
			}
		}
	}
}

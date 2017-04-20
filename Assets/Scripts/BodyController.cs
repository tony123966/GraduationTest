using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public struct EaveColumnModelStruct
{
	public ModelStruct friezeModelStruct;
	public ModelStruct balustradeModelStruct;

	public EaveColumnModelStruct(ModelStruct friezeModelStruct, ModelStruct balustradeModelStruct)
	{
		this.friezeModelStruct = friezeModelStruct;
		this.balustradeModelStruct = balustradeModelStruct;
	}
}
public struct GoldColumnModelStruct
{
	public ModelStruct windowModelStruct;

	public GoldColumnModelStruct(ModelStruct windowModelStruct)
	{
		this.windowModelStruct = windowModelStruct;
	}
}
public class BodyController:MonoBehaviour
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
	public float columnFundationHeight;//柱礎高度

	public float eaveColumnTopRadius = 1f;
	public float eaveColumnDownRadius = 1f;
	public float columnFundationRadius;
	
	public Vector3 bodyCenter;


	//***********************************************************************
	[HideInInspector]
	public List<CylinderMesh> eaveColumnList = new List<CylinderMesh>();
	[HideInInspector]
	public List<CylinderMesh> goldColumnList = new List<CylinderMesh>();
	//***********************************************************************

	public void InitFunction(GameObject parent,PlatformController platformController)
	{
		//初始值******************************************************************************

		eaveColumnHeight = eaveColumnDownRadius * 11;
		columnFundationHeight = eaveColumnHeight * 0.05f;

		columnFundationRadius=eaveColumnTopRadius*1.2f;

		eaveColumnRatio2platformOffset = (platformController.platformFrontWidth * 0.1f);
		goldColumnRatio2platformOffset = eaveColumnRatio2platformOffset * 2.5f;

		bodyCenter = platformController.platformCenter + new Vector3(0, platformController.platformHeight / 2.0f + eaveColumnHeight / 2.0f, 0);

		//************************************************************************************
		body = new GameObject("Body");
		body.transform.position = bodyCenter;
		body.transform.parent = parent.transform;

		//**************************************************************************************
		switch (bodyType)
		{
			#region Chuan_Dou
			case BodyType.Chuan_Dou:
				CreateRingColumn(platformController.topPointPosList, platformController.platformCenter);
				//base on RingColumn
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
	private CylinderMesh CreateColumn(Vector3 pos, float topRadius, float downRadius,float height, string name = "Column")
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
	public Vector3 mutiplyVector3(Vector3 a,Vector3 b)
	{

		return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
	}
	public void CreateRingColumn(List<Vector3> posList, Vector3 center)
	{
		eaveColumnList.Clear();
		goldColumnList.Clear();

		float columnRemainHeight = eaveColumnHeight - columnFundationHeight;

		for (int i = 0; i < posList.Count; i++)
		{
			Vector2 v = new Vector2(posList[i].x - center.x, posList[i].z - center.z);
			//eaveColumn
			v = v.normalized * eaveColumnRatio2platformOffset;
			Vector3 eaveColumnPos = posList[i] - new Vector3(v.x, 0, v.y) + new Vector3(0, columnRemainHeight / 2.0f + columnFundationHeight, 0);
			CylinderMesh newColumn = CreateColumn(eaveColumnPos, eaveColumnTopRadius, eaveColumnDownRadius, columnRemainHeight, "EaveColumn");
			eaveColumnList.Add(newColumn);

			//eaveColumnFundation
			Vector3 columnFundationPos = eaveColumnPos - new Vector3(0, eaveColumnHeight / 2.0f, 0);
			CreateColumn(columnFundationPos, columnFundationRadius, columnFundationRadius, columnFundationHeight, "EaveColumnFundation");

			//eaveBayColumn
			int nextIndex = (i + 1) % posList.Count;
			Vector2 vNext = new Vector2(posList[nextIndex].x - center.x, posList[nextIndex].z - center.z);
			vNext = vNext.normalized * eaveColumnRatio2platformOffset;
			Vector3 posNext = posList[nextIndex] - new Vector3(vNext.x, 0, vNext.y) + new Vector3(0, columnRemainHeight / 2.0f + columnFundationHeight, 0);

			float disBetweenEaveColumn = Vector3.Distance(eaveColumnPos, posNext);
			float bayWidth = disBetweenEaveColumn / eaveColumnbayNumber;
			Vector3 bayDir = posNext - eaveColumnPos;

			for (int j = 1; j < eaveColumnbayNumber; j++)
			{
				Vector3 eaveBayColumnPos = bayDir.normalized * (j * bayWidth) + eaveColumnPos;
				CylinderMesh newColumnT = CreateColumn(eaveBayColumnPos, eaveColumnTopRadius, eaveColumnDownRadius, columnRemainHeight, "EaveBayColumn");
				eaveColumnList.Add(newColumnT);

				//eaveBayColumnFundation
				columnFundationPos = eaveBayColumnPos - new Vector3(0, eaveColumnHeight/ 2.0f, 0);
				CreateColumn(columnFundationPos, columnFundationRadius, columnFundationRadius, columnFundationHeight, "EaveBayColumnFundation");
			}


			//goldColumn
			v = new Vector2(posList[i].x - center.x, posList[i].z - center.z);
			v = v.normalized * goldColumnRatio2platformOffset;
			Vector3 goldColumnPos = posList[i] - new Vector3(v.x, 0, v.y) + new Vector3(0, columnRemainHeight / 2.0f + columnFundationHeight, 0);
			CylinderMesh newColumnZ = CreateColumn(goldColumnPos, eaveColumnTopRadius, eaveColumnDownRadius, columnRemainHeight, "GoldColumn");
			goldColumnList.Add(newColumnZ);
			//goldColumnFundation
			columnFundationPos = goldColumnPos - new Vector3(0, eaveColumnHeight / 2.0f, 0);
			CreateColumn(columnFundationPos, columnFundationRadius, columnFundationRadius, columnFundationHeight, "GoldColumnFundation");

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
			
				Vector3 pos = dir.normalized * (width / 2.0f + j * width + eaveColumnDownRadius) + goldColumnList[i].pos-columnFundationHeight*Vector3.up;
				MainController.Instance.CreateWallMesh(pos, width, eaveColumnHeight, 1, 0.3f, 0.3f, 0.6f, 1.0f, rotateAngle, meshFilter);
			}

			//CreateWindowModel




		}
	}
	public void CreateFrieze()
	{
		float heightOffset = 0.1f*eaveColumnHeight;

		for (int i = 0; i < eaveColumnList.Count; i++)
		{
			float width = MainController.Instance.friezeWidth;
			float dis = Vector3.Distance(eaveColumnList[i].pos, eaveColumnList[(i + 1) % eaveColumnList.Count].pos) - eaveColumnDownRadius * 2;
			float number = Mathf.FloorToInt(dis / width);
			Vector3 dir = eaveColumnList[(i + 1) % eaveColumnList.Count].pos - eaveColumnList[i].pos;
			float disDiff = (dis - width * number) / number;
			width = dis / number;
			float rotateAngle = (Vector3.Dot(Vector3.right, dir) > 0 ? Vector3.Angle(dir, Vector3.forward) : 180 - Vector3.Angle(dir, Vector3.forward));
			for (int j = 0; j < number; j++)
			{
				Vector3 pos = dir.normalized * (width / 2.0f + j * width + eaveColumnDownRadius) + eaveColumnList[i].bottomPos + heightOffset * Vector3.up - columnFundationHeight * Vector3.up;
				GameObject clone = Instantiate(ModelController.Instance.eaveColumnModelStruct.friezeModelStruct.model, pos, ModelController.Instance.eaveColumnModelStruct.friezeModelStruct.model.transform.rotation) as GameObject;
				clone.transform.rotation = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Quaternion.Euler(ModelController.Instance.eaveColumnModelStruct.friezeModelStruct.rotation);
				//clone.transform.GetChild(0).localScale = new Vector3(eaveColumnModelStruct.friezeModelStruct.scale.x, eaveColumnModelStruct.friezeModelStruct.scale.y, (eaveColumnModelStruct.friezeModelStruct.scale.z / width) * (width + disDiff));
				clone.transform.GetChild(0).localScale = new Vector3(ModelController.Instance.eaveColumnModelStruct.friezeModelStruct.scale.x, ModelController.Instance.eaveColumnModelStruct.friezeModelStruct.scale.y, (ModelController.Instance.eaveColumnModelStruct.friezeModelStruct.scale.z) * (width + disDiff) / MainController.Instance.friezeWidth);
				//clone.transform.GetChild(0).localScale = eaveColumnModelStruct.friezeModelStruct.scale;
				clone.transform.parent = body.transform;
			}
		}
	}
	public void CreateBalustrade()
	{
		float heightOffset = 0.9f * eaveColumnHeight;

		for (int i = 0; i < eaveColumnList.Count; i++)
		{
			float width = MainController.Instance.balustradeWidth;
			float dis = Vector3.Distance(eaveColumnList[i].pos, eaveColumnList[(i + 1) % eaveColumnList.Count].pos) - eaveColumnDownRadius * 2;
			float number = Mathf.FloorToInt(dis / width);
			Vector3 dir = eaveColumnList[(i + 1) % eaveColumnList.Count].pos - eaveColumnList[i].pos;
			float disDiff = (dis - width * number) / number;
			width = dis / number;

			float rotateAngle = (Vector3.Dot(Vector3.right, dir) > 0 ? Vector3.Angle(dir, Vector3.forward) : 180-Vector3.Angle(dir, Vector3.forward));
			for (int j = 0; j < number; j++)
			{
				Vector3 pos = dir.normalized * (width / 2.0f + j * width + eaveColumnDownRadius) + eaveColumnList[i].bottomPos + heightOffset * Vector3.up - columnFundationHeight * Vector3.up;
				GameObject clone = Instantiate(ModelController.Instance.eaveColumnModelStruct.balustradeModelStruct.model, pos, ModelController.Instance.eaveColumnModelStruct.balustradeModelStruct.model.transform.rotation) as GameObject;
				clone.transform.rotation = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Quaternion.Euler(ModelController.Instance.eaveColumnModelStruct.balustradeModelStruct.rotation);
				clone.transform.GetChild(0).localScale = new Vector3(ModelController.Instance.eaveColumnModelStruct.balustradeModelStruct.scale.x, ModelController.Instance.eaveColumnModelStruct.balustradeModelStruct.scale.y, (ModelController.Instance.eaveColumnModelStruct.balustradeModelStruct.scale.z) * (width + disDiff) / MainController.Instance.balustradeWidth);
				//clone.transform.GetChild(0).localScale=eaveColumnModelStruct.balustradeModelStruct.scale;
				clone.transform.parent = body.transform;
			}
		}
	}
}

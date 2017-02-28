using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BodyController : Singleton<BodyController>
{
	public GameObject body = null;
	//Body******************************************************************************
	const float CUN = 3.33f;
	public enum BodyType { Chuan_Dou = 0, Tai_Liang = 1 };//Chuan_Dou 穿斗式 ,Tai_Liang 抬梁式
	public BodyType bodyType = BodyType.Chuan_Dou;

	public int bayNumber = 1;//間數量

	public float eaveColumnHeight;
	public float eaveColumnRadius = 1f;

	public Vector3 bodyCenter;

	//**********************************************************************************

	//***********************************************************************
	public int eaveColumnNumber;
	public List<CylinderMesh> eaveColumnList = new List<CylinderMesh>();
	public List<CylinderMesh> hypostyleColumnList = new List<CylinderMesh>();
	//***********************************************************************

	public void InitFunction()
	{
		//初始值******************************************************************************
		eaveColumnNumber = (int)MainController.Instance.sides;

		eaveColumnHeight = eaveColumnRadius * 11;

		bodyCenter = PlatformController.Instance.platformCenter + new Vector3(0, PlatformController.Instance.platformHeight / 2.0f + eaveColumnHeight / 2.0f, 0);
		//************************************************************************************
		body = new GameObject("Body");
		body.transform.position = bodyCenter;
		body.transform.parent = MainController.Instance.building.transform;

		switch (bodyType)
		{
			#region Chuan_Dou
		case BodyType.Chuan_Dou:
				CreateRingColumn();
				break;
			#endregion
		}
	}
	public void UpdateFunction()
	{

	}
	private CylinderMesh CreateColumn(Vector3 pos, float radius, float height)
	{
		GameObject col = new GameObject("Column");
		col.transform.position = pos;
		col.transform.parent=body.transform;
		col.AddComponent<CylinderMesh>();

		Vector3 topPos = pos + new Vector3(0, height / 2.0f, 0);
		Vector3 bottomPos = pos - new Vector3(0, height / 2.0f, 0);

		col.GetComponent<CylinderMesh>().CylinderInitSetting(topPos, bottomPos, radius, radius);
		col.GetComponent<CylinderMesh>().SetMesh();
		return col.GetComponent<CylinderMesh>();
	}
	public void CreateRingColumn()
	{
		eaveColumnList.Clear();
		 PlatformController.Instance.topPointPosList.Reverse();
		for (int i = 0; i < PlatformController.Instance.topPointPosList.Count; i++)
		{
			Vector2 v = new Vector2(PlatformController.Instance.topPointPosList[i].x - PlatformController.Instance.platformCenter.x, PlatformController.Instance.topPointPosList[i].z - PlatformController.Instance.platformCenter.z);
			v.Normalize();
			v = v * PlatformController.Instance.platformFrontWidthOffset2Body;
			Vector3 pos = PlatformController.Instance.topPointPosList[i] - new Vector3(v.x, 0, v.y) + new Vector3(0, eaveColumnHeight / 2.0f, 0);
			CylinderMesh newColumn = CreateColumn(pos, eaveColumnRadius, eaveColumnHeight);
			eaveColumnList.Add(newColumn);
		}
	}
}

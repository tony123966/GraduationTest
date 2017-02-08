using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BodyController : Singleton<BodyController>
{
	public GameObject body = null;
	const float CUN = 3.33f;


	//***********************************************************************
	//public bool twoSide=false;
	public int eaveColumnNumber;
	public List<CylinderMesh> eaveColumnList = new List<CylinderMesh>();
	public List<CylinderMesh> hypostyleColumnList = new List<CylinderMesh>();
	//***********************************************************************

	public void InitFunction()
	{

		eaveColumnNumber = (int)MainController.Instance.sides;

		MainController.Instance.eaveColumnHeight = MainController.Instance.eaveColumnRadius * 11;

		MainController.Instance.bodyCenter = MainController.Instance.platformCenter + new Vector3(0, MainController.Instance.platformHeight / 2.0f + MainController.Instance.eaveColumnHeight / 2.0f, 0);

		body = new GameObject("Body");
		body.transform.position = MainController.Instance.bodyCenter;
		body.transform.parent = MainController.Instance.building.transform;

		switch (MainController.Instance.bodyType)
		{
			#region Chuan_Dou
		case MainController.BodyType.Chuan_Dou:
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
		for (int i = 0; i < MainController.Instance.platformController.topPointPosList.Count; i++)
		{
			Vector2 v = new Vector2(MainController.Instance.platformController.topPointPosList[i].x - MainController.Instance.platformCenter.x, MainController.Instance.platformController.topPointPosList[i].z - MainController.Instance.platformCenter.z);
			v.Normalize();
			v = v * MainController.Instance.platformFrontWidthOffset2Body;
			Vector3 pos = MainController.Instance.platformController.topPointPosList[i] - new Vector3(v.x, 0, v.y) + new Vector3(0, MainController.Instance.eaveColumnHeight / 2.0f, 0);
			CylinderMesh newColumn = CreateColumn(pos, MainController.Instance.eaveColumnRadius, MainController.Instance.eaveColumnHeight);
			eaveColumnList.Add(newColumn);
		}
	}
}

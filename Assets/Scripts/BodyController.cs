using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BodyController : MonoBehaviour
{
	const float CUN = 3.33f;
	private static BodyController instance;
	public static BodyController Instance { get { return instance; } }
	//***********************************************************************
	//public bool twoSide=false;
	public int eaveColumnNumber;
	public List<CylinderMesh> eaveColumnList = new List<CylinderMesh>();
	public List<GameObject> hypostyleColumnList = new List<GameObject>();
	//***********************************************************************
	private void Awake()
	{
		if (instance != null && instance != this) Destroy(this.gameObject);
		else instance = this;
	}
	public void InitFunction()
	{

		eaveColumnNumber = (int)MainController.sides;

		MainController.bodyHeight = MainController.bodyRadius * 11;

		MainController.bodyCenter = MainController.platformCenter + new Vector3(0, MainController.platformHeight / 2.0f + MainController.bodyHeight / 2.0f, 0);

		switch (MainController.bodyType)
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
		col.AddComponent<CylinderMesh>();

		Vector3 topPos = pos + new Vector3(0, height / 2.0f, 0);
		Vector3 bottomPos = pos - new Vector3(0, height / 2.0f, 0);

		col.GetComponent<CylinderMesh>().CylinderInitSetting(topPos, bottomPos, radius, radius);
		col.GetComponent<CylinderMesh>().SetMesh();
		return col.GetComponent<CylinderMesh>();
	}
	public void CreateRingColumn()
	{
		Debug.Log("ddc" + PlatformController.Instance.name);
	Debug.Log("dd"+ PlatformController.Instance.topPointPosList.Count);
		for (int i = 0; i < PlatformController.Instance.topPointPosList.Count; i++)
		{
			Vector2 v = new Vector2(PlatformController.Instance.topPointPosList[i].x - MainController.platformCenter.x, PlatformController.Instance.topPointPosList[i].z - MainController.platformCenter.z);
			v.Normalize();
			v = v * MainController.platformFrontWidthOffset2Body;
			Vector3 pos = PlatformController.Instance.topPointPosList[i] - new Vector3(v.x, 0, v.y) + new Vector3(0, MainController.bodyHeight / 2.0f, 0);
			CreateColumn(pos, MainController.bodyRadius, MainController.bodyHeight);
			Debug.Log("pos" + pos);
		}

		GameObject col = new GameObject("Column");
		col.AddComponent<CylinderMesh>();

		Vector3 topPos =  new Vector3(10, 0, 0);
		Vector3 bottomPos = new Vector3(10, 10, 10);

		col.GetComponent<CylinderMesh>().CylinderInitSetting(topPos, bottomPos, 10, 10);
		col.GetComponent<CylinderMesh>().SetMesh();
	}
}

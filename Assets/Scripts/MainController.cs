using UnityEngine;
using System.Collections;

public class MainController : MonoBehaviour {
	public GameObject controlPoint;
	public static Vector3 center=Vector3.zero;
	private static MainController instance;
	public static MainController Instance { get { return instance; } }
	//FormFractor***********************************************************************
	public static int sides;
	//**********************************************************************************
	//Body******************************************************************************
	public enum BodyType { Chuan_Dou = 0, Tai_Liang = 1 };//Chuan_Dou 穿斗式 ,Tai_Liang 抬梁式
	public static BodyType bodyType = BodyType.Chuan_Dou;
	public static int bayNumber=3;
	public static float frontBayWidth;
	public static float sideBayWidth;

	public static float bodyHeight;

	public BodyController bodyController = BodyController.Instance;
	//**********************************************************************************
	//Platform**************************************************************************
	public enum PlatformType { };
	public static float platformFrontWidth = 10;
	public static float platformSideWidth = 10;
	public static float platformFrontWidthOffset2Body = 1;
	public static float platformSideWidthOffset2Body = 1;

	public static float platformHeight;

	//**********************************************************************************
	// Use this for initialization
	private void Awake()
	{
		if (instance != null && instance != this)Destroy(this.gameObject);
		else instance = this;
	}
	private void InitSetting()
	{
		bodyController.InitFunction();
	}
}

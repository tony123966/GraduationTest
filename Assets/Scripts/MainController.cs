using UnityEngine;
using System.Collections;

public class MainController : MonoBehaviour {
	public static Vector3 center;
	private static MainController instance;
	public static MainController Instance { get { return instance; } }
	//FormFactor***********************************************************************
	public enum FormFactorType { ThreeSide = 3, FourSide = 4, FiveSide = 5, SixSide = 6, EightSide=8, };
	public static FormFactorType sides = FormFactorType.ThreeSide;
	//**********************************************************************************
	//Body******************************************************************************
	public enum BodyType { Chuan_Dou = 0, Tai_Liang = 1 };//Chuan_Dou 穿斗式 ,Tai_Liang 抬梁式
	public static BodyType bodyType = BodyType.Chuan_Dou;

	public static int bayNumber=1;//間數量

	public static float bodyHeight;
	public static float bodyRadius=0.2f;

	public static Vector3 bodyCenter;

	public BodyController bodyController = BodyController.Instance;
	//**********************************************************************************
	//Platform**************************************************************************
	public enum PlatformType { };
	public static float platformFrontWidth = 20;
	public static float platformFrontWidthOffset2Body = 1;

	public static float platformHeight=1;

	public static Vector3 platformCenter;

	public PlatformController platformController = PlatformController.Instance;
	//**********************************************************************************
	// Use this for initialization
	private void Awake()
	{
		if (instance != null && instance != this)Destroy(this.gameObject);
		else instance = this;

		InitSetting();
	}
	private void InitSetting()
	{
		platformController.InitFunction();
		bodyController.InitFunction();
	}
}

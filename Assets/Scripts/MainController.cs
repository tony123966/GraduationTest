using UnityEngine;
using System.Collections;
public class MainController : Singleton<MainController>
{
	public Vector3 buildingCenter = Vector3.zero;
	public static GameObject building;

	//FormFactor***********************************************************************
	public enum FormFactorType { ThreeSide = 3, FourSide = 4, FiveSide = 5, SixSide = 6, EightSide=8, };
	public static FormFactorType sides = FormFactorType.FiveSide;
	//**********************************************************************************
	//Roof**************************************************************************
	public enum RoofType { Zan_Jian_Ding = 0, Wu_Dian_Ding = 1, Lu_Ding = 2 ,Juan_Peng=3};//Zan_Jian_Ding攢尖頂, Wu_Dian_Ding廡殿頂,Lu_Ding盝頂,Juan_Peng卷棚
	public  RoofType roofType = RoofType.Zan_Jian_Ding;

	public static Vector3 roofTopCenter;
	public static float tileLength;//瓦片長度
	public static float allJijaHeight;//總舉架高度
	public static float eave2eaveColumnOffset;//出檐長度

	public static RoofController roofController = RoofController.Instance;
	//**********************************************************************************
	//Body******************************************************************************
	public enum BodyType { Chuan_Dou = 0, Tai_Liang = 1 };//Chuan_Dou 穿斗式 ,Tai_Liang 抬梁式
	public static BodyType bodyType = BodyType.Chuan_Dou;

	public static int bayNumber = 1;//間數量

	public static float eaveColumnHeight;
	public static float eaveColumnRadius = 0.2f;

	public static Vector3 bodyCenter;

	public static BodyController bodyController = BodyController.Instance;
	//**********************************************************************************
	//Platform**************************************************************************
	public enum PlatformType { };
	public static float platformFrontWidth = 5;
	public static float platformFrontWidthOffset2Body;

	public static float platformHeight;

	public static Vector3 platformCenter=Vector3.zero;

	public static PlatformController platformController = PlatformController.Instance;
	//**********************************************************************************
	// Use this for initialization

	private void Awake()
	{
		InitFunction();
	}
	private void InitFunction()
	{

		building = new GameObject("Building");
		building.transform.position = buildingCenter;

		platformFrontWidthOffset2Body=platformFrontWidth*0.1f;

		platformController.InitFunction();
		bodyController.InitFunction();
		roofController.InitFunction();
	}
}

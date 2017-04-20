using UnityEngine;
using System.Collections;

public class ModelController : Singleton<ModelController>
{

	//**********************************************************************************
	[HideInInspector]
	public EaveColumnModelStruct eaveColumnModelStruct;

	public GameObject friezeModel;
	public Vector3 friezeModelRotation = Vector3.zero;
	public Vector3 friezeModelScale = new Vector3(5, 5, 5);
	private ModelStruct friezeModelStruct;
	public GameObject balustradeModel;
	public Vector3 balustradeModelRotation = Vector3.zero;
	public Vector3 balustradeModelScale = new Vector3(5, 5, 5);
	private ModelStruct balustradeModelStruct;

	[HideInInspector]
	public GoldColumnModelStruct goldColumnModelStruct;

	public GameObject windowModel;
	public Vector3 windowModelRotation = Vector3.zero;
	public Vector3 windowModelScale = Vector3.one;
	private ModelStruct windowModelStruct;

	//***********************************************************************
	//Model*************************************************************************************
	[HideInInspector]
	public RoofSurfaceModelStruct roofSurfaceModelStruct;

	public GameObject roundTileModel;
	public Vector3 roundTileModelRotation = new Vector3(0, 90, 0);
	public Vector3 roundTileModelScale = Vector3.one;
	private ModelStruct roundTileModelStruct;
	public GameObject flatTileModel;
	public Vector3 flatTileModelRotation = Vector3.zero;
	public Vector3 flatTileModelScale = new Vector3(1, 1, 1.2f);
	private ModelStruct flatTileModelStruct;
	public GameObject eaveTileModel;
	public Vector3 eaveTileModelRotation = Vector3.zero;
	public Vector3 eaveTileModelScale = Vector3.one;
	private ModelStruct eaveTileModelStruct;

	[HideInInspector]
	public MainRidgeModelStruct mainRidgeModelStruct;

	public GameObject mainRidgeTileModel;
	public Vector3 mainRidgeTileModelRotation = Vector3.zero;
	public Vector3 mainRidgeTileModelScale = new Vector3(2, 2, 0.5f);
	private ModelStruct mainRidgeTileModelStruct;

	//******************************************************************************************

	void Awake() 
	{
		friezeModelStruct = new ModelStruct(friezeModel, friezeModelRotation, friezeModelScale);
		balustradeModelStruct = new ModelStruct(balustradeModel, balustradeModelRotation, balustradeModelScale);
		eaveColumnModelStruct = new EaveColumnModelStruct(friezeModelStruct, balustradeModelStruct);

		windowModelStruct = new ModelStruct(windowModel, windowModelRotation, windowModelScale);
		goldColumnModelStruct = new GoldColumnModelStruct(windowModelStruct);
		//**************************************************************************************


		roundTileModelStruct = new ModelStruct(roundTileModel, roundTileModelRotation, roundTileModelScale);
		flatTileModelStruct = new ModelStruct(flatTileModel, flatTileModelRotation, flatTileModelScale);
		eaveTileModelStruct = new ModelStruct(eaveTileModel, eaveTileModelRotation, eaveTileModelScale);
		roofSurfaceModelStruct = new RoofSurfaceModelStruct(roundTileModelStruct, flatTileModelStruct, eaveTileModelStruct);


		mainRidgeTileModelStruct = new ModelStruct(mainRidgeTileModel, mainRidgeTileModelRotation, mainRidgeTileModelScale);
		mainRidgeModelStruct = new MainRidgeModelStruct(mainRidgeTileModelStruct);


	}
}

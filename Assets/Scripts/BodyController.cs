using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class BodyController : MonoBehaviour
{
	const float CUN = 3.33f;
	private static BodyController instance;
	public static BodyController Instance { get { return instance; } }
	//***********************************************************************
	public bool twoSide=false;
	public List<GameObject> eaveColumnList = new List<GameObject>();
	public List<GameObject> hypostyleColumnList = new List<GameObject>();
	//***********************************************************************
	private void Awake()
	{
		if (instance != null && instance != this) Destroy(this.gameObject);
		else instance = this;
	}
	public void InitFunction()
	{
		MainController.frontBayWidth = (MainController.platformFrontWidth - 2 * MainController.platformFrontWidthOffset2Body) / MainController.bayNumber;
		MainController.sideBayWidth = (MainController.platformSideWidth - 2 * MainController.platformSideWidthOffset2Body) / MainController.bayNumber;

		switch (MainController.bodyType)
		{
			#region Chuan_Dou
			case MainController.BodyType.Chuan_Dou:

			if (twoSide)
			{
				
			}
			else
			{
				//MainController.center
			
			}

				break;
			#endregion

			#region Tai_Liang
			case MainController.BodyType.Tai_Liang:
				break;

			#endregion
		}
	}
	public void UpdateFunction()
	{
	
	}
	private void CreateColumn(Vector3 pos,float height) 
	{ 
	
	
	}
}

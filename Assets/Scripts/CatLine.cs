using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class CatLine : MonoBehaviour {

	private int numberOfPoints = 300;
	public List<GameObject> controlPointList = new List<GameObject>();
	public List<Vector3> innerPointList = new List<Vector3>();
	public List<Vector3> anchorInnerPointlist = new List<Vector3>(); 
	public void SetLineNumberOfPoints(int number)
	{
		numberOfPoints = number;
	}
	public void SetCatmullRom(float anchorDis=0)
	{
		DisplayCatmullromSpline(anchorDis);
	}
	Vector3 ReturnCatmullRomPos(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
	{
		Vector3 pos = 0.5f * ((2f * p1) + (-p0 + p2) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t * t + (-p0 + 3f * p1 - 3f * p2 + p3) * t * t * t);
		return pos;
	}
	void DisplayCatmullromSpline(float anchorDis)
	{
		innerPointList.Clear();
		Vector3 p0, p1, p2, p3;

		if (controlPointList.Count < 2) return;
		else if (controlPointList.Count == 2)
		{
			p0 = controlPointList[0].transform.position;
			p1 = controlPointList[0].transform.position;
			p2 = controlPointList[1].transform.position;
			p3 = controlPointList[1].transform.position;


			float segmentation = 1 / (float)numberOfPoints;
			float t = 0;
			for (int i = 0; i < numberOfPoints; i++)
			{
				Vector3 newPos = ReturnCatmullRomPos(t, p0, p1, p2, p3);
				innerPointList.Add(newPos);
				t += segmentation;
			}
		}
		else
		{
			for (int index = 0; index < controlPointList.Count - 1; index++)
			{

				p0 = controlPointList[Mathf.Max(index - 1, 0)].transform.position;
				p1 = controlPointList[index].transform.position;
				p2 = controlPointList[Mathf.Min(index + 1, controlPointList.Count - 1)].transform.position;
				p3 = controlPointList[Mathf.Min(index + 2, controlPointList.Count - 1)].transform.position;


				float segmentation = 1 / (float)numberOfPoints;
				float t = 0;
				float dis=0;
				for (int i = 0; i < numberOfPoints; i++)
				{
					Vector3 newPos = ReturnCatmullRomPos(t, p0, p1, p2, p3);
					innerPointList.Add(newPos);
					if (dis >= anchorDis)
					{
						anchorInnerPointlist.Add(newPos);
						dis=0;
					}
					t += segmentation;
					dis+=Vector3.Magnitude(innerPointList[i]-innerPointList[(i>0?(i-1):0)]);
				}

			}
		}
	}
}

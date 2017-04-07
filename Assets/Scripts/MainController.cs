﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
struct ModelStruct//模型旋轉、縮放
{
	public GameObject model;
	public Vector3 rotation;
	public Vector3 scale;

	public ModelStruct(GameObject model, Vector3 rotation, Vector3 scale)
	{
		this.model = model;
		this.rotation = rotation;
		this.scale = scale;
	}
}
public class MainController : Singleton<MainController>
{
	public Vector3 buildingCenter = Vector3.zero;
	public GameObject building;

	//FormFactor***********************************************************************
	public enum FormFactorSideType { ThreeSide = 3, FourSide = 4, FiveSide = 5, SixSide = 6, EightSide = 8 };
	public FormFactorSideType sides = FormFactorSideType.ThreeSide;
	public enum FormFactorType { RegularRing = 0, FreeQuad = 1 };//Ring 正多邊形, FreeQuad 非等邊長矩形
	public FormFactorType formFactorType = FormFactorType.RegularRing;
	//**********************************************************************************


	// Use this for initialization

	private void Awake()
	{
		InitFunction();
	}
	public void InitFunction()
	{

		building = new GameObject("Building");
		building.transform.position = buildingCenter;

		PlatformController.Instance.InitFunction();
		BodyController.Instance.InitFunction();
		RoofController.Instance.InitFunction();
	}
	public List<Vector3> CreateCubeMesh(Vector3 centerPos, float width, float height, float length, float rotateAngle, MeshFilter meshFilter)
	{
		List<Vector3> controlPointPosList = new List<Vector3>();

		Mesh mesh = new Mesh();
		meshFilter.mesh = mesh;
		mesh.Clear();

		#region Vertices
		Vector3 p0 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-width * .5f, -height * .5f, length * .5f) + centerPos;
		Vector3 p1 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(width * .5f, -height * .5f, length * .5f) + centerPos;
		Vector3 p2 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(width * .5f, -height * .5f, -length * .5f) + centerPos;
		Vector3 p3 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-width * .5f, -height * .5f, -length * .5f) + centerPos;

		Vector3 p4 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-width * .5f, height * .5f, length * .5f) + centerPos;
		Vector3 p5 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(width * .5f, height * .5f, length * .5f) + centerPos;
		Vector3 p6 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(width * .5f, height * .5f, -length * .5f) + centerPos;
		Vector3 p7 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-width * .5f, height * .5f, -length * .5f) + centerPos;



		controlPointPosList.Add(p0);
		controlPointPosList.Add(p1);
		controlPointPosList.Add(p2);
		controlPointPosList.Add(p3);

		controlPointPosList.Add(p7);
		controlPointPosList.Add(p6);
		controlPointPosList.Add(p5);
		controlPointPosList.Add(p4);

		Vector3[] vertices = new Vector3[]
		{
			// Bottom
			p0, p1, p2, p3,
 
			// Left
			p7, p4, p0, p3,
 
			// Front
			p4, p5, p1, p0,
 
			// Back
			p6, p7, p3, p2,
 
			// Right
			p5, p6, p2, p1,
 
			// Top
			p7, p6, p5, p4
		};
		#endregion

		#region Normales
		Vector3 up = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.up;
		Vector3 down = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.down;
		Vector3 front = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.forward;
		Vector3 back = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.back;
		Vector3 left = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.left;
		Vector3 right = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.right;

		Vector3[] normales = new Vector3[]
		{
			// Bottom
			down, down, down, down,
 
			// Left
			left, left, left, left,
 
			// Front
			front, front, front, front,
 
			// Back
			back, back, back, back,
 
			// Right
			right, right, right, right,
 
			// Top
			up, up, up, up
		};
		#endregion

		#region UVs
		Vector2 _00 = new Vector2(0f, 0f);
		Vector2 _10 = new Vector2(1f, 0f);
		Vector2 _01 = new Vector2(0f, 1f);
		Vector2 _11 = new Vector2(1f, 1f);

		Vector2[] uvs = new Vector2[]
		{
			// Bottom
			_11, _01, _00, _10,
 
			// Left
			_11, _01, _00, _10,
 
			// Front
			_11, _01, _00, _10,
 
			// Back
			_11, _01, _00, _10,
 
			// Right
			_11, _01, _00, _10,
 
			// Top
			_11, _01, _00, _10,
		};
		#endregion

		#region Triangles
		int[] triangles = new int[]
		{
			// Bottom
			3, 1, 0,
			3, 2, 1,			
 
			// Left
			3 + 4 * 1, 1 + 4 * 1, 0 + 4 * 1,
			3 + 4 * 1, 2 + 4 * 1, 1 + 4 * 1,
 
			// Front
			3 + 4 * 2, 1 + 4 * 2, 0 + 4 * 2,
			3 + 4 * 2, 2 + 4 * 2, 1 + 4 * 2,
 
			// Back
			3 + 4 * 3, 1 + 4 * 3, 0 + 4 * 3,
			3 + 4 * 3, 2 + 4 * 3, 1 + 4 * 3,
 
			// Right
			3 + 4 * 4, 1 + 4 * 4, 0 + 4 * 4,
			3 + 4 * 4, 2 + 4 * 4, 1 + 4 * 4,
 
			// Top
			3 + 4 * 5, 1 + 4 * 5, 0 + 4 * 5,
			3 + 4 * 5, 2 + 4 * 5, 1 + 4 * 5,
 
		};
		#endregion

		mesh.vertices = vertices;
		mesh.normals = normales;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		mesh.RecalculateBounds();
		mesh.Optimize();

		return controlPointPosList;
	}
	public List<Vector3> CreateRegularRingMesh(Vector3 centerPos, int nbSides, float radius, float height, float rotateAngle, MeshFilter meshFilter)
	{

		List<Vector3> controlPointPosList = new List<Vector3>();
		controlPointPosList.Clear();

		Mesh mesh = new Mesh();
		meshFilter.mesh = mesh;
		mesh.Clear();

		Vector3 topPos = centerPos + new Vector3(0, height / 2.0f, 0);
		Vector3 bottomPos = centerPos - new Vector3(0, height / 2.0f, 0);
		float bottomRadius = radius;
		float topRadius = radius;
		float _2pi = Mathf.PI * 2f;

		#region Vertices
		Vector3[] vertices = new Vector3[3 * nbSides * 2 + nbSides * 6];
		int vert = 0;

		// Bottom cap
		vertices[vert++] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * bottomPos;
		for (int i = 0; i < nbSides; i++, vert++)
		{
			float rad = (float)i / nbSides * _2pi;
			vertices[vert] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * (new Vector3(Mathf.Cos(rad) * bottomRadius, 0f, Mathf.Sin(rad) * bottomRadius) + bottomPos);
			controlPointPosList.Add(vertices[vert]);
		}
		// Top cap
		vertices[vert++] = topPos;
		for (int i = 0; i < nbSides; i++, vert++)
		{
			float rad = (float)i / nbSides * _2pi;
			vertices[vert] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * (new Vector3(Mathf.Cos(rad) * topRadius, 0f, Mathf.Sin(rad) * topRadius) + topPos);
			controlPointPosList.Add(vertices[vert]);
		}
		// Sides
		for (int i = 0; i < nbSides; i++)
		{
			vertices[vert++] = vertices[i + 1];
			vertices[vert++] = vertices[i + 1 + (nbSides + 1)];
			vertices[vert++] = vertices[(i + 1) % nbSides + 1 + (nbSides + 1)];
			vertices[vert++] = vertices[i + 1];
			vertices[vert++] = vertices[(i + 1) % nbSides + 1 + (nbSides + 1)];
			vertices[vert++] = vertices[(i + 1) % nbSides + 1];

		}

		#endregion
		#region Normales

		// bottom + top + sides
		Vector3[] normales = new Vector3[vertices.Length];
		vert = 0;
		// Bottom cap
		for (int i = 0; i <= nbSides; i++)
		{
			normales[vert++] = Vector3.down;
		}
		// Top cap
		for (int i = 0; i <= nbSides; i++)
		{
			normales[vert++] = Vector3.up;
		}
		// Sides
		for (int i = 0; i < nbSides; i++)
		{
			Vector3 nor = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.Cross(vertices[i + 1 + (nbSides + 1)] - vertices[i + 1], vertices[(i + 1) % nbSides + 1] - vertices[i + 1]).normalized;
			for (int j = 0; j < 2; j++)
			{
				normales[vert++] = nor;
			}
		}
		#endregion
		#region UVs
		//Vector2[] uvs = new Vector2[vertices.Length];
		#endregion
		#region Triangles
		int[] triangles = new int[3 * nbSides * 2 + 6 * nbSides];
		vert = 0;
		// Bottom cap
		for (int i = 0; i < nbSides; i++)
		{
			triangles[vert++] = 0;
			triangles[vert++] = i + 1;
			triangles[vert++] = (i + 1) % nbSides + 1;
		}
		// Top cap
		for (int i = 0; i < nbSides; i++)
		{
			triangles[vert++] = (nbSides + 1);
			triangles[vert++] = (i + 1) % nbSides + 1 + (nbSides + 1);
			triangles[vert++] = i + 1 + (nbSides + 1);
		}
		// Sides

		for (int i = 0; i < nbSides; i++)
		{
			triangles[vert++] = i + 1;
			triangles[vert++] = (i + 1) % nbSides + 1 + (nbSides + 1);
			triangles[vert++] = (i + 1) % nbSides + 1;

			triangles[vert++] = i + 1;
			triangles[vert++] = i + 1 + (nbSides + 1);
			triangles[vert++] = (i + 1) % nbSides + 1 + (nbSides + 1);
		}
		#endregion


		mesh.vertices = vertices;
		mesh.normals = normales;
		//mesh.uv = uvs;
		mesh.triangles = triangles;

		mesh.RecalculateBounds();
		mesh.Optimize();

		return controlPointPosList;
		
			#region Test
		/*	List<Vector3> controlPointPosList = new List<Vector3>();

		Mesh mesh = new Mesh();
		meshFilter.mesh = mesh;
		mesh.Clear();


		Vector3 topPos = new Vector3(0, height / 2.0f, 0);
		Vector3 bottomPos = - new Vector3(0, height / 2.0f, 0);


		float bottomRadius = radius;
		float topRadius = radius;
		int nbVerticesCap = nbSides + 1;
		#region Vertices

		// bottom + top + sides
		Vector3[] vertices = new Vector3[nbVerticesCap + nbVerticesCap + nbSides * 2 + 2];
		int vert = 0;
		float _2pi = Mathf.PI * 2f;

		float cylinderLength = Vector3.Magnitude(topPos - bottomPos);

		// Bottom cap
		vertices[vert++] = bottomPos;
		while (vert <= nbSides)
		{
			float rad = (float)vert / nbSides * _2pi;
			vertices[vert] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * ((new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius)) + bottomPos) + centerPos;
			controlPointPosList.Add(vertices[vert]);
			vert++;
		}

		// Top cap

		vertices[vert++] = (bottomPos + Vector3.up * cylinderLength);
		while (vert <= nbSides * 2 + 1)
		{
			float rad = (float)(vert - nbSides - 1) / nbSides * _2pi;
			vertices[vert] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * ((new Vector3(Mathf.Cos(rad) * topRadius, 0, Mathf.Sin(rad) * topRadius)) + bottomPos + Vector3.up * cylinderLength)+centerPos;
			controlPointPosList.Add(vertices[vert]);
			vert++;
		}

		// Sides
		int v = 0;
		while (vert <= vertices.Length - 4)
		{
			float rad = (float)v / nbSides * _2pi;
			vertices[vert] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * ((new Vector3(Mathf.Cos(rad) * topRadius, 0, Mathf.Sin(rad) * topRadius)) + bottomPos + Vector3.up * cylinderLength)+centerPos;
			vertices[vert + 1] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * (new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius)) + bottomPos;
			vert += 2;
			v++;
		}
		vertices[vert] = vertices[nbSides * 2 + 2];
		vertices[vert + 1] = vertices[nbSides * 2 + 3];

		#endregion

		#region Normales

		// bottom + top + sides
		Vector3[] normales = new Vector3[vertices.Length];
		vert = 0;

		// Bottom cap
		while (vert <= nbSides)
		{
			normales[vert++] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.down;
		}

		// Top cap
		while (vert <= nbSides * 2 + 1)
		{
			normales[vert++] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.up;
		}

		// Sides
		v = 0;
		while (vert <= vertices.Length - 4)
		{
			float rad = (float)v / nbSides * _2pi;
			float cos = Mathf.Cos(rad);
			float sin = Mathf.Sin(rad);

			normales[vert] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(cos, 0f, sin);
			normales[vert + 1] = normales[vert];

			vert += 2;
			v++;
		}
		normales[vert] = normales[nbSides * 2 + 2];
		normales[vert + 1] = normales[nbSides * 2 + 3];
		#endregion

		#region UVs
		Vector2[] uvs = new Vector2[vertices.Length];

		// Bottom cap
		int u = 0;
		uvs[u++] = new Vector2(0.5f, 0.5f);
		while (u <= nbSides)
		{
			float rad = (float)u / nbSides * _2pi;
			uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
			u++;
		}

		// Top cap
		uvs[u++] = new Vector2(0.5f, 0.5f);
		while (u <= nbSides * 2 + 1)
		{
			float rad = (float)u / nbSides * _2pi;
			uvs[u] = new Vector2(Mathf.Cos(rad) * .5f + .5f, Mathf.Sin(rad) * .5f + .5f);
			u++;
		}

		// Sides
		int u_sides = 0;
		while (u <= uvs.Length - 4)
		{
			float t = (float)u_sides / nbSides;
			uvs[u] = new Vector3(t, 1f);
			uvs[u + 1] = new Vector3(t, 0f);
			u += 2;
			u_sides++;
		}
		uvs[u] = new Vector2(1f, 1f);
		uvs[u + 1] = new Vector2(1f, 0f);
		#endregion

		#region Triangles
		int nbTriangles = nbSides + nbSides + nbSides * 2;
		int[] triangles = new int[nbTriangles * 3 + 3];

		// Bottom cap
		int tri = 0;
		int i = 0;
		while (tri < nbSides - 1)
		{
			triangles[i] = 0;
			triangles[i + 1] = tri + 1;
			triangles[i + 2] = tri + 2;
			tri++;
			i += 3;
		}
		triangles[i] = 0;
		triangles[i + 1] = tri + 1;
		triangles[i + 2] = 1;
		tri++;
		i += 3;

		// Top cap
		//tri++;
		while (tri < nbSides * 2)
		{
			triangles[i] = tri + 2;
			triangles[i + 1] = tri + 1;
			triangles[i + 2] = nbVerticesCap;
			tri++;
			i += 3;
		}

		triangles[i] = nbVerticesCap + 1;
		triangles[i + 1] = tri + 1;
		triangles[i + 2] = nbVerticesCap;
		tri++;
		i += 3;
		tri++;

		// Sides
		while (tri <= nbTriangles)
		{
			triangles[i] = tri + 2;
			triangles[i + 1] = tri + 1;
			triangles[i + 2] = tri + 0;
			tri++;
			i += 3;

			triangles[i] = tri + 1;
			triangles[i + 1] = tri + 2;
			triangles[i + 2] = tri + 0;
			tri++;
			i += 3;
		}
		#endregion


		mesh.vertices = vertices;
		mesh.normals = normales;
		mesh.uv = uvs;
		mesh.triangles = triangles;

		mesh.RecalculateBounds();
		mesh.Optimize();


		return controlPointPosList;*/
			#endregion
		
	}
	public void CreateTorusMesh(Vector3 centerPos, float width, float height, float length, float innerWidthRatio, float innerHeightDownRatio, float innerHeightTopRatio, float innerLengthRatio, float rotateAngle, MeshFilter meshFilter)
	{
		int nbSides = 4;

		Mesh mesh = new Mesh();
		meshFilter.mesh = mesh;
		mesh.Clear();
		Vector3 p0 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(width * .5f, -height * .5f, length * .5f) + centerPos;
		Vector3 p1 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(width * .5f, height * .5f, length * .5f) + centerPos;
		Vector3 p2 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-width * .5f, height * .5f, length * .5f) + centerPos;
		Vector3 p3 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-width * .5f, -height * .5f, length * .5f) + centerPos;

		Vector3 p4 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(width * .5f, -height * .5f, -length * .5f) + centerPos;
		Vector3 p5 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(width * .5f, height * .5f, -length * .5f) + centerPos;
		Vector3 p6 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-width * .5f, height * .5f, -length * .5f) + centerPos;
		Vector3 p7 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-width * .5f, -height * .5f, -length * .5f) + centerPos;

		Vector3 p8 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(innerWidthRatio * width * .5f, -(.5f - innerHeightDownRatio) * height, innerLengthRatio * length * .5f) + centerPos;
		Vector3 p9 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(innerWidthRatio * width * .5f, (innerHeightTopRatio - .5f) * height, innerLengthRatio * length * .5f) + centerPos;
		Vector3 p10 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-innerWidthRatio * width * .5f, (innerHeightTopRatio - .5f) * height, innerLengthRatio * length * .5f) + centerPos;
		Vector3 p11 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-innerWidthRatio * width * .5f, -(.5f - innerHeightDownRatio) * height, innerLengthRatio * length * .5f) + centerPos;

		Vector3 p12 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(innerWidthRatio * width * .5f, -(.5f - innerHeightDownRatio) * height, -innerLengthRatio * length * .5f) + centerPos;
		Vector3 p13 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(innerWidthRatio * width * .5f, (innerHeightTopRatio - .5f) * height, -innerLengthRatio * length * .5f) + centerPos;
		Vector3 p14 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-innerWidthRatio * width * .5f, (innerHeightTopRatio - .5f) * height, -innerLengthRatio * length * .5f) + centerPos;
		Vector3 p15 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-innerWidthRatio * width * .5f, -(.5f - innerHeightDownRatio) * height, -innerLengthRatio * length * .5f) + centerPos;
		// Outter shell is at radius1 + radius2 / 2, inner shell at radius1 - radius2 / 2

		#region Vertices

		// bottom + top + sides
		//Vector3[] vertices = new Vector3[nbVerticesCap * 2 + nbVerticesSides * 2];


		Vector3[] vertices = new Vector3[]
		{
			// Bottom
			p4, p0, p3, p7,
 			// innerBottom
			p8, p12, p15, p11,
			// Top
			p1, p5, p6, p2,
			// innerTop
			p13, p9, p10, p14,
			// Left
			p4, p5, p1, p0,
 			// innerLeft
			p8, p9, p13, p12,
			// Right
			p3, p2, p6, p7,
 			// innerRight
			p15, p14, p10, p11,
			// Front
			p0, p1, p9, p8,
			p1, p2, p10, p9,
			p2, p3, p11, p10,
			p3, p0, p8, p11,
			// Back
			p7, p6, p14, p15,
			p6, p5, p13, p14,
			p5, p4, p12, p13,
			p4, p7, p15, p12,
			// Back
			//p7, p6, p5, p4,
 			// innerBack
			//p15, p14, p13, p12

		};
		#endregion

		#region Normales

		// bottom + top + sides
		Vector3[] normales = new Vector3[vertices.Length];
		int vert = 0;
		// Bottom cap
		for (int i = 0; i < nbSides; i++)
		{
			normales[i + vert] = Vector3.down;
			normales[i + nbSides + vert] = -Vector3.down;
		}
		vert += nbSides * 2;
		// Top cap
		for (int i = 0; i < nbSides; i++)
		{
			normales[i + vert] = Vector3.up;
			normales[i + vert + nbSides] = -Vector3.up;
		}
		vert += nbSides * 2;
		// Left cap
		for (int i = 0; i < nbSides; i++)
		{
			normales[i + vert] = Vector3.right;
			normales[i + vert + nbSides] = -Vector3.right;
		}
		vert += nbSides * 2;
		// Right cap
		for (int i = 0; i < nbSides; i++)
		{
			normales[i + vert] = Vector3.left;
			normales[i + vert + nbSides] = -Vector3.left;
		}
		vert += nbSides * 2;
		// Front cap
		for (int j = 0; j < 4; j++)
		{
			Vector3 nor = Vector3.Cross(vertices[nbSides * j + 1 + vert] - vertices[nbSides * j + vert], vertices[nbSides * j + 2 + vert] - vertices[nbSides * j + vert]).normalized;
			for (int i = 0; i < nbSides; i++)
			{
				normales[i + nbSides * j + vert] = -nor;
			}
		}
		vert += nbSides * 4;
		// Back cap
		for (int j = 0; j < 4; j++)
		{
			Vector3 nor = Vector3.Cross(vertices[nbSides * j + 1 + vert] - vertices[nbSides * j + vert], vertices[nbSides * j + 2 + vert] - vertices[nbSides * j + vert]).normalized;
			for (int i = 0; i < nbSides; i++)
			{
				normales[i + nbSides * j + vert] = nor;
			}
		}
		#endregion

		#region UVs
		//Vector2[] uvs = new Vector2[vertices.Length];


		#endregion

		#region Triangles
		vert = 0;
		int[] triangles = new int[nbSides * 6 * 2 + nbSides * 6 * 2];
		for (int j = 0; j < 16; j++)
		{
			for (int i = 0; i < 2; i++)
			{
				triangles[vert++] = j * 4;
				triangles[vert++] = i + 1 + j * 4;
				triangles[vert++] = i + 2 + j * 4;
			}
		}

		#endregion

		mesh.vertices = vertices;
		mesh.normals = normales;
		//mesh.uv = uvs;
		mesh.triangles = triangles;

		mesh.RecalculateBounds();
		mesh.Optimize();
	}
	public void CreateStairMesh(Vector3 centerPos, float width, float height, float length, float rotateAngle, MeshFilter meshFilter)
	{

		Mesh mesh = new Mesh();
		meshFilter.mesh = mesh;
		mesh.Clear();

		int stairNum = 4;
		float stairLength = length / stairNum;
		float stairWidth = width;
		float stairHeight = height / stairNum;
		Vector3 stairLengthVector = Quaternion.AngleAxis(rotateAngle, Vector3.up) * (stairLength * Vector3.forward);
		Vector3 stairHeightVector = Quaternion.AngleAxis(rotateAngle, Vector3.up) * (stairHeight * Vector3.up);
		Vector3[] vertices = new Vector3[4 * (2 + 4 * stairNum)];

		#region Vertices
		Vector3 p0 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-width * .5f, -height * .5f, length * .5f) + centerPos;
		Vector3 p1 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(width * .5f, -height * .5f, length * .5f) + centerPos;
		Vector3 p2 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(width * .5f, -height * .5f, -length * .5f) + centerPos;
		Vector3 p3 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-width * .5f, -height * .5f, -length * .5f) + centerPos;

		Vector3 p4 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-width * .5f, height * .5f, length * .5f) + centerPos;
		Vector3 p5 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(width * .5f, height * .5f, length * .5f) + centerPos;
		Vector3 p6 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(width * .5f, height * .5f, -length * .5f) + centerPos;
		Vector3 p7 = Quaternion.AngleAxis(rotateAngle, Vector3.up) * new Vector3(-width * .5f, height * .5f, -length * .5f) + centerPos;
		int vert = 0;
		// Bottom cap
		vertices[vert++] = p0;
		vertices[vert++] = p3;
		vertices[vert++] = p2;
		vertices[vert++] = p1;
		// Back cap
		vertices[vert++] = p7;
		vertices[vert++] = p6;
		vertices[vert++] = p2;
		vertices[vert++] = p3;
		//Stair
		for (int i = 0; i < stairNum; i++)
		{
			//Front
			vertices[vert++] = p0 + i * stairHeightVector - i * stairLengthVector;
			vertices[vert++] = p1 + i * stairHeightVector - i * stairLengthVector;
			vertices[vert++] = p1 + (i + 1) * stairHeightVector - i * stairLengthVector;
			vertices[vert++] = p0 + (i + 1) * stairHeightVector - i * stairLengthVector;

			//Top
			vertices[vert++] = p0 + (i + 1) * stairHeightVector - i * stairLengthVector;
			vertices[vert++] = p1 + (i + 1) * stairHeightVector - i * stairLengthVector;
			vertices[vert++] = p1 + (i + 1) * stairHeightVector - (i + 1) * stairLengthVector;
			vertices[vert++] = p0 + (i + 1) * stairHeightVector - (i + 1) * stairLengthVector;
			//right
			vertices[vert++] = p0 + -i * stairLengthVector;
			vertices[vert++] = p0 + (i + 1) * stairHeightVector - i * stairLengthVector;
			vertices[vert++] = p0 + (i + 1) * stairHeightVector - (i + 1) * stairLengthVector;
			vertices[vert++] = p0 - (i + 1) * stairLengthVector;

			//left
			vertices[vert++] = p1 + -i * stairLengthVector;
			vertices[vert++] = p1 - (i + 1) * stairLengthVector;
			vertices[vert++] = p1 + (i + 1) * stairHeightVector - (i + 1) * stairLengthVector;
			vertices[vert++] = p1 + (i + 1) * stairHeightVector - i * stairLengthVector;


		}
		#endregion
		#region Normales

		Vector3[] normales = new Vector3[vertices.Length];
		vert = 0;

		// Bottom cap
		for (int i = 0; i < 4; i++)
		{
			normales[vert++] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.down;
		}
		// Back cap
		for (int i = 0; i < 4; i++)
		{
			normales[vert++] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * -Vector3.forward;
		}
		//Stair
		for (int i = 0; i < stairNum; i++)
		{
			//Front
			for (int j = 0; j < 4; j++)
			{
				normales[vert++] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.forward;
			}
			//Top
			for (int j = 0; j < 4; j++)
			{
				normales[vert++] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.up;
			}
			//right
			for (int j = 0; j < 4; j++)
			{
				normales[vert++] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.right;
			}
			//left
			for (int j = 0; j < 4; j++)
			{
				normales[vert++] = Quaternion.AngleAxis(rotateAngle, Vector3.up) * Vector3.left;
			}
		}
		#endregion
		#region UVs
		//Vector2[] uvs = new Vector2[vertices.Length];
		#endregion
		#region Triangles
		vert = 0;
		int[] triangles = new int[6 * 2 + 6 * 4 * stairNum];
		for (int j = 0; j < 2 + stairNum * 4; j++)
		{
			for (int i = 0; i < 2; i++)
			{
				triangles[vert++] = j * 4;
				triangles[vert++] = i + 1 + j * 4;
				triangles[vert++] = i + 2 + j * 4;
			}
		}

		#endregion

		mesh.vertices = vertices;
		mesh.normals = normales;
		//mesh.uv = uvs;
		mesh.triangles = triangles;

		mesh.RecalculateBounds();
		mesh.Optimize();
	}
}

﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class PlatformController : Singleton<PlatformController>
{

	public GameObject platform=null;
	MeshFilter meshFilter;
	MeshRenderer meshRenderer;

	public  List<Vector3> topPointPosList;
	public  List<Vector3> bottomPointPosList;


	//***********************************************************************

	//***********************************************************************

	public void InitFunction()
	{
		float platformRadius = MainController.Instance.platformFrontWidth / (2 * Mathf.Cos((Mathf.PI * 2) / (int)MainController.Instance.sides) * 2);

		MainController.Instance.platformHeight = MainController.Instance.platformFrontWidth*0.1f;

		platform=new GameObject("Platform");
		platform.transform.position = MainController.Instance.platformCenter;
		platform.transform.parent = MainController.Instance.building.transform;
		meshFilter = platform.AddComponent<MeshFilter>();
		meshRenderer=platform.AddComponent<MeshRenderer>();
		meshRenderer.material.color = Color.white;
		CreatePlatform(MainController.Instance.platformCenter, platformRadius, MainController.Instance.platformHeight, 0);
	}
	private void CreatePlatform(Vector3 pos, float radius, float height, float rotateAngle)
	{

		Mesh mesh = new Mesh();
		meshFilter.mesh = mesh;
		mesh.Clear();


		Vector3 topPos = pos + new Vector3(0, height / 2.0f, 0);
		Vector3 bottomPos = pos - new Vector3(0, height / 2.0f, 0);


		int nbSides = (int)MainController.Instance.sides;
		float bottomRadius=radius;
		float topRadius=radius;
		int nbVerticesCap = nbSides + 1;
		#region Vertices

		// bottom + top + sides
		Vector3[] vertices = new Vector3[nbVerticesCap + nbVerticesCap + nbSides * 2 + 2];
		int vert = 0;
		float _2pi = Mathf.PI * 2f;

		float cylinderLength = Vector3.Magnitude(topPos - bottomPos);

		// Bottom cap
		bottomPointPosList.Clear();
		vertices[vert++] = bottomPos;
		while (vert <= nbSides)
		{
			float rad = (float)vert / nbSides * _2pi;
			vertices[vert] = (new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius) + bottomPos);
			bottomPointPosList.Add(vertices[vert]);
			vert++;
		}

		// Top cap
		topPointPosList.Clear();

		vertices[vert++] = (bottomPos + Vector3.up * cylinderLength);
		while (vert <= nbSides * 2 + 1)
		{
			float rad = (float)(vert - nbSides - 1) / nbSides * _2pi;
			vertices[vert] = (new Vector3(Mathf.Cos(rad) * topRadius, 0, Mathf.Sin(rad) * topRadius) + bottomPos + Vector3.up * cylinderLength);
			topPointPosList.Add(vertices[vert]);
			vert++;
		}

		// Sides
		int v = 0;
		while (vert <= vertices.Length - 4)
		{
			float rad = (float)v / nbSides * _2pi;
			vertices[vert] = (new Vector3(Mathf.Cos(rad) * topRadius, 0, Mathf.Sin(rad) * topRadius) + bottomPos + Vector3.up * cylinderLength);
			vertices[vert + 1] = (new Vector3(Mathf.Cos(rad) * bottomRadius, 0, Mathf.Sin(rad) * bottomRadius) + bottomPos);
			vert += 2;
			v++;
		}
		vertices[vert] = vertices[nbSides * 2 + 2];
		vertices[vert + 1] = vertices[nbSides * 2 + 3];
		
		for (int t = 0; t < vertices.Length; t++)
		{
			vertices[t] -= platform.transform.position;
		}
		#endregion

		#region Normales

		// bottom + top + sides
		Vector3[] normales = new Vector3[vertices.Length];
		vert = 0;

		// Bottom cap
		while (vert <= nbSides)
		{
			normales[vert++] = Vector3.down;
		}

		// Top cap
		while (vert <= nbSides * 2 + 1)
		{
			normales[vert++] = Vector3.up;
		}

		// Sides
		v = 0;
		while (vert <= vertices.Length - 4)
		{
			float rad = (float)v / nbSides * _2pi;
			float cos = Mathf.Cos(rad);
			float sin = Mathf.Sin(rad);

			normales[vert] = new Vector3(cos, 0f, sin);
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

		//platform.transform.up = Vector3.Normalize(topPos - bottomPos);
		platform.transform.RotateAround(bottomPos, topPos - bottomPos, Vector3.Angle(Vector3.up, topPos - bottomPos));
	}
	/*private void CreatePlatform(Vector3 pos, float radius, float height, float rotateAngle)
	{
		Vector3 topPos = pos + new Vector3(0, height / 2.0f, 0);
		Vector3 bottomPos = pos - new Vector3(0, height / 2.0f, 0);

		int edgeCount=(int)MainController.sides;
		float piDouble = Mathf.PI * 2;
		float sliceUnit = (piDouble / edgeCount);

		#region Vertices

		Vector3[] vertices = new Vector3[edgeCount * 2];
		for (int i = 0; i < edgeCount; i++)
		{
			float ansX = (radius * Mathf.Cos(-i * sliceUnit)); //求X座標
			float ansZ = (radius * Mathf.Sin(-i * sliceUnit)); //求Y座標

			ansX = (Mathf.Cos(-rotateAngle) * ansX - Mathf.Sin(-rotateAngle) * ansZ) + MainController.platformCenter.x;
			ansZ = (Mathf.Sin(-rotateAngle) * ansX + Mathf.Cos(-rotateAngle) * ansZ) + MainController.platformCenter.y;
			vertices[i] = new Vector3(ansX, 0, ansZ) + topPos;
			vertices[i + edgeCount] = new Vector3(ansX, 0, ansZ) + bottomPos;
		}
		#endregion

		#region Normales
		Vector3[] normales = new Vector3[vertices.Length*2];
		for (int i = 0; i < edgeCount; i++)
		{
			normales[i] = Vector3.up;
			normales[i + edgeCount]= Vector3.down;
		}
		#endregion
	}*/
}

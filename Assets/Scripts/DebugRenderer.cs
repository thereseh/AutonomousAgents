using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*

CLASS WRITTEN BY NOAH RATCLIFF!

 */

/// <summary>
/// Uses GL to render debug lines for objects in the scene
/// </summary>
public class DebugRenderer : MonoBehaviour
{
	public Material[] Materials;

	//stack to render on next pass
	private Stack<GLLine> lines;

	
	// Use this for initialization
	void Start()
	{
		lines = new Stack<GLLine>();
	}
	
	/// <summary>
	/// Pushes line data to the buffer to be rendered on next pass
	/// </summary>
	/// <param name="line"></param>
	public void DrawLine(GLLine line)
	{
		lines.Push(line);
	}
	
	/// <summary>
	/// Pushes line data to the buffer to be rendered on next pass
	/// </summary>
	/// <param name="start"></param>
	/// <param name="end"></param>
	/// <param name="material"></param>
	public void DrawLine(Vector3 start, Vector3 end, Material material)
	{
			DrawLine (new GLLine (start, end, material));
	}
	
	void OnRenderObject()
	{
		while(lines.Count > 0)
		{
			drawGLLine(lines.Pop());
		}
	}
	
	private void drawGLLine(GLLine line)
	{
			// set the material
			line.Material.SetPass (0);
		
			// draw the line
			GL.Begin (GL.LINES);
			GL.Vertex (line.Start);
			GL.Vertex (line.End);
			GL.End ();
	}
}

/// <summary>
/// Struct to hold line data
/// </summary>
public struct GLLine
{
	public Vector3 Start, End;
	public Material Material;
	
	public GLLine(Vector3 start, Vector3 end, Material material)
	{
		Start = start;
		End = end;
		Material = material;
	}
}
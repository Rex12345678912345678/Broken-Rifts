using UnityEngine;

public class QuadMeshCreator
{
	public static Mesh BuildQuad(float width, float height, Rect? uv = null, XAlignmentTypes xAlignment = XAlignmentTypes.Center, YAlignmentTypes yAlignment = YAlignmentTypes.Center)
	{
		return BuildQuad(width, height, Color.white, uv, xAlignment, yAlignment);
	}

	public static Mesh BuildQuad(float width, float height, Color color, Rect? uv = null, XAlignmentTypes xAlignment = XAlignmentTypes.Center, YAlignmentTypes yAlignment = YAlignmentTypes.Center)
	{
		var mesh = new Mesh();
		var array = new Vector3[4];
		var num = height * 0.5f;
		var num2 = width * 0.5f;
		var vector = new Vector2(0f - num, 0f - num2);
		switch (xAlignment)
		{
		case XAlignmentTypes.Left:
			vector.x = 0f;
			break;
		case XAlignmentTypes.Center:
			vector.x = 0f - num2;
			break;
		case XAlignmentTypes.Right:
			vector.x = 0f - width;
			break;
		}
		switch (yAlignment)
		{
		case YAlignmentTypes.Top:
			vector.y = 0f - height;
			break;
		case YAlignmentTypes.Center:
			vector.y = 0f - num;
			break;
		case YAlignmentTypes.Bottom:
			vector.y = 0f;
			break;
		}
		array[0] = new Vector3(vector.x, vector.y, 0f);
		array[1] = new Vector3(vector.x, vector.y + height, 0f);
		array[2] = new Vector3(vector.x + width, vector.y, 0f);
		array[3] = new Vector3(vector.x + width, vector.y + height, 0f);
		var array2 = new Vector2[array.Length];
		if (!uv.HasValue)
		{
			array2[0] = new Vector2(0f, 0f);
			array2[1] = new Vector2(0f, 1f);
			array2[2] = new Vector2(1f, 0f);
			array2[3] = new Vector2(1f, 1f);
		}
		else
		{
			array2[0] = new Vector2(uv.Value.xMin, uv.Value.yMin);
			array2[1] = new Vector2(uv.Value.xMin, uv.Value.yMax);
			array2[2] = new Vector2(uv.Value.xMax, uv.Value.yMin);
			array2[3] = new Vector2(uv.Value.xMax, uv.Value.yMax);
		}
		var triangles = new int[6] { 0, 1, 2, 3, 2, 1 };
		var normals = new Vector3[4]
		{
			Vector3.forward,
			Vector3.forward,
			Vector3.forward,
			Vector3.forward
		};
		var colors = new Color[4] { color, color, color, color };
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.triangles = triangles;
		mesh.colors = colors;
		mesh.normals = normals;
		return mesh;
	}
}

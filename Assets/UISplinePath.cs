using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UISplinePath : Graphic
{
	public Vector2 pointA;
	public Vector2 pointB;
	public List<RectTransform> obstacles;

	public float lineWidth = 5f;
	public bool horizontalFirst = true;

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		vh.Clear();

		// Calculate the shortest path around obstacles
		List<Vector2> path = CalculateShortestPath(
			pointA,
			pointB,
			obstacles);

		// Define the width of the line
		float lineWidth = 5f;

		// Create vertices for the path
		for (int i = 0; i < path.Count - 1; i++)
		{
			AddLine(vh, path[i], path[i + 1], lineWidth);
		}
	}

	private List<Vector2> CalculateShortestPath(Vector2 start, Vector2 end, List<RectTransform> obstacles)
	{
		// For simplicity, this example uses a basic approach to find the shortest path
		// In a real scenario, you might want to use a more sophisticated pathfinding algorithm like A*

		List<Vector2> path = new List<Vector2> { start };

		bool TryHorizontal()
		{
			Vector2 mid = new Vector2(end.x, start.y);
			if (!IsPathObstructed(start, mid, obstacles) && !IsPathObstructed(mid, end, obstacles))
			{
				path.Add(mid);
				path.Add(end);
				return true;
			}
			return false;
		}

		bool TryVertical()
		{
			Vector2 mid = new Vector2(start.x, end.y);
			if (!IsPathObstructed(start, mid, obstacles) && !IsPathObstructed(mid, end, obstacles))
			{
				path.Add(mid);
				path.Add(end);
				return true;
			}
			return false;
		}

		if (horizontalFirst)
		{
			if (!TryHorizontal() && !TryVertical())
				path.Add(end);
		}
		else
		{
			if (!TryVertical() && !TryHorizontal())
				path.Add(end);
		}

		return path;
	}

	private bool IsPathObstructed(Vector2 start, Vector2 end, List<RectTransform> obstacles)
	{
		foreach (var obstacle in obstacles)
		{
			Rect world = new Rect(
				obstacle.TransformPoint(obstacle.rect.min),
				obstacle.TransformPoint(obstacle.rect.max) - obstacle.TransformPoint(obstacle.rect.min));

			if (LineIntersectsRect(start, end, world))
			{
				return true;
			}
		}
		return false;
	}

	private bool LineIntersectsRect(Vector2 p1, Vector2 p2, Rect rect)
	{
		return LineIntersectsLine(p1, p2, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, rect.yMin)) ||
			   LineIntersectsLine(p1, p2, new Vector2(rect.xMax, rect.yMin), new Vector2(rect.xMax, rect.yMax)) ||
			   LineIntersectsLine(p1, p2, new Vector2(rect.xMax, rect.yMax), new Vector2(rect.xMin, rect.yMax)) ||
			   LineIntersectsLine(p1, p2, new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMin, rect.yMin));
	}

	private bool LineIntersectsLine(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
	{
		float denominator = (p4.y - p3.y) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.y - p1.y);
		if (denominator == 0)
		{
			return false;
		}

		float ua = ((p4.x - p3.x) * (p1.y - p3.y) - (p4.y - p3.y) * (p1.x - p3.x)) / denominator;
		float ub = ((p2.x - p1.x) * (p1.y - p3.y) - (p2.y - p1.y) * (p1.x - p3.x)) / denominator;

		return (ua >= 0 && ua <= 1 && ub >= 0 && ub <= 1);
	}

	private void AddLine(VertexHelper vh, Vector2 start, Vector2 end, float width)
	{
		Vector2 direction = (end - start).normalized;
		Vector2 perpendicular = new Vector2(-direction.y, direction.x) * width / 2;

		UIVertex vertex = UIVertex.simpleVert;
		vertex.color = color;

		// Define the vertices for the line
		vertex.position = start + perpendicular;
		vh.AddVert(vertex);
		vertex.position = start - perpendicular;
		vh.AddVert(vertex);
		vertex.position = end - perpendicular;
		vh.AddVert(vertex);
		vertex.position = end + perpendicular;
		vh.AddVert(vertex);

		// Define the triangles for the line
		int startIndex = vh.currentVertCount - 4;
		vh.AddTriangle(startIndex, startIndex + 1, startIndex + 2);
		vh.AddTriangle(startIndex, startIndex + 2, startIndex + 3);
	}
}





// public class NodePoint {
// 		public float x;
// 		public float y;

// 		public Line up;
// 		public Line down;
// 		public Line left;
// 		public Line right;
// 	}

// 	public class Line {
// 		public NodePoint a;
// 		public NodePoint b;

// 		public Line(NodePoint a, NodePoint b)
// 		{
// 			this.a = a;
// 			this.b = b;
// 		}
// 	}

// 	public class Graph {
// 		public List<NodePoint> nodes = new List<NodePoint>();
// 		public List<Line> vLines = new List<Line>();
// 		public List<Line> hLines = new List<Line>();

// 		public void AddPoint(Vector2 location)
// 		{
// 			NodePoint point = new NodePoint();
// 			point.x = location.x;
// 			point.y = location.y;

// 			Line left = RaycastLeft(location);
// 			if (left != null) {
// 				// Add a new point to split the horizontal line
// 				NodePoint split = new NodePoint();
// 				split.x = location.x;
// 				split.y = left.a.y;

// 				Line left_2 = new Line(split, left.a);
// 				left.a = split;

// 				var (up, down) = left.b.y > left.a.y ? (left.b, left.a) : (left.a, left.b);

// 			}
// 		}

// 		public Line RaycastLeft(Vector2 start)
// 		{
// 			Line closest = null;
// 			float closestDistance = float.MaxValue;

// 			for (int i = 0; i < hLines.Count; i++)
// 			{
// 				Line line = hLines[i];
				
// 				float dist = start.x - line.a.x;
// 				if (dist >= 0)
// 				{
// 					if ((line.a.y <= start.y && line.b.y >= start.y) ||
// 						(line.b.y <= start.y && line.a.y >= start.y))
// 					{
// 						if (dist < closestDistance)
// 						{
// 							closest = line;
// 							closestDistance = dist;
// 						}
// 					}
// 				}
// 			}

// 			return closest;
// 		}

// 		public Line RaycastRight(Vector2 start)
// 		{
// 			Line closest = null;
// 			float closestDistance = float.MaxValue;

// 			for (int i = 0; i < hLines.Count; i++)
// 			{
// 				Line line = hLines[i];
				
// 				float dist = line.a.x - start.x;
// 				if (dist >= 0)
// 				{
// 					if ((line.a.y <= start.y && line.b.y >= start.y) ||
// 						(line.b.y <= start.y && line.a.y >= start.y))
// 					{
// 						if (dist < closestDistance)
// 						{
// 							closest = line;
// 							closestDistance = dist;
// 						}
// 					}
// 				}
// 			}

// 			return closest;
// 		}

// 		public Line RaycastUp(Vector2 start)
// 		{
// 			Line closest = null;
// 			float closestDistance = float.MaxValue;

// 			for (int i = 0; i < vLines.Count; i++)
// 			{
// 				Line line = vLines[i];
				
// 				float dist = line.a.y - start.y;
// 				if (dist >= 0)
// 				{
// 					if ((line.a.x <= start.x && line.b.x >= start.x) ||
// 						(line.b.x <= start.x && line.a.x >= start.x))
// 					{
// 						if (dist < closestDistance)
// 						{
// 							closest = line;
// 							closestDistance = dist;
// 						}
// 					}
// 				}
// 			}

// 			return closest;
// 		}

// 		public Line RaycastDown(Vector2 start)
// 		{
// 			Line closest = null;
// 			float closestDistance = float.MaxValue;

// 			for (int i = 0; i < vLines.Count; i++)
// 			{
// 				Line line = vLines[i];
				
// 				float dist = start.y - line.a.y;
// 				if (dist >= 0)
// 				{
// 					if ((line.a.x <= start.x && line.b.x >= start.x) ||
// 						(line.b.x <= start.x && line.a.x >= start.x))
// 					{
// 						if (dist < closestDistance)
// 						{
// 							closest = line;
// 							closestDistance = dist;
// 						}
// 					}
// 				}
// 			}

// 			return closest;
// 		}
// 	}
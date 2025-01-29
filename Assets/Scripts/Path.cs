using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public struct Path
{
	public GraphNodeMines Mine;
	public GraphNodeBase Base;
	public List<Graph> Graphs;
	public float IncomeProduction;
	public float TotalTime;

	public Path(GraphNodeMines startNode, GraphNodeBase endNode)
	{
		Mine = startNode;
		Base = endNode;
		Graphs = new List<Graph>();
		IncomeProduction = 0;
		TotalTime = 0;
	}

	public Path(Path path)
	{
		Mine = path.Mine;
		Base = path.Base;
		Graphs = new List<Graph>();
		foreach (var graph in path.Graphs)
		{
			Graphs.Add(graph);
		}
		IncomeProduction = path.IncomeProduction;
		TotalTime = path.TotalTime;
	}

	/// <summary>
	/// Вычисление производительности маршрута
	/// </summary>
	/// <param name="train"></param>
	public void CalculatePath(Train train)
	{
		IncomeProduction = Base.GetResourceMultiplier(train);
		TotalTime = 0;
		TotalTime += Mine.GetProduceExtractionTime(train);

		foreach (var graph in Graphs)
		{
			TotalTime += graph.Weight / train.MoveSpeed;
		}
	}
}
[System.Serializable]
public struct GraphPath
{
	public bool PathComplete;
	public bool ErrorPath;
	public GraphNode TargetNode;
	public GraphNode EndNode;
	public List<Graph> Graphs;
	public List<GraphNode> ClosetNodes;

	public GraphPath(GraphPath path)
	{
		PathComplete = path.PathComplete;
		ErrorPath = path.ErrorPath;
		TargetNode = path.TargetNode;
		EndNode = path.EndNode;
		Graphs =new List<Graph>();
		foreach (var pathGraph in path.Graphs)
		{
			Graphs.Add(pathGraph);
		}
		ClosetNodes = new List<GraphNode>();
		foreach (var graphNode in path.ClosetNodes)
		{
			ClosetNodes.Add(graphNode);
		}
	}

	/// <summary>
	/// Добавление новой ноды к маршруту
	/// </summary>
	/// <param name="graph"></param>
	public void AddGraph(Graph graph)
	{
		Graphs.Add(graph);
		if (graph.FirstGraphNode == EndNode)
		{
			EndNode = graph.SecondGraphNode;
		}
		else
		{
			EndNode = graph.FirstGraphNode;
		}
		ClosetNodes.Add(EndNode);

		if (TargetNode == EndNode)
		{
			PathComplete = true;
		}
	}

	/// <summary>
	/// Получение общего веса пути
	/// </summary>
	/// <returns></returns>
	public float GetTotalWeight()
	{
		if (ErrorPath)
		{
			return Mathf.Infinity;
		}
		return Graphs.Sum(graph => graph.Weight);
	}
}

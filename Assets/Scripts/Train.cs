using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Train : MonoBehaviour
{
	public float MoveSpeed=10.0f;
	public float ProductionSpeed = 20.0f;
	public Color TrainColor;
	public LineRenderer CurrentPathLine;
	private Action<Train> _onDestroyTrain;
	private bool _isStop;
	private List<Graph> _path;
	private float _moveProgress;
	private Action<GraphNode> _onEndMove;
	private GraphNode _targetNode;
	

	void OnDestroy()
	{
		if (_onDestroyTrain != null)
		{
			_onDestroyTrain(this);
		}
	}

	void Update()
	{
		TrainAi();
	}

	/// <summary>
	/// инициализаия поезда и присвоение ему начальной точки
	/// </summary>
	/// <param name="spawnNode"></param>
	/// <param name="onDestroy"></param>
	public void Init(GraphNode spawnNode, Action<Train> onDestroy)
	{
		_onDestroyTrain += onDestroy;
		var currentPath = WorldController.Instance.FindTrainTarget(this, spawnNode);
		Move(currentPath.Mine, spawnNode, OnMineEnter);

		var mesh = GetComponentsInChildren<Renderer>();
		foreach (var renderer1 in mesh)
		{
			renderer1.material.color = TrainColor;
		}

		CurrentPathLine.startColor = TrainColor;
		CurrentPathLine.endColor = TrainColor;
	}

	private void TrainAi()
	{
		if (!_isStop)
		{
			ReplaceTrainToPath();
		}
	}

	/// <summary>
	/// Поезд заехал на шахту
	/// </summary>
	/// <param name="node"></param>
	private void OnMineEnter(GraphNode node)
	{
		var mine = node as GraphNodeMines;
		var waitTime = ProductionSpeed * mine.ExtractionTimeMultiplier;
		StartCoroutine(Wait(waitTime, () =>
		{
			var currentPath = WorldController.Instance.FindTrainTarget(this, node);
			Move(currentPath.Base,node,OnBaseEnter);
		}));
	}

	/// <summary>
	/// Поезд заехал на базу
	/// </summary>
	/// <param name="node"></param>
	private void OnBaseEnter(GraphNode node)
	{
		node.OnTrainEnter(this);
		var currentPath = WorldController.Instance.FindTrainTarget(this, node);

		Move(currentPath.Mine,node, OnMineEnter);
	}

	private IEnumerator Wait(float waitTime, Action onEndWait)
	{
		yield return new WaitForSeconds(waitTime);
		onEndWait();
	}

	/// <summary>
	/// Расчет пути до указанной точки
	/// </summary>
	/// <param name="targetNode"></param>
	/// <param name="currentNode"></param>
	/// <param name="onEndMove"></param>
	private void Move(GraphNode targetNode, GraphNode currentNode,Action<GraphNode> onEndMove)
	{
		_path = WorldController.Instance.FindPath(currentNode,targetNode);
		if (_path.Count > 0)
		{
			var nextGraph = _path[0];
			_targetNode = GetNextNode(nextGraph, currentNode);
		
		}
		_onEndMove = onEndMove;
		_isStop = false;
	}

	/// <summary>
	/// Получение противоположной точки графа
	/// </summary>
	/// <param name="graph"></param>
	/// <param name="currentNode"></param>
	/// <returns></returns>
	private GraphNode GetNextNode(Graph graph,GraphNode currentNode)
	{
		if (graph.FirstGraphNode == currentNode)
		{
			return graph.SecondGraphNode;
		}
		return graph.FirstGraphNode;
	}

	/// <summary>
	/// Перемещение поезда 
	/// </summary>
	private void ReplaceTrainToPath()
	{
		if (_path != null&& _path.Count>0)
		{
			UpdatePathLine(_path);
			var position = _path[0];
			Vector3 trainPosition;
			if (_targetNode == position.FirstGraphNode)
			{
				trainPosition = Vector3.Lerp(position.SecondGraphNode.transform.position,
						position.FirstGraphNode.transform.position, _moveProgress);
			}
			else
			{
				trainPosition = Vector3.Lerp(position.FirstGraphNode.transform.position,
						position.SecondGraphNode.transform.position, _moveProgress);
			}
			_moveProgress += (MoveSpeed/position.Weight)*Time.deltaTime;
			transform.position = trainPosition;
			if (_moveProgress >= 1.0f)
			{
				if (_path.Count > 1)
				{
					var nextGraph = _path[1];
					_targetNode = GetNextNode(nextGraph, _targetNode);
				}

				_path.RemoveAt(0);
				_moveProgress = 0.0f;
			}
		}
		else
		{
			_isStop = true;
			if (_onEndMove != null)
			{
				_onEndMove(_targetNode);
			}
			
		}
	}

	private void UpdatePathLine(List<Graph> path)
	{
		CurrentPathLine.positionCount = path.Count+1;
		CurrentPathLine.SetPosition(0,transform.position);
		var nextNode = _targetNode;
		CurrentPathLine.SetPosition(1, nextNode.transform.position);
		if (_path.Count > 1)
		{
			for (int i = 1; i < path.Count; i++)
			{
				var nextGraph = _path[i];
				nextNode = GetNextNode(nextGraph, nextNode);
				CurrentPathLine.SetPosition(i+1,nextNode.transform.position);
			}
		
			
		}
		
	}
}

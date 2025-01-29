using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class WorldController : MonoBehaviour
{
	public static WorldController Instance { get; set; }
	public List<Train> TrainsReference=new List<Train>();
	public int MaxTrainCount=5;
	public float SpawnTrainDelta=5.0f;

	public float TotalScore
	{
		get => _totalScore;
		set
		{
			_totalScore = value;
			if (_onChangeTotalScore != null)
			{
				_onChangeTotalScore((int)_totalScore);
			}
		}
	}
	private float _totalScore;
	private List<GraphNodeBase> _bases=new List<GraphNodeBase>();
	private List<GraphNodeMines> _mines = new List<GraphNodeMines>();
	private List<Graph> _graphs=new List<Graph>();
	private List<Path> _paths=new List<Path>();
	private List<Train> _trains=new List<Train>();

	public static Action<int> _onChangeTotalScore;
	
	void Awake()
    {
	    Instance = this;
    }

	void Start()
	{
		StartCoroutine(LateStart());
	}

    IEnumerator LateStart()
    {
	    yield return new WaitForEndOfFrame();
		OnChangeWorld();
		StartCoroutine(SpawnTrainCoroutine());
	}

    IEnumerator SpawnTrainCoroutine()
    {
	 
	    while (true)
	    {
		    if (_trains.Count < MaxTrainCount)
		    {
			    SpawnTrain();
		    }
			yield return new WaitForSeconds(SpawnTrainDelta);
	    }
	}

    public void AddNode(GraphNode node)
    {
	    node.OnChangeNodeValue += OnChangeWorld;
	    var isBaseNode = node as GraphNodeBase;
	    if (isBaseNode)
	    {
		    _bases.Add(isBaseNode);
			return;
	    }
	    var isMineNode= node as GraphNodeMines;
	    if (isMineNode)
	    {
		    _mines.Add(isMineNode);
		    return;
	    }
	}

    public void AddGraph(Graph graph)
    {
	    _graphs.Add(graph);
	    graph.OnChangeGraphValue += OnChangeWorld;
	}

	private void SpawnTrain()
	{
		var trainRef = TrainsReference[Random.Range(0, TrainsReference.Count)];
		var trainSpawnPosition = _bases[Random.Range(0, _bases.Count)];
		var newTrain = Instantiate(trainRef, trainSpawnPosition.transform.position,
				trainSpawnPosition.transform.rotation);
		newTrain.Init(trainSpawnPosition, OnDestroyTrain);
		_trains.Add(newTrain);
	}

	private void OnDestroyTrain(Train train)
	{
		_trains.Remove(train);
	}

	/// <summary>
	/// Поиск пути между двумя нодами
	/// </summary>
	/// <param name="startGraphNode"></param>
	/// <param name="endGraphNode"></param>
	/// <returns></returns>
    public List<Graph> FindPath(GraphNode startGraphNode, GraphNode endGraphNode)
    {
	    var paths = new List<GraphPath>
	    {
			    new GraphPath()
			    {
						PathComplete = false,
						TargetNode = endGraphNode,
					    EndNode = startGraphNode,
					    Graphs = new List<Graph>(),
					    ClosetNodes = new List<GraphNode>()
					    {
							    startGraphNode
						}
			    }
	    };
	    var allPathCompleted = false;
	    while (!allPathCompleted)
	    {
		    allPathCompleted = true;
		    var pathCount = paths.Count;
			for (var index = 0; index < pathCount; index++)
			{
				var path = paths[index];
				if (path.PathComplete||path.ErrorPath)
				{
					continue;
				}
			
				var connectedGraph = GetConnectedGraph(path.EndNode, path.ClosetNodes);
				if (connectedGraph.Count > 0)
				{
					for (var i = 1; i < connectedGraph.Count; i++)
					{
						var newPath = new GraphPath(path);
						newPath.AddGraph(connectedGraph[i]);
						if (!newPath.PathComplete)
						{
							allPathCompleted = false;
						}

						paths.Add(newPath);
					}

					path.AddGraph(connectedGraph[0]);
				}
				else
				{
					path.ErrorPath = true;
				}

				if (!path.PathComplete||!path.ErrorPath)
				{
					allPathCompleted = false;
				}
				paths[index] = path;
			}
	    }
	    var orderPaths = paths.OrderBy(x => x.GetTotalWeight());
		return orderPaths.FirstOrDefault().Graphs;
    }

	/// <summary>
	/// Пересчет маршрутов при изменении параметров мира
	/// </summary>
    public void OnChangeWorld()
    {
	    _paths=new List<Path>();
	    foreach (var mine in _mines)
	    {
		    foreach (var nodeBase in _bases)
		    {
			    var newPath = new Path(mine, nodeBase)
			    {
					    Graphs = FindPath(mine, nodeBase)
			    };

			    _paths.Add(newPath);
		    }
	    }
	}

	/// <summary>
	/// Поиск наиболее выгодного маршрута для поезда
	/// </summary>
	/// <param name="train"></param>
	/// <param name="trainPosition"></param>
	/// <returns></returns>
    public Path FindTrainTarget(Train train, GraphNode trainPosition)
	{
		var paths = new List<Path>();
		foreach (var path in _paths)
	    {
		    var pathCopy=new Path(path);
			pathCopy.CalculatePath(train);
			
			var pathToStart = FindPath(trainPosition, pathCopy.Mine);
			foreach (var graph in pathToStart)
			{
				pathCopy.TotalTime += graph.Weight / train.MoveSpeed;
			}

			paths.Add(pathCopy);
		}
		
		var trainPath = paths.OrderByDescending(x => x.IncomeProduction).ThenBy(x=>x.TotalTime);
		return trainPath.FirstOrDefault();
	}

	/// <summary>
	/// Поиск графов связанных с нодой
	/// </summary>
	/// <param name="startGraphNode"></param>
	/// <param name="closetNodes"></param>
	/// <returns></returns>
	private List<Graph> GetConnectedGraph(GraphNode startGraphNode,List<GraphNode> closetNodes)
    {
	    var graphs = new List<Graph>();
	    foreach (var graph in _graphs)
	    {
		    if (closetNodes.Contains(graph.FirstGraphNode) && closetNodes.Contains(graph.SecondGraphNode))
		    {
				continue;
		    }

		    if (graph.FirstGraphNode == startGraphNode || graph.SecondGraphNode == startGraphNode)
		    {
				graphs.Add(graph);
		    }
	    }
	    return graphs;
    }
}

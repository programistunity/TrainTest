using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[System.Serializable]
public class Graph:MonoBehaviour
{
	public GraphNode FirstGraphNode;
	public GraphNode SecondGraphNode;
	public float Weight;

	public Action OnChangeGraphValue;
#if UNITY_EDITOR
	[ContextMenu("ShowLine")]
	public  void ShowLine()
	{
		var lineRender = GetComponent<LineRenderer>();
		lineRender.SetPosition(0, FirstGraphNode.transform.position);
		lineRender.SetPosition(1, SecondGraphNode.transform.position);
	}
#endif

	void Start()
	{
		WorldController.Instance.AddGraph(this);
		var lineRender = GetComponent<LineRenderer>();
		lineRender.SetPosition(0,FirstGraphNode.transform.position);
		lineRender.SetPosition(1, SecondGraphNode.transform.position);
	}


	void OnValidate()
	{
		if (OnChangeGraphValue != null)
		{
			OnChangeGraphValue();
		}
	}

}

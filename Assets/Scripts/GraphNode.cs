using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNode :MonoBehaviour
{
	public Action OnChangeNodeValue;

	void Start()
	{
		WorldController.Instance.AddNode(this);
	}

	void OnValidate()
	{
		if (OnChangeNodeValue != null)
		{
			OnChangeNodeValue();
		}
	}

	public virtual float GetProduceExtractionTime(Train train)
	{
		return 0;
	}

	public virtual float GetResourceMultiplier(Train train)
	{
		return 0;
	}

	public virtual void OnTrainEnter(Train train)
	{
	}
}

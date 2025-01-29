using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphNodeMines : GraphNode
{
	public float ExtractionTimeMultiplier = 0.1f;

	
	public override float GetProduceExtractionTime(Train train)
	{
		return ExtractionTimeMultiplier*train.ProductionSpeed;
	}
}

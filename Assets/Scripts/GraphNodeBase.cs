
using UnityEngine;

public class GraphNodeBase :  GraphNode
{
	public float ResourceMultiplier=1.0f;

	public override float GetResourceMultiplier(Train train)
	{
		return ResourceMultiplier;
	}

	public override void OnTrainEnter(Train train)
	{
		base.OnTrainEnter(train);
		WorldController.Instance.TotalScore += ResourceMultiplier;
	}
}

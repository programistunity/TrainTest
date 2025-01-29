using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiController : MonoBehaviour
{
	public TextMeshProUGUI TotalCountText;

	void Start()
	{
		WorldController._onChangeTotalScore += OnChangeTotalScore;
	}

	private void OnChangeTotalScore(int totalScore)
	{
		TotalCountText.text = totalScore.ToString();
	}
}

using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Greyrats.JointAI.Analysis
{
	public class DebugVisualInstance : MonoBehaviour
	{
		public TextMeshProUGUI text;
		public LineRenderer lr;
		public Image colorImage;
		public Gradient imageGradient;
		public Transform leftBotPos;
		public Transform rightTopPos;

		private Queue<float> _values;
		private int _pointsAmount;
		private string _nameValue;
		private float minValue;
		private float maxValue;
		public void Initialize(int pointsAmount, string nameValue)
		{
			_pointsAmount = pointsAmount;
			_values = new Queue<float>(pointsAmount);
			lr.positionCount = pointsAmount;
			for (int i = 0; i < pointsAmount; i++)
			{
				lr.SetPosition(i, rightTopPos.position);
			}
			_nameValue = nameValue;
		}

		public void AddQueueValue(float value)
		{
			if (_values.Count >= _pointsAmount)
			{
				_values.Dequeue();
			}
			_values.Enqueue(value);
			colorImage.color = imageGradient.Evaluate(value);
			minValue = _values.Min();
			maxValue = _values.Max();
			text.text = $"{_nameValue}: {value:F3}|min:{minValue:F3}|max:{maxValue:F3}";
		}

		public void Update()
		{
			int i = 0;
			float count = _values.Count;
			foreach (var tmp in _values)
			{
				float lerpValue = Mathf.InverseLerp(minValue, maxValue, tmp);

				float currentLerp = i / count;
				float tmpDistanceX = Mathf.Lerp(leftBotPos.position.x, rightTopPos.position.x, currentLerp);
				lr.SetPosition(i,
					new Vector3(tmpDistanceX,
						leftBotPos.position.y + (lerpValue),
						Mathf.Lerp(leftBotPos.position.z, rightTopPos.position.z, currentLerp)));
				i++;
			}
		}
	}
}
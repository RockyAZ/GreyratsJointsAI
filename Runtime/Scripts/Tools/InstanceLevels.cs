using UnityEngine;

namespace Greyrats.JointAI.Tools
{
	public class InstanceLevels : MonoBehaviour
	{
		public GameObject level;
		public bool calculateNewPosition;
		public int rows = 1, cols = 1, floors = 50;
		public float distRows = 100, distColumns = 100, distFloors = 10;
		public bool active = true;
		public bool hideRenderers = true;

		void Awake()
		{
			if (gameObject.activeInHierarchy && active)
				CreateLevels();
		}

		private void CreateLevels()
		{
			var reworkedlevel = Instantiate(level,
				new(
					level.transform.position.x + distRows,
					level.transform.position.y + distFloors,
					level.transform.position.z + distColumns),
				Quaternion.identity,
				transform);
			if (hideRenderers)
			{
				foreach (var tmp in reworkedlevel.GetComponentsInChildren<MeshRenderer>())
				{
					tmp.enabled = false;
				}
				foreach (var tmp in reworkedlevel.GetComponentsInChildren<SkinnedMeshRenderer>())
				{
					tmp.enabled = false;
				}
			}

			Vector3 basePos = reworkedlevel.transform.position;
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					for (int k = 0; k < floors; k++)
					{
						if (i == 0 && j == 0 && k == 0)
							continue;
						Vector3 newPos = new(basePos.x + i * distRows,
							basePos.y + k * distFloors,
							basePos.z + j * distColumns);
						Instantiate(reworkedlevel,
							calculateNewPosition ? newPos : reworkedlevel.transform.position,
							Quaternion.identity,
							transform);
					}
				}
			}
		}
	}
}
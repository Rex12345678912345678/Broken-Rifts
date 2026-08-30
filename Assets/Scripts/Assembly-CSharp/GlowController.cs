using UnityEngine;

public class GlowController : MonoBehaviour
{
	public Mesh SupportMesh;

	public Mesh NeutralMesh;

	public Mesh AttackMesh;

	public void SetStateColor(GlowState state)
	{
		switch (state)
		{
		case GlowState.Support:
		{
			var componentsInChildren2 = GetComponentsInChildren<MeshFilter>(true);
			foreach (var meshFilter2 in componentsInChildren2)
			{
				meshFilter2.mesh = SupportMesh;
			}
			break;
		}
		case GlowState.Attack:
		{
			var componentsInChildren3 = GetComponentsInChildren<MeshFilter>(true);
			foreach (var meshFilter3 in componentsInChildren3)
			{
				meshFilter3.mesh = AttackMesh;
			}
			break;
		}
		case GlowState.Neutral:
		{
			var componentsInChildren = GetComponentsInChildren<MeshFilter>(true);
			foreach (var meshFilter in componentsInChildren)
			{
				meshFilter.mesh = NeutralMesh;
			}
			break;
		}
		}
	}
}

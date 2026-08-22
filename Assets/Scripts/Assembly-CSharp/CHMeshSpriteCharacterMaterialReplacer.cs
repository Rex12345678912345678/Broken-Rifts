using UnityEngine;

[RequireComponent(typeof(CHMeshSprite))]
public class CHMeshSpriteCharacterMaterialReplacer : MonoBehaviour
{
	private void Start()
	{
		var component = GetComponent<CHMeshSprite>();
		if (component != null && component.m_AtlasType == AtlasTypes.SmoothMoves && component.m_SmoothMovesAtlas == null)
			return;

		if (component != null) 
			component.UpdateSprite(true);
	}
}

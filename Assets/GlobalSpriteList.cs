using UnityEngine;

public class GlobalSpriteList : SingletonMono<GlobalSpriteList>
{
	public Sprite[] sprites;

	public Sprite Recommended_Sprite;
	public Sprite History_Sprite;
	public Sprite Other_Sprite;

	public static Sprite GetSprite(int index)
	{
#if UNITY_EDITOR
		if (m_instance == null && (m_instance = FindObjectOfType<GlobalSpriteList>()) == null)
		{
			return null;
		}
#endif

		if (index < 0 || index >= Instance.sprites.Length)
		{
			Debug.LogError("GlobalSpriteList index out of range: " + index);
			return null;
		}

		return Instance.sprites[index];
	}

	public static Sprite GetIcon(InputQuickFillIconType type)
	{
		return type switch
		{
			InputQuickFillIconType.Recommended => Instance.Recommended_Sprite,
			InputQuickFillIconType.History => Instance.History_Sprite,
			InputQuickFillIconType.Other => Instance.Other_Sprite,
			_ => null,
		};
	}

	public static int RandomIconIndex => Random.Range(0, Instance.sprites.Length);
}
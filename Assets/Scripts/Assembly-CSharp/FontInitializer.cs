using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FontInitializer : MonoBehaviour
{
	public List<FontSet> m_FontSets;

	private int m_CurrentLanguageIndex;

	private List<UIFont> m_CurrentReplacementFonts;

	public void ReplaceFontsIfNeeded(string languageName, bool checkAssetProviders = true)
	{
		var num = 0;
		var flag = false;
		m_CurrentReplacementFonts = new List<UIFont>();
		foreach (var fontSet in m_FontSets)
		{
			if (fontSet.m_Language == languageName)
			{
				foreach (var fontPair in fontSet.m_FontPairs)
				{
					fontPair.m_Font.replacement = fontPair.m_FontReplacement;
					m_CurrentReplacementFonts.Add(fontPair.m_FontReplacement);
					if (checkAssetProviders && fontPair.m_FontReplacementAssetProvider)
					{
						StartCoroutine(UpdateWithRealFonts(fontPair, languageName, fontPair.m_FontReplacement.name.Replace("_lite", string.Empty)));
					}
				}
				flag = true;
				m_CurrentLanguageIndex = num;
			}
			else
			{
				foreach (var fontPair2 in fontSet.m_FontPairs)
				{
					fontPair2.m_FontReplacement = null;
				}
			}
			num++;
		}
		if (!flag)
		{
			ResetFonts();
		}
	}

	private IEnumerator UpdateWithRealFonts(FontPair fontPair, string languageName, string fontName)
	{
		while (!ContentLoader.Instance || !ContentLoader.Instance.LoadedFonts)
		{
			yield return new WaitForEndOfFrame();
		}
		yield return StartCoroutine(fontPair.m_FontReplacementAssetProvider.InitializeCoroutine());
		while (!fontPair.m_FontReplacementAssetProvider.m_Initialized)
		{
			yield return new WaitForEndOfFrame();
		}
		fontPair.m_FontReplacement = (fontPair.m_FontReplacementAssetProvider.GetObject(fontName) as GameObject).GetComponent<UIFont>();
		ReplaceFontsIfNeeded(languageName, false);
	}

	public void ResetFonts()
	{
		var fontSet = m_FontSets[0];
		foreach (var fontPair in fontSet.m_FontPairs)
		{
			fontPair.m_Font.replacement = null;
		}
		m_CurrentLanguageIndex = 0;
	}

	internal void GoToNextLanguage()
	{
		var index = (m_CurrentLanguageIndex + 1) % m_FontSets.Count;
		ReplaceFontsIfNeeded(m_FontSets[index].m_Language);
	}

	internal void RequestCharacters(string characters)
	{
		DebugLog.Log("Requesting characters: " + characters);
		foreach (var fontPair in m_FontSets[0].m_FontPairs)
		{
			fontPair.m_Font.dynamicFont.RequestCharactersInTexture(characters, fontPair.m_Font.dynamicFont.fontSize, fontPair.m_Font.dynamicFontStyle);
		}
	}
}

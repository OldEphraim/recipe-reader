using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RecipeStep
{
    public int index;
    public string text;
    public string verb;
    public int correctPiece;
    public string displayText;
    public string completedText;
}

[System.Serializable]
public class Recipe
{
    public string id;
    public string name;
    public string difficulty;
    public List<RecipeStep> steps;
}

[System.Serializable]
public class RecipeCollection
{
    public List<Recipe> recipes;

    public static RecipeCollection LoadFromJson(string json)
    {
        return JsonUtility.FromJson<RecipeCollection>(json);
    }

    public static RecipeCollection LoadFromTextAsset(TextAsset asset)
    {
        if (asset == null) return null;
        return LoadFromJson(asset.text);
    }

    public static RecipeCollection LoadFromResources(string resourcePath)
    {
        var asset = Resources.Load<TextAsset>(resourcePath);
        return LoadFromTextAsset(asset);
    }

    public List<Recipe> GetByDifficulty(string difficulty)
    {
        var result = new List<Recipe>();
        if (recipes == null) return result;
        foreach (var r in recipes)
        {
            if (r.difficulty == difficulty) result.Add(r);
        }
        return result;
    }
}

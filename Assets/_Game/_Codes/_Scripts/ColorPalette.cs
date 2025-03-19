using System.Collections.Generic;
using UnityEngine;

public static class ColorPalette
{
    public static readonly Dictionary<Colors, Color> Colors = new Dictionary<Colors, Color>
    {
        { global::Colors.White, Color.white },
        { global::Colors.Red, Color.red },
        { global::Colors.Blue, Color.blue },
        { global::Colors.Green, Color.green },
        { global::Colors.Yellow, Color.yellow },
        { global::Colors.Purple, new Color(0.5f, 0, 0.5f) }, // Mor renk
        { global::Colors.Gray, Color.gray },
    };
}
public static class MaterialHelper
{
    public static Material GetMaterialFromColor(Colors color, LevelEditorDatas levelEditorDatas)
    {
        if (levelEditorDatas == null)
        {
            Debug.LogError("LevelEditorDatas atanmadı! Materyal atanamaz.");
            return null;
        }

        Material baseMaterial = null;

        switch (color)
        {
            case Colors.Blue:
                baseMaterial = levelEditorDatas.blueStickman;
                break;
            case Colors.Yellow:
                baseMaterial = levelEditorDatas.yellowStickman;
                break;
            case Colors.Red:
                baseMaterial = levelEditorDatas.redStickman;
                break;
            case Colors.Purple:
                baseMaterial = levelEditorDatas.purpleStickman;
                break;
            case Colors.Green:
                baseMaterial = levelEditorDatas.greenStick;
                break;
            case Colors.White:
            case Colors.Gray:
                return null; // Beyaz ve Gri için materyal oluşturma
        }

        if (baseMaterial == null)
        {
            Debug.LogWarning($"Materyal bulunamadı: {color}");
            return null;
        }

        Material newMaterial = new Material(baseMaterial);
        return newMaterial;
    }
}




using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

public static class Extensions
{
    public static void Debug(this object obj)
    {
        UnityEngine.Debug.LogError(obj);
    }
    public static void Debug(this Object obj)
    {
        UnityEngine.Debug.LogError(obj, obj);
    }
    public static void Debug(this object obj, Color color)
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        UnityEngine.Debug.LogError($"<color=#{hexColor}><b>{obj}</b></color>");
    }
    public static void Debug(this Object obj, Color color)
    {
        string hexColor = ColorUtility.ToHtmlStringRGB(color);
        UnityEngine.Debug.LogError($"<color=#{hexColor}><b>{obj}</b></color>", obj);
    }
    public static void LocalReset(this Transform transform)
    {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    public static void SetActive(this IList<GameObject> gameObjects, bool active)
    {
        if (gameObjects.IsNullOrEmpty())
        {
            return;
        }

        foreach (var item in gameObjects)
        {
            if (item == null)
            {
                continue;
            }
            item.SetActive(active);
        }
    }

    public static void SetActive(this IList<Component> components, bool active)
    {
        if (components.IsNullOrEmpty())
        {
            return;
        }

        foreach (var item in components)
        {
            if (item == null)
            {
                continue;
            }
            item.SetActive(active);
        }
    }

    public static void SetActive(this Component component, bool active)
    {
        component.gameObject.SetActive(active);
    }

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        if (!gameObject.TryGetComponent<T>(out var component))
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }



    public static bool IsNullOrEmpty(this string text)
    {
        return string.IsNullOrEmpty(text);
    }

    public static bool IsNullOrEmpty<T>(this IList<T> container)
    {
        if (container == null || container.Count == 0)
        {
            return true;
        }

        return false;
    }

    public static bool IsNullOrEmptyOrBare<T>(this IList<T> container)
    {
        if (container.IsNullOrEmpty())
        {
            return true;
        }

        if (container.Count(x => x != null) == 0)
        {
            return true;
        }

        return false;
    }

    public static bool IsNullOrEmpty<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
    {
        if (dictionary == null || dictionary.Count == 0)
        {
            return true;
        }

        return false;
    }

    public static void SetDuration(this ParticleSystem.MainModule main, float duration)
    {
        main.duration = duration;
    }
    public static void SetPlayOnAwake(this ParticleSystem.MainModule main, bool active)
    {
        main.playOnAwake = active;
    }
    public static void SetShapeMesh(this ParticleSystem.ShapeModule shape, Mesh mesh)
    {
        shape.mesh = mesh;
    }
    public static void SetShapeSize(this ParticleSystem.ShapeModule shape, Vector3 size)
    {
        shape.scale = size;
    }
    public static void SetShapeRotation(this ParticleSystem.ShapeModule shape, Vector3 rotation)
    {
        shape.rotation = rotation;
    }
    public static void SetShapeType(this ParticleSystem.ShapeModule shape, ParticleSystemShapeType type)
    {
        shape.shapeType = type;
    }
    public static void SetRateOverTime(this ParticleSystem.EmissionModule emission, float rateOverTime)
    {
        emission.rateOverTime = rateOverTime;
    }
    public static void SetStartSize(this ParticleSystem.MainModule main, ParticleSystem.MinMaxCurve startSize)
    {
        main.startSize = startSize;
    }
    public static void MultiplyStartSize(this ParticleSystem.MainModule main, float sizeMultiplier)
    {
        ParticleSystem.MinMaxCurve particleSize = main.startSize;

        particleSize.constantMin = sizeMultiplier;
        particleSize.constantMax = sizeMultiplier;
        main.startSize = particleSize;
    }
    public static void Multiply(this ParticleSystem.MinMaxCurve minMaxCurve, float sizeMultiplier)
    {
        minMaxCurve.constantMin = sizeMultiplier;
        minMaxCurve.constantMax = sizeMultiplier;
    }
    public static Vector3 SetY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, y, vector.z);
    }
    public static Vector3 SetX(this Vector3 vector, float x)
    {
        return new Vector3(x, vector.y, vector.z);
    }
    public static Vector3 SetZ(this Vector3 vector, float z)
    {
        return new Vector3(vector.x, vector.y, z);
    }
    public static Vector3 AddY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, vector.y + y, vector.z);
    }

    public static Vector3 Multiply(this Vector3 vector, float x)
    {
        return new Vector3(vector.x * x, vector.y * x, vector.z * x);
    }
    public static Vector3 MultiplyX(this Vector3 vector, float x)
    {
        return new Vector3(vector.x * x, vector.y, vector.z);
    }
    public static Vector3 MultiplyY(this Vector3 vector, float y)
    {
        return new Vector3(vector.x, vector.y * y, vector.z);
    }
    public static Vector3 MultiplyZ(this Vector3 vector, float z)
    {
        return new Vector3(vector.x, vector.y, vector.z * z);
    }
    public static Vector2 SetY(this Vector2 vector, float y)
    {
        return new Vector2(vector.x, y);
    }   
    public static Color SetA(this Color color, float a)
    {
        color.a = a;
        return color;
    }
    public static void SetAlpha(this Image image, float a)
    {
        image.color = image.color.SetA(a);
    }

    public static T GetRandomElement<T>(this IList<T> list)
    {
        return list[list.GetRandomIndex()];
    }

    public static int GetRandomIndex<T>(this IList<T> list)
    {
        return Random.Range(0, list.Count);
    }

    public static int GetRandomIndex(this IList<Component> list, bool active)
    {
        List<Component> useables = list.ToList().FindAll(x => x.gameObject.activeSelf == active);
        return list.ToList().IndexOf(useables.GetRandomElement());
    }

    public static T GetRandomElement<T>(this T enumValue) where T : struct
    {
        return enumValue.GetAllElements().GetRandomElement();
    }

    public static T GetRandomElement<T>(this T enumValue, T exception) where T : struct
    {
        var elements = enumValue.GetAllElements();
        elements.Remove(exception);
        return elements.GetRandomElement();
    }

    public static List<T> GetAllElements<T>(this T enumValue) where T : struct
    {
        return System.Enum.GetValues(enumValue.GetType()).OfType<T>().ToList();
    }

    public static T SelectAndRemove<T>(this HashSet<T> hashSet)
    {
        if (hashSet.Count > 0)
        {
            var enumerator = hashSet.GetEnumerator();
            enumerator.MoveNext();
            T element = enumerator.Current;
            hashSet.Remove(element);
            return element;
        }
        return default;
    }

    public static void SetActive(this CanvasGroup canvasGroup, bool active)
    {
        float alpha = active ? 1.0f : 0.0f;
        canvasGroup.alpha = alpha;
        canvasGroup.blocksRaycasts = active;
        canvasGroup.interactable = active;
    }

    public static Tween SetActive(this CanvasGroup canvasGroup, bool active, float duration, bool soundEnable = false)
    {
        float firstAlpha = canvasGroup.alpha;
        canvasGroup.SetActive(active);
        float endAlpha = canvasGroup.alpha;
        canvasGroup.alpha = firstAlpha;
        return canvasGroup.DOFade(endAlpha, duration);
    }

    
    public static float Remap(this float value, float from1, float to1, float from2, float to2)
    {
        return ((value - from1) * (to2 - from2) / (to1 - from1)) + from2;
    }

    public static int LayerValue(this string layerName)
    {
        return LayerMask.GetMask(layerName);
    }
    public static int LayerIndex(this string layerName)
    {
        return LayerMask.NameToLayer(layerName);
    }

    public static Tween DOWeight(this Volume volume, float startWeight, float weight, float duration)
    {
        return DOVirtual.Float(startWeight, weight, duration, x => volume.weight = x);
    }
    public static Tween DOFloat(this FloatParameter floatParameter, float startValue, float value, float duration)
    {
        return DOVirtual.Float(startValue, value, duration, x => floatParameter.value = x);
    }
}

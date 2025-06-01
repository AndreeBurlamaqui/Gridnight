using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Bunch of random utilities, growing by each project
/// </summary>
public static class TinyUtils
{
    public enum FourDirections
    {
        UP,
        RIGHT, CENTER, LEFT,
        DOWN
    }
    public enum EightDirections
    {
        TOP_LEFT, TOP_CENTER, TOP_RIGHT,
        MIDDLE_LEFT, MIDDLE_CENTER, MIDDLE_RIGHT,
        BOTTOM_LEFT, BOTTOM_CENTER, BOTTOM_RIGHT
    }

    public static Vector2 ToVector2(this EightDirections direction)
    {
        return direction switch
        {
            EightDirections.TOP_LEFT => new Vector2(-1, 1),
            EightDirections.TOP_CENTER => new Vector2(0, 1),
            EightDirections.TOP_RIGHT => new Vector2(1, 1),

            EightDirections.MIDDLE_LEFT => new Vector2(-1, 0),
            EightDirections.MIDDLE_CENTER => Vector2.zero,
            EightDirections.MIDDLE_RIGHT => new Vector2(1, 0),

            EightDirections.BOTTOM_LEFT => new Vector2(-1, -1),
            EightDirections.BOTTOM_CENTER => new Vector2(0, -1),
            EightDirections.BOTTOM_RIGHT => new Vector2(1, -1),

            _ => Vector2.zero
        };
    }


    private static Camera _mCam;
    public static Camera MainCamera
    {
        get
        {
            if(_mCam == null) {
                _mCam = Camera.main;
            }
            return _mCam;
        }
    }

    public static void SetAlpha(this SpriteRenderer i, float newAlpha) => i.color = i.color.SetAlpha(newAlpha);

    public static bool DictionaryEquals<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> otherDictionary)
    {
        if(dictionary == otherDictionary)
            return true;

        if(dictionary == null || otherDictionary == null)
            return false;

        if(dictionary.Count != otherDictionary.Count)
            return false;

        foreach(var kvp in dictionary) {
            if(!otherDictionary.TryGetValue(kvp.Key, out TValue value) || !value.Equals(kvp.Value))
                return false;
        }

        return true;
    }

    public static int GetDictionaryHashCode<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        unchecked {
            int hash = 17;
            foreach(var kvp in dictionary) {
                hash = hash * 31 + kvp.Key.GetHashCode();
                hash = hash * 31 + kvp.Value.GetHashCode();
            }
            return hash;
        }
    }

    public static bool ContainsKeyValue<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
    {
        return dictionary.TryGetValue(key, out var dictValue) && dictValue.Equals(value);
    }

    public static void CloseGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }

    #region COLOR CONTROLLER
    public static Color SetAlpha(this Color ogColor, float newAlpha)
    {
        Color cloneColor = ogColor;
        cloneColor.a = newAlpha;

        ogColor = cloneColor;
        return ogColor;
    }

    public static Color SetHSV(this Color color, float hue, float saturation, float value)
    {
        saturation = Mathf.Clamp01(saturation);
        value = Mathf.Clamp01(value);
        hue = Mathf.Clamp01(hue);

        color = Color.HSVToRGB(hue, saturation, value);
        return color;
    }
    public static Color SetSaturationValue(this Color color, float saturation, float value)
    {
        saturation = Mathf.Clamp01(saturation);
        value = Mathf.Clamp01(value);

        Color.RGBToHSV(color, out var curHue, out var curSaturation, out var curValue);
        color = Color.HSVToRGB(curHue, saturation, value);
        return color;
    }

    public static Color SetValue(this Color color, float value)
    {
        value = Mathf.Clamp01(value);

        Color.RGBToHSV(color, out var curHue, out var curSaturation, out var curValue);
        color = Color.HSVToRGB(curHue, curSaturation, value);
        return color;
    }

    public static Color AddValue(this Color color, float amount)
    {
        Color.RGBToHSV(color, out var curHue, out var curSaturation, out var curValue);
        return SetValue(color, curValue + amount);
    }

    public static Color SetSaturation(this Color color, float saturation)
    {
        saturation = Mathf.Clamp01(saturation);

        Color.RGBToHSV(color, out var curHue, out var curSaturation, out var curValue);
        color = Color.HSVToRGB(curHue, saturation, curValue);
        return color;
    }

    #endregion

    public static bool IsLastSibling(this Transform t)
    {
        if(t != null && t.parent != null) {
            return t.GetSiblingIndex() == (t.parent.childCount - 1);
        }
        return false;
    }

    public static bool IsFirstSibling(this Transform t)
    {
        if(t != null && t.parent != null) {
            return t.GetSiblingIndex() == 0;
        }
        return false;
    }

    public static List<T> PopRange<T>(this Stack<T> stack, int amount)
    {
        var result = new List<T>(amount);
        while(amount-- > 0 && stack.Count > 0) {
            if(!stack.TryPop(out T popped)) {
                continue;
            }

            result.Add(popped);
        }
        return result;
    }

    public static void PushRange<T>(this Stack<T> source, IEnumerable<T> collection)
    {
        foreach(var item in collection) {
            source.Push(item);
        }
    }

    #region SIMPLE POOLING

    public static ObjectPool<T> CreateObjectPool<T>(T prefab, Transform parent, int poolSize,
        Action<T> OnGetAction = null, Action<T> OnReleaseAction = null) where T : Component
    {
        return new ObjectPool<T>(
            () => UnityEngine.Object.Instantiate(prefab, parent),
            (go) =>
            {
                go.gameObject.SetActive(true);
                OnGetAction?.Invoke(go);
            },
            (go) =>
            {
                go.gameObject.SetActive(false);
                OnReleaseAction?.Invoke(go);
            },
            (go) =>
            {
                UnityEngine.Object.Destroy(go);
                OnReleaseAction?.Invoke(go);
            },
            false, poolSize, poolSize * 2);
    }

    #endregion

    public static Vector2 GetScreenResolution()
    {
#if UNITY_EDITOR
        // Editor screen size
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        var method = T.GetMethod("GetMainGameViewTargetSize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        if (method != null)
        {
            object result = method.Invoke(null, null);
            return (Vector2)result;
        }
        return new Vector2(Screen.width, Screen.height); // fallback
#else
        // Runtime screen size
        return new Vector2(Screen.width, Screen.height);
#endif
    }


    #region ENUM-RELATED

    private static Dictionary<Type, int> cachedEnumLength = new();

    public static int GetEnumLength<TEnum>() where TEnum : Enum
    {
        Type enumType = typeof(TEnum);
        if(!enumType.IsEnum) {
            return 0;
        }

        if(!cachedEnumLength.TryGetValue(enumType, out var length)) {
            length = Enum.GetNames(enumType).Length;
            cachedEnumLength.Add(enumType, length);
        }

        return length;
    }

    public static TEnum GetRandomEnum<TEnum>() where TEnum : Enum
    {
        int randomValue = UnityEngine.Random.Range(0, GetEnumLength<TEnum>());
        return (TEnum)(object)randomValue;
    }

    #endregion

    public static bool AnyNullValue<T>(this T[] array)
    {
        for(int t = 0; t < array.Length; t++) {
            if(array[t] == null) {
                return true;
            }
        }

        return false;
    }

    public static void EnsureCapacity<T>(ref NativeArray<T> array, int requiredCapacity, Allocator allocator) where T : struct
    {
        if (array.IsCreated && array.Length < requiredCapacity)
        {
            array.Dispose();
            array = new NativeArray<T>(requiredCapacity, allocator);
        }
        else if (!array.IsCreated)
        {
            array = new NativeArray<T>(requiredCapacity, allocator);
        }
    }
}


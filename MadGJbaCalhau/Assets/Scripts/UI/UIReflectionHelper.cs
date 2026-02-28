using System.Reflection;
using UnityEngine;

/// <summary>
/// Utility to set private/serialized fields at runtime (used by UIBuilder).
/// </summary>
public static class UIReflectionHelper
{
    public static void SetPrivate(object obj, string fieldName, object value)
    {
        if (obj == null) return;
        FieldInfo field = obj.GetType().GetField(fieldName,
            BindingFlags.NonPublic | BindingFlags.Instance);
        if (field != null)
            field.SetValue(obj, value);
        else
            Debug.LogWarning($"[UIReflectionHelper] Field '{fieldName}' not found on {obj.GetType().Name}");
    }
}


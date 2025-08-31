using System.IO;
using UnityEditor;
using UnityEngine;

public class MixamoPrefixRemover : AssetPostprocessor
{
    void OnPreprocessModel()
    {
        if (!IsFbx(assetPath)) return;
    }

    void OnPostprocessModel(GameObject g)
    {
        if (!IsFbx(assetPath)) return;
        StripPrefixRecursive(g.transform);
    }

    void OnPostprocessAnimation(GameObject root, AnimationClip clip)
    {
        if (!IsFbx(assetPath)) return;
        if (clip == null) return;

        var floatBindings = AnimationUtility.GetCurveBindings(clip);
        foreach (var binding in floatBindings)
        {
            var newBinding = binding;
            newBinding.path = StripPrefixInPath(binding.path);

            if (newBinding.path == binding.path) continue;

            var curve = AnimationUtility.GetEditorCurve(clip, binding);
            
            AnimationUtility.SetEditorCurve(clip, binding, null);
            AnimationUtility.SetEditorCurve(clip, newBinding, curve);
        }

        var objBindings = AnimationUtility.GetObjectReferenceCurveBindings(clip);
        foreach (var binding in objBindings)
        {
            var newBinding = binding;
            newBinding.path = StripPrefixInPath(binding.path);

            if (newBinding.path == binding.path) continue;

            var curve = AnimationUtility.GetObjectReferenceCurve(clip, binding);
            AnimationUtility.SetObjectReferenceCurve(clip, binding, null);
            AnimationUtility.SetObjectReferenceCurve(clip, newBinding, curve);
        }
    }

    private static bool IsFbx(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".fbx";
    }

    private static readonly string Prefix = "mixamorig:";

    private static void StripPrefixRecursive(Transform t)
    {
        if (t.name.StartsWith(Prefix))
        {
            t.name = t.name.Substring(Prefix.Length);
        }
        for (int i = 0; i < t.childCount; i++)
        {
            StripPrefixRecursive(t.GetChild(i));
        }
    }

    private static string StripPrefixInPath(string path)
    {
        return string.IsNullOrEmpty(path) ? path : path.Replace(Prefix, string.Empty);
    }
}

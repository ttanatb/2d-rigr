using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BoneFinder
{
    // Start is called before the first frame update
    public static Transform[] FindAllBonesWithName(string baseName, Transform root, string incrementFormat = "{0}_{1}",
        int startingIndex = 0)
    {
        if (baseName.Length == 0) return null;

        List<Transform> bones = new List<Transform>();

        // given "{0}_{1}", eyelid_r, 0 -> return [eyelid_r_0 - eyelid_r_n]
        string baseBoneName = string.Format(incrementFormat, baseName, startingIndex);


        // TODO: dear god i'm using gameobject.find
        Transform res = FindBoneFromRoot(baseBoneName, root);
        if (res == null)
        {
            Debug.LogErrorFormat("Unable to find bone in the format {0} under {1}", baseBoneName, root);
            return null;
        }

        var parent = res.parent;
        int currIndex = startingIndex;
        for (int i = 0; i < parent.childCount; i++)
        {
            string name = string.Format(incrementFormat, baseName, currIndex);
            var child = parent.GetChild(i);

            if (child.name == name)
            {
                bones.Add(child);
                currIndex += 1;
                i = -1;
                continue;
            }
        }

        string nameOfNext = string.Format(incrementFormat, baseName, currIndex + 1);
        if (parent.Find(nameOfNext) != null)
        {
            Debug.LogErrorFormat("Gap in bone naming: {0}", nameOfNext);
            // TODO handle this error better than just logging it maybe or who cares
        }

        return bones.ToArray();
    }

    public static Transform FindBoneWithName(string name, Transform root)
    {
        if (name.Length == 0) return null;

        Transform res = FindBoneFromRoot(name, root);
        if (res == null)
        {
            Debug.LogErrorFormat("Unable to find bone in the format {0} under {1}",
                name, root);
            return res;
        }

        return res;
    }


    private static Transform FindBoneFromRoot(string boneName, Transform root)
    {
        if (boneName.Length == 0) return null;

        // TODO: this isn't runtime friendly bc of how much it allocates for the
        // GC
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            Transform curr = queue.Dequeue();
            if (curr.name == boneName) return curr;

            for (int i = 0; i < curr.childCount; i++)
                queue.Enqueue(curr.GetChild(i));
        }
        return null;
    }
    public static Transform FindBoneWithPrefix(string name, Transform root)
    {
        if (name.Length == 0) return null;

        Transform res = FindBoneFromRootWithPrefix(name, root);
        if (res == null)
        {
            Debug.LogErrorFormat("Unable to find bone in the format {0} under {1}",
                name, root);
            return res;
        }

        return res;
    }


    private static Transform FindBoneFromRootWithPrefix(string boneName, Transform root)
    {
        if (boneName.Length == 0) return null;

        // TODO: this isn't runtime friendly bc of how much it allocates for the
        // GC
        Queue<Transform> queue = new Queue<Transform>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            Transform curr = queue.Dequeue();
            if (curr.name.StartsWith(boneName)) return curr;

            for (int i = 0; i < curr.childCount; i++)
                queue.Enqueue(curr.GetChild(i));
        }
        return null;
    }
}

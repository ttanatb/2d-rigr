using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.U2D.Animation;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class BoneRealignmentSurgery : MonoBehaviour
{
    [SerializeField] private Transform m_rootBone = null;
    [SerializeField] private StringTransformDictionary m_nameToBone = null;

    private TransformNode m_originalRot;

    [Button()]
    private void CreateNodes()
    {
        m_originalRot = CreateTransformNode(m_rootBone);
        m_nameToBone = new StringTransformDictionary();

        var stack = new Stack<TransformNode>();
        stack.Push(m_originalRot);
        m_nameToBone.Add(m_originalRot.Transform.name, m_originalRot.Transform);

        while (stack.Count > 0)
        {
            var curr = stack.Pop();

            for (int i = 0; i < curr.Children.Length; i++)
            {
                var child = curr.Transform.GetChild(i);
                curr.Children[i] = CreateTransformNode(child);
                stack.Push(curr.Children[i]);
                m_nameToBone.Add(curr.Children[i].Transform.name, curr.Children[i].Transform);
            }
        }

        Debug.Log("Created orig tree");
    }

    [NaughtyAttributes.Button()]
    private void Realign()
    {
        StopAllCoroutines();
        StartCoroutine(RealignCoroutine());
    }

    private IEnumerator RealignCoroutine()
    {
        // 1 - disable all sprite skins
        var spriteSkins = GetComponentsInChildren<SpriteSkin>();
        foreach (var spriteSkin in spriteSkins)
        {
            spriteSkin.enabled = false;
        }

        var stack = new Stack<Transform>();
        var workingQueue = new Queue<Transform>();

        stack.Push(m_rootBone);
        for (int i = 0; i < m_rootBone.childCount; i++)
        {
            workingQueue.Enqueue(m_rootBone.GetChild(i));
        }

        while (workingQueue.Count > 0)
        {
            var current = workingQueue.Dequeue();
            stack.Push(current);
            for (int i = 0; i < current.childCount; i++)
            {
                workingQueue.Enqueue(current.GetChild(i));
            }
        }
        FlattenBones();

        // Debug.Log("Stack should now be leaf -> root");
        // yield return new WaitForEndOfFrame();

        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in renderers)
        {
            var bones = spriteRenderer.sprite.GetBones();
            for (int i = 0; i < bones.Length; i++)
            {
                var bone = bones[i];
                if (!m_nameToBone.Contains(bone.name)) {
                    Debug.LogError($"Unable to find {bone.name}");
                    continue;
                }
                var boneTransform = m_nameToBone[bone.name];

                // bones[i].rotation = boneTransform.localRotation;
                bones[i].rotation *= GetClosestRotation(bones[i].rotation.eulerAngles.z);
            }
            spriteRenderer.sprite.SetBones(bones);
        }

        while (stack.Count > 0)
        {
            var curr = stack.Pop();

            float currAngle = curr.localRotation.eulerAngles.z;
            curr.rotation *= GetClosestRotation(currAngle);
        }

        UnflattenBones();

        // yield return new WaitForSeconds(1);

        foreach (var spriteSkin in spriteSkins)
        {
            spriteSkin.enabled = true;
        }

        yield return null;
    }

    [Button]
    private void LogBones()
    {
        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var spriteRenderer in renderers)
        {
            var bones = spriteRenderer.sprite.GetBones();
            foreach (var bone in bones)
            {
                // if (!m_nameToBone.Contains(bone.name)) {
                // Debug.LogError($"Unable to find {bone.name}");
                // continue;
                // }
                // var boneTransform = m_nameToBone[bone.name];

                // bones[i].rotation = boneTransform.localRotation;
                // bones[i].rotation *= GetClosestRotation(bones[i].rotation.eulerAngles.z);
                Debug.Log($"({spriteRenderer.name}): Bone ({bone.name}), Rot: {bone.rotation.eulerAngles}");
            }
            // spriteRenderer.sprite.SetBones(bones);
        }
    }

    private Quaternion GetClosestRotation(float currAngle)
    {
        // float currAngle = curr.localRotation.eulerAngles.z;
        if (currAngle < -360 || currAngle > 360)
            currAngle %= 360;

        int floor = 90 * (currAngle > 0 ? (int)currAngle / 90        : (int)(currAngle - 90) / 90);
        int ceil =  90 * (currAngle > 0 ? (int)(currAngle + 90) / 90 : (int)currAngle / 90);
        int closest = Mathf.Abs(ceil - currAngle) < Mathf.Abs(floor - currAngle) ? ceil : floor;

        // string title = $"Align {curr.name}?";
        // string msg = $"Aligning it will get it from {currAngle} to {closest}";

        // Selection.SetActiveObjectWithContext(curr, curr);
        // Selection.activeObject = curr;
        // yield return new WaitForEndOfFrame();

        // int dialogueChoice = EditorUtility.DisplayDialogComplex(title, msg, "yes, align them bones",
        //     "stop i've made mistakes", "mama, skip this");
        //
        // if (dialogueChoice == 0) // ok
        //         curr.Rotate(Vector3.forward, closest - currAngle);
        // else if (dialogueChoice == 1) //cancel
        //     break;

        // if (Mathf.Abs(closest - currAngle) < 15.0f)
            // curr.Rotate(Vector3.forward, closest - currAngle);

        return Mathf.Abs(closest - currAngle) < 15.0f ?
            Quaternion.AngleAxis(closest - currAngle, Vector3.forward) : Quaternion.identity;
    }

    [Button()]
    private void FlattenBones()
    {
        var stack = new Stack<Transform>();
        stack.Push(m_rootBone);

        while (stack.Count > 0)
        {
            var curr = stack.Pop();
            curr.SetParent(null,true);

            for (int i = 0; i < curr.childCount; i++)
                stack.Push(curr.GetChild(i));
        }
    }

    [Button()]
    private void UnflattenBones()
    {
        m_rootBone.SetParent(transform, true);

        var stack = new Stack<TransformNode>();
        stack.Push(m_originalRot);

        while (stack.Count > 0)
        {
            var curr = stack.Pop();
            foreach (var child in curr.Children)
            {
                child.Transform.SetParent(curr.Transform, true);
                stack.Push(child);
            }
        }
    }

    private static TransformNode CreateTransformNode(Transform transformObj)
    {
        return new TransformNode(
            new TransformNode[transformObj.childCount], transformObj.localRotation.eulerAngles.z, transformObj);
    }

    [Button]
    private void ResetBones()
    {
        var spriteSkins = GetComponentsInChildren<SpriteSkin>();
        foreach (var spriteSkin in spriteSkins)
        {
            spriteSkin.enabled = false;
        }

        var stack = new Stack<TransformNode>();
        stack.Push(m_originalRot);

        while (stack.Count > 0)
        {
            var curr = stack.Pop();
            curr.Transform.localRotation = Quaternion.Euler(0,0,curr.ZAngle);

            foreach(var child in curr.Children)
                stack.Push(child);
        }

        Debug.Log("Bones have been reset");
        foreach (var spriteSkin in spriteSkins)
        {
            spriteSkin.enabled = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private struct TransformNode
    {
        public float ZAngle;
        public TransformNode[] Children;
        public Transform Transform;

        public TransformNode(TransformNode[] children, float zAngle, Transform transform)
        {
            Children = children;
            ZAngle = zAngle;
            Transform = transform;
        }
    }
}



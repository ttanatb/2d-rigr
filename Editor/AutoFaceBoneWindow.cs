using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using Rigr.Utils;
using UnityEditor.Presets;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Rigr.Editor
{
    public class AutoFaceBoneWindow : EditorWindow
    {
        private readonly struct Bone
        {
            private readonly string m_name;
            private readonly float m_length;
            private readonly Transform m_transform;

            public Bone(Transform transform)
            {
                m_name = transform.name;
                m_length = BONE_LENGTH;
                m_transform = transform;
            }

            public void AddToStream(YamlSequenceNode sequence, IReadOnlyDictionary<Transform, int> transformToId, Vector2
            documentBounds)
            {
                ShiftPosition(documentBounds);
                sequence.Add(ToNode(transformToId));
            }

            private YamlMappingNode ToNode(IReadOnlyDictionary<Transform, int> transformToId)
            {
                var localPos = m_transform.localPosition;
                var pos = new YamlMappingNode {
                    {
                        "x", $"{localPos.x}"
                    },
                    {
                        "y", $"{localPos.y}"
                    },
                    {
                        "z", $"{localPos.z}"
                    }
                };

                var localRot = m_transform.localRotation;
                var rot = new YamlMappingNode
                {
                    {
                        "x", $"{localRot.x}"
                    },
                    {
                        "y", $"{localRot.y}"
                    },
                    {
                        "z", $"{localRot.z}"
                    },
                    {
                        "w", $"{localRot.w}"
                    }
                };
                var parent = m_transform.parent;
                int parentId = -1;
                if (parent != null && transformToId.ContainsKey(parent))
                    parentId = transformToId[parent];

                return new YamlMappingNode
                {
                    {
                        "name", m_name
                    },
                    {
                        "position", pos
                    },
                    {
                        "rotation", rot
                    },
                    {
                        "length", $"{m_length}"
                    },
                    {
                        "parentId", $"{parentId}"
                    }
                };
            }

            private void ShiftPosition(Vector2 documentBounds)
            {
                var pos = m_transform.localPosition;
                pos *= SCALE;
                m_transform.localPosition = pos;
            }
        }

        private readonly struct GameObjectFactory
        {
            private readonly IDictionary<Transform, int> m_idToTransform;
            private readonly IReadOnlyDictionary<string, SpriteRenderer> m_spriteDict;
            private readonly BoneConfigSO m_boneConfigSo;
            private readonly Vector3 m_offset;

            public GameObjectFactory(IDictionary<Transform, int> idToTransform,
                IReadOnlyDictionary<string, SpriteRenderer> spriteDict, BoneConfigSO boneConfigSo,
                Vector3 offset)
            {
                m_idToTransform = idToTransform;
                m_spriteDict = spriteDict;
                m_boneConfigSo = boneConfigSo;
                m_offset = offset;
            }

            public Transform CreateGameObject(BoneConfigSO.BoneName boneName,
                BoneConfigSO.Side boneSide = BoneConfigSO.Side.Invalid,
                BoneConfigSO.SpriteName spriteName = BoneConfigSO.SpriteName.Invalid,
                BoneConfigSO.Side spriteSide = BoneConfigSO.Side.Invalid,
                BoundsExtension.XSide xSide = BoundsExtension.XSide.Center, BoundsExtension.YSide ySide = BoundsExtension.YSide.Center,
                Transform parent = null)
            {
                return CreateGameObjectCommon(m_boneConfigSo.GetName(boneName, boneSide), spriteName, spriteSide, xSide,
                    ySide, parent);
            }

            public Transform[] CreateGameObjects(BoneConfigSO.BoneName boneName, BoneConfigSO.Side boneSide,
                int count, int frontOffset, int backOffset,
                BoneConfigSO.SpriteName spriteName = BoneConfigSO.SpriteName.Invalid,
                BoneConfigSO.Side spriteSide = BoneConfigSO.Side.Invalid,
                BoundsExtension.XSide xSide = BoundsExtension.XSide.Varied, BoundsExtension.YSide ySide = BoundsExtension.YSide.Center,
                Transform parent = null)
            {
                var results = new Transform[count];
                for (int i = 0; i < count; i++)
                {
                    string name = string.Format(m_boneConfigSo.IncrementFormat,
                        m_boneConfigSo.GetName(boneName, boneSide), i);
                    results[i] =  CreateGameObjectCommon(name, spriteName, spriteSide, xSide, ySide, parent,
                        i, count, frontOffset, backOffset);
                }
                return results;
            }

            private Transform CreateGameObjectCommon(string boneName,
                BoneConfigSO.SpriteName spriteName,
                BoneConfigSO.Side spriteSide,
                BoundsExtension.XSide xSide,
                BoundsExtension.YSide ySide,
                Transform parent,
                int index = -1, int count = 1,
                int frontOffset = 0, int backOffset = 0)
            {
                new GameObject(boneName).TryGetComponent(out Transform transform);

                if (parent != null)
                    transform.SetParent(parent, false);

                m_idToTransform.Add(transform, m_idToTransform.Count);
                transform.position = m_offset;

                if (spriteName == BoneConfigSO.SpriteName.Invalid)
                    return transform;

                string spriteNameStr = m_boneConfigSo.GetName(spriteName, spriteSide);
                if (!m_spriteDict.ContainsKey(spriteNameStr))
                    return transform;

                var renderer = m_spriteDict[spriteNameStr];
                var bounds = renderer.bounds;
                var spriteShape = ySide == BoundsExtension.YSide.CenterNoAlpha ? CurveUtils.CreateSmoothCurve(renderer)
                    : new SpriteShape();
                transform.position += bounds.AdjacentToBounds(
                    ySide, xSide, index, count, frontOffset, backOffset, spriteShape);
                return transform;
            }
        }

        public GameObject m_sprite;
        public BoneConfigSO m_boneConfigSo;

        private Dictionary<string, YamlNode> m_nameToSpriteNode = null;
        private Dictionary<string, SpriteRenderer> m_nameToSpriteRenderer = null;

        private const float BONE_LENGTH = 50;
        private const float SCALE = 100;

        // Add menu named "My Window" to the Window menu
        [MenuItem("Window/Auto Face Bone")]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            var window = (AutoFaceBoneWindow)EditorWindow.GetWindow(typeof(AutoFaceBoneWindow));
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Auto Face Bone Maker", EditorStyles.boldLabel);
            m_sprite = (GameObject)EditorGUILayout.ObjectField(new GUIContent("Sprite"), m_sprite, typeof(GameObject),
                false);
            m_boneConfigSo = (BoneConfigSO)EditorGUILayout.ObjectField(new GUIContent("ConfigSO"), m_boneConfigSo,
                typeof(BoneConfigSO), false);

            if (GUILayout.Button("Auto Face Make"))
            {
                GenerateBones($"{AssetDatabase.GetAssetPath(m_sprite)}.meta",
                    $"{AssetDatabase.GetAssetPath(m_sprite)}_copy");

            }
        }

        private void GetData(YamlMappingNode root, out Vector2 docBounds, out
            Dictionary<string, YamlNode> nameToSpriteNode, out YamlSequenceNode bonesYaml)
        {

            var scriptedImporter = root.Children[new YamlScalarNode("ScriptedImporter")];
            var rigSpriteImportData = (YamlSequenceNode)scriptedImporter[new YamlScalarNode("rigSpriteImportData")];
            var characterData = scriptedImporter[new YamlScalarNode("characterData")];
            bonesYaml = (YamlSequenceNode)characterData[new YamlScalarNode("bones")];

            var documentBounds = scriptedImporter[new YamlScalarNode("documentSize")];
            docBounds = GetDocumentBounds(documentBounds);
            nameToSpriteNode = new Dictionary<string, YamlNode>();
        }

        private void GenerateBones(string path, string copyPath)
        {
            var reader = new StreamReader(path);
            var yaml = new YamlStream();
            yaml.Load(reader);
            reader.Close();

            // save a copy
            var copyWriter = new StreamWriter(copyPath);
            yaml.Save(copyWriter, false);

            var mapping = (YamlMappingNode)yaml.Documents[0].RootNode;
            GetData(mapping, out var docBounds, out m_nameToSpriteNode, out YamlSequenceNode bonesYaml);

            m_nameToSpriteRenderer = new Dictionary<string, SpriteRenderer>();
            for (int i = 0; i < m_sprite.transform.childCount; i++)
            {
                m_sprite.transform.GetChild(i).TryGetComponent(out SpriteRenderer sprite);

                if (sprite == null) continue;
                m_nameToSpriteRenderer.Add(sprite.name, sprite);
            }

            bonesYaml.Children.Clear();

            var transformToId = new Dictionary<Transform, int>();
            var rootGameObj = CreateTransforms(transformToId, m_nameToSpriteRenderer, m_boneConfigSo, docBounds);


            var bones = new List<Bone>(transformToId.Count);
            PopulateBoneList(transformToId, bones, rootGameObj);

            foreach (var bone in bones)
            {
                bone.AddToStream(bonesYaml, transformToId, docBounds);
            }

            var writer = new StreamWriter(path);
            yaml.Save(writer, false);

            writer.Close();
            copyWriter.Close();
        }

        private static Transform CreateTransforms(IDictionary<Transform, int> transformToId,
            IReadOnlyDictionary<string, SpriteRenderer> spriteDict, BoneConfigSO boneConfigSo, Vector2 docBounds)
        {
            var factory = new GameObjectFactory(transformToId, spriteDict, boneConfigSo,
                Vector3.right * docBounds.x / 2.0f / SCALE);

            var root = factory.CreateGameObject(BoneConfigSO.BoneName.Neck);
            root.Rotate(Vector3.forward, 90.0f);

            var head = factory.CreateGameObject(BoneConfigSO.BoneName.Head,
                BoneConfigSO.Side.Invalid, BoneConfigSO.SpriteName.Face,
                BoneConfigSO.Side.Invalid, BoundsExtension.XSide.Center, BoundsExtension.YSide.Bottom, root);

            var lCheek = factory.CreateGameObject(BoneConfigSO.BoneName.Cheek, BoneConfigSO.Side.Left,
                BoneConfigSO.SpriteName.Face, BoneConfigSO.Side.Invalid, BoundsExtension.XSide.FifthPercentile,
                BoundsExtension.YSide.Center, root);
            var rCheek = factory.CreateGameObject(BoneConfigSO.BoneName.Cheek, BoneConfigSO.Side.Right,
                BoneConfigSO.SpriteName.Face,BoneConfigSO.Side.Invalid, BoundsExtension.XSide.NinetyFifthPercentile,
                BoundsExtension.YSide.Center, root);

            var lEar = factory.CreateGameObject(BoneConfigSO.BoneName.Ear, BoneConfigSO.Side.Left,
                BoneConfigSO.SpriteName.Ear, BoneConfigSO.Side.Left, BoundsExtension.XSide.MidLeft,
                BoundsExtension.YSide.Center, root);
            var rEar = factory.CreateGameObject(BoneConfigSO.BoneName.Ear, BoneConfigSO.Side.Right,
                BoneConfigSO.SpriteName.Ear,BoneConfigSO.Side.Right, BoundsExtension.XSide.MidRight,
                BoundsExtension.YSide.Center, root);

            var lHairBg = factory.CreateGameObject(BoneConfigSO.BoneName.HairBg, BoneConfigSO.Side.Left,
                BoneConfigSO.SpriteName.HairBg, BoneConfigSO.Side.Invalid, BoundsExtension.XSide.Left,
                BoundsExtension.YSide.Center, root);
            var rHairBg = factory.CreateGameObject(BoneConfigSO.BoneName.HairBg, BoneConfigSO.Side.Right,
                BoneConfigSO.SpriteName.HairBg,BoneConfigSO.Side.Invalid, BoundsExtension.XSide.Right,
                BoundsExtension.YSide.Center, root);

            var hairBase = factory.CreateGameObject(BoneConfigSO.BoneName.HairCenter, BoneConfigSO.Side.Invalid,
                BoneConfigSO.SpriteName.HairBase, BoneConfigSO.Side.Invalid, BoundsExtension.XSide.Center,
                BoundsExtension.YSide.Center, root);
            var hairAcc = factory.CreateGameObject(BoneConfigSO.BoneName.HairAcc, BoneConfigSO.Side.Invalid,
                BoneConfigSO.SpriteName.HairAcc,BoneConfigSO.Side.Invalid, BoundsExtension.XSide.Center,
                BoundsExtension.YSide.Center, root);

            var face = factory.CreateGameObject(BoneConfigSO.BoneName.Face, BoneConfigSO.Side.Invalid,
                BoneConfigSO.SpriteName.Face, BoneConfigSO.Side.Invalid, BoundsExtension.XSide.Center,
                BoundsExtension.YSide.Center, head);

            var mouthRoot = factory.CreateGameObject(BoneConfigSO.BoneName.Mouth, BoneConfigSO.Side.Invalid,
                BoneConfigSO.SpriteName.Mouth, BoneConfigSO.Side.Invalid, BoundsExtension.XSide.Center,
                BoundsExtension.YSide.Center, face);
            var lipLeft = factory.CreateGameObject(BoneConfigSO.BoneName.Lip, BoneConfigSO.Side.Left,
                BoneConfigSO.SpriteName.Mouth, BoneConfigSO.Side.Invalid, BoundsExtension.XSide.Left,
                BoundsExtension.YSide.Center, mouthRoot);
            var lipRight = factory.CreateGameObject(BoneConfigSO.BoneName.Lip, BoneConfigSO.Side.Right,
                BoneConfigSO.SpriteName.Mouth, BoneConfigSO.Side.Invalid, BoundsExtension.XSide.Right,
                BoundsExtension.YSide.Center, mouthRoot);

            var upperLips = factory.CreateGameObjects(BoneConfigSO.BoneName.UpperLip, BoneConfigSO.Side.Invalid,
                boneConfigSo.LipBoneCount, 1, 1,
                BoneConfigSO.SpriteName.UpperLip, BoneConfigSO.Side.Invalid, BoundsExtension.XSide.Varied,
                BoundsExtension.YSide.Bottom, mouthRoot);
            var lowerLips = factory.CreateGameObjects(BoneConfigSO.BoneName.LowerLip,BoneConfigSO.Side.Invalid,
                boneConfigSo.LipBoneCount, 1, 1,
                BoneConfigSO.SpriteName.LowerLip, BoneConfigSO.Side.Invalid, BoundsExtension.XSide.Varied,
                BoundsExtension.YSide.Top, mouthRoot);
            foreach (var bone in lowerLips)
            {
                bone.Rotate(Vector3.forward, 180);
            }

            var nose = factory.CreateGameObject(BoneConfigSO.BoneName.Nose, BoneConfigSO.Side.Invalid,
                BoneConfigSO.SpriteName.Nose, BoneConfigSO.Side.Invalid, BoundsExtension.XSide.Center,
                BoundsExtension.YSide.Center, face);

            var lEyeRoot = factory.CreateGameObject(BoneConfigSO.BoneName.Eye, BoneConfigSO.Side.Left,
                BoneConfigSO.SpriteName.WhiteEye, BoneConfigSO.Side.Left, BoundsExtension.XSide.Center,
                BoundsExtension.YSide.Center, face);
            var lEyeball = factory.CreateGameObject(BoneConfigSO.BoneName.Eyeball, BoneConfigSO.Side.Left,
                BoneConfigSO.SpriteName.Eyeball, BoneConfigSO.Side.Left, BoundsExtension.XSide.Center,
                BoundsExtension.YSide.Center, lEyeRoot);

            var lEyelids = factory.CreateGameObjects(BoneConfigSO.BoneName.Eyelid, BoneConfigSO.Side.Left,
                boneConfigSo.EyelidBoneCount,0, 0,
                BoneConfigSO.SpriteName.EyelidBorder, BoneConfigSO.Side.Left, BoundsExtension.XSide.Varied,
                BoundsExtension.YSide.Bottom, lEyeRoot);

            var lEyebrow = factory.CreateGameObject(BoneConfigSO.BoneName.Eyebrow, BoneConfigSO.Side.Left,
                BoneConfigSO.SpriteName.Eyebrow, BoneConfigSO.Side.Left, BoundsExtension.XSide.Center,
                BoundsExtension.YSide.Center, lEyeRoot);

            var lEyebrows = factory.CreateGameObjects(BoneConfigSO.BoneName.Eyebrow, BoneConfigSO.Side.Left,
                boneConfigSo.EyebrowBoneCount, 0, 0,
                BoneConfigSO.SpriteName.Eyebrow, BoneConfigSO.Side.Left, BoundsExtension.XSide.Varied,
                BoundsExtension.YSide.CenterNoAlpha, lEyebrow);

            var rEyeRoot = factory.CreateGameObject(BoneConfigSO.BoneName.Eye, BoneConfigSO.Side.Right,
                BoneConfigSO.SpriteName.WhiteEye, BoneConfigSO.Side.Right, BoundsExtension.XSide.Center,
                BoundsExtension.YSide.Center, face);
            var rEyeball = factory.CreateGameObject(BoneConfigSO.BoneName.Eyeball, BoneConfigSO.Side.Right,
                BoneConfigSO.SpriteName.Eyeball, BoneConfigSO.Side.Right, BoundsExtension.XSide.Center,
                BoundsExtension.YSide.Center, rEyeRoot);

            var rEyelids = factory.CreateGameObjects(BoneConfigSO.BoneName.Eyelid,  BoneConfigSO.Side.Right,
                boneConfigSo.EyelidBoneCount, 0, 0,
                BoneConfigSO.SpriteName.EyelidBorder, BoneConfigSO.Side.Right, BoundsExtension.XSide.Varied,
                BoundsExtension.YSide.Bottom, rEyeRoot);
            var rEyebrow = factory.CreateGameObject(BoneConfigSO.BoneName.Eyebrow, BoneConfigSO.Side.Right,
                BoneConfigSO.SpriteName.Eyebrow, BoneConfigSO.Side.Right, BoundsExtension.XSide.Center,
                BoundsExtension.YSide.Center, rEyeRoot);

            var rEyebrows = factory.CreateGameObjects(BoneConfigSO.BoneName.Eyebrow, BoneConfigSO.Side.Right,
                boneConfigSo.EyebrowBoneCount, 0, 0,
                BoneConfigSO.SpriteName.Eyebrow, BoneConfigSO.Side.Right, BoundsExtension.XSide.Varied,
                BoundsExtension.YSide.CenterNoAlpha, rEyebrow);

            return root;
        }

        private void PopulateBoneList(IReadOnlyDictionary<Transform, int> transformToId, List<Bone> bones,
            Transform rootGameObj)
        {
            for (int i = 0; i < transformToId.Count; i++)
            {
                bones.Add(new Bone());
            }

            foreach (var pair in transformToId)
            {
                bones[pair.Value] = new Bone(pair.Key);
            }
        }

        private Vector2 GetDocumentBounds(YamlNode node)
        {
            var xNode = (YamlScalarNode)node[new YamlScalarNode("x")];
            var yNode = (YamlScalarNode)node[new YamlScalarNode("y")];
            return  new Vector2(float.Parse(xNode.Value), float.Parse(yNode.Value));
        }
    }
}

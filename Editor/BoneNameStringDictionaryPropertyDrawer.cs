using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BoneNameStringDictionary))]
public class BoneDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }

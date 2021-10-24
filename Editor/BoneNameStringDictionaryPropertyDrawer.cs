using UnityEditor;



namespace Rigr.Editor
{
    [CustomPropertyDrawer(typeof(BoneNameStringDictionary))]
    public class BoneDictionaryPropertyDrawer : SerializableDictionaryPropertyDrawer { }
}

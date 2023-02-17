using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BundleEditor : MonoBehaviour
{
    public static string ABCONFIGPATH = "Assets/Scripts/Editor/ABConfig.asset";
    [MenuItem(("Tools/Build AssetBundle"))]
    public static void Build()
    {
        ABConfig abConfig = AssetDatabase.LoadAssetAtPath<ABConfig>(ABCONFIGPATH);
        
    }
}

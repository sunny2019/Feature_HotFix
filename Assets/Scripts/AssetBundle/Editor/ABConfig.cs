using System;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;

[CreateAssetMenu(fileName = "ABConfig", menuName = "CreateABConfig", order = 0)]
public class ABConfig : ScriptableObject
{
    [Tooltip("单个文件所在文件夹路径，会遍历这个文件夹下面所有Prefab，所有的Prefab的名字不能重复，必须保证名字的唯一性")]
#if ODIN_INSPECTOR
    [FolderPath]
#endif
    public string[] m_AllPrefabPath;

    public FileDirABName[] m_AllFileDirAB;


    [Serializable]
    public class FileDirABName
    {
        public string ABName;
#if ODIN_INSPECTOR
        void SetDefaultABName()
        {
            if (string.IsNullOrEmpty(Path))
                return;
            ABName = System.IO.Path.GetFileName(Path).ToLower();
        }
        
        [FolderPath]
        [OnValueChanged("SetDefaultABName")]
#endif
        public string Path;
    }
}
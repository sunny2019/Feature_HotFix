using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ex;
using UnityEngine;

public class ResouceTest : MonoBehaviour
{
#if UNITY_EDITOR
    // Start is called before the first frame update
    void Start()
    {
        TestLoadAB();
    }

    void TestLoadAB()
    {
        AssetBundle configAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/assetbundleconfig");
        TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetBundleConfig");
        AssetBundleConfig testSerlize = ExBinarySerialize.DeserializeBytes<AssetBundleConfig>(textAsset.bytes);

        string path = "Assets/GameData/Prefabs/Attack.prefab";
        uint crc = ExCRC32.GetCRC32(path);
        ABBase abBase = null;
        for (int i = 0; i < testSerlize.ABList.Count; i++)
        {
            if (testSerlize.ABList[i].Crc == crc)
            {
                abBase = testSerlize.ABList[i];
            }
        }

        for (int i = 0; i < abBase.ABDependce.Count; i++)
        {
            AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABDependce[i]);
        }

        configAB = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + abBase.ABName);
        GameObject obj = GameObject.Instantiate(configAB.LoadAsset<GameObject>(abBase.AssetName));
    }
#endif
}
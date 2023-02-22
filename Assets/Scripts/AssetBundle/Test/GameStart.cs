
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    private void Awake()
    {
        AssetBundleManager.Instance.LoadAssetBundleConfig();
    }

  
}

using System;
using System.Collections.Generic;
using System.IO;
using Ex;
using UnityEngine;
using Object = UnityEngine.Object;

public class AssetBundleManager : Singleton<AssetBundleManager>
{
    //资源关系依赖配表，可以根据crc来找到对应的资源块
    protected Dictionary<uint, ResourceItem> m_ResourceItemDic = new Dictionary<uint, ResourceItem>();

    //储存已加载的AB包，key为crc
    protected Dictionary<uint, AssetBundleItem> m_AssetBundleItemDic = new Dictionary<uint, AssetBundleItem>();

    //AssetBundleItem类对象池
    protected ClassObjectPool<AssetBundleItem> m_AssetBundleItemPool = ObjectManager.Instance.GetOrCreateClassPool<AssetBundleItem>(500);

    /// <summary>
    /// 加载ab配置表
    /// </summary>
    /// <returns></returns>
    public bool LoadAssetBundleConfig()
    {
        m_ResourceItemDic.Clear();
        string configPath = Application.streamingAssetsPath + "/assetbundleconfig";
        AssetBundle configAB = AssetBundle.LoadFromFile(configPath);
        TextAsset textAsset = configAB.LoadAsset<TextAsset>("AssetBundleConfig");
        if (textAsset == null)
        {
            Debug.LogError("AssetBundleConfig is no exist!");
            return false;
        }

        AssetBundleConfig config = ExBinarySerialize.DeserializeBytes<AssetBundleConfig>(textAsset.bytes);

        for (int i = 0; i < config.ABList.Count; i++)
        {
            ABBase abBase = config.ABList[i];
            ResourceItem item = new ResourceItem();
            item.m_Crc = abBase.Crc;
            item.m_AssetName = abBase.AssetName;
            item.m_ABName = abBase.ABName;
            item.m_DependAssetBundle = abBase.ABDependce;

            if (m_ResourceItemDic.ContainsKey(item.m_Crc))
            {
                Debug.LogError("重复Crc  资源名:" + item.m_AssetName + " ab包名：" + item.m_ABName);
            }
            else
            {
                m_ResourceItemDic.Add(item.m_Crc, item);
            }
        }

        return true;
    }

    /// <summary>
    /// 根据路径的crc加载中间类ResourceItem
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceItem LoadResourceAssetBundle(uint crc)
    {
        ResourceItem item = null;
        if (!m_ResourceItemDic.TryGetValue(crc, out item) || item == null)
        {
            Debug.LogError(string.Format("LoadResourceAssetBundle error : can not find {0} in AssetBundleConfig", crc.ToString()));
            return item;
        }

        if (item.m_AssetBundle != null)
        {
            return item;
        }

        item.m_AssetBundle = LoadAssetBundle(item.m_ABName);

        if (item.m_DependAssetBundle != null)
        {
            for (int i = 0; i < item.m_DependAssetBundle.Count; i++)
            {
                LoadAssetBundle(item.m_DependAssetBundle[i]);
            }
        }

        return item;
    }

    /// <summary>
    /// 根据名字加载单个assetbunld
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    private AssetBundle LoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = ExCRC32.GetCRC32(name);

        if (!m_AssetBundleItemDic.TryGetValue(crc, out item))
        {
            AssetBundle assetBundle = null;
            string fullPath = Application.streamingAssetsPath + "/" + name;
            if (File.Exists(fullPath))
            {
                assetBundle = AssetBundle.LoadFromFile(fullPath);
            }

            if (assetBundle == null)
            {
                Debug.LogError("Load AssetBundle Error : " + fullPath);
            }

            item = m_AssetBundleItemPool.Spawn(true);
            item.assetBundle = assetBundle;
            item.RefCount++;
            m_AssetBundleItemDic.Add(crc, item);
        }
        else
        {
            item.RefCount++;
        }

        return item.assetBundle;
    }


    /// <summary>
    /// 释放资源
    /// </summary>
    /// <param name="item"></param>
    public void ReleaseAsset(ResourceItem item)
    {
        if (item == null)
        {
            return;
        }

        if (item.m_DependAssetBundle != null && item.m_DependAssetBundle.Count > 0)
        {
            for (int i = 0; i < item.m_DependAssetBundle.Count; i++)
            {
                UnLoadAssetBundle(item.m_DependAssetBundle[i]);
            }
        }

        UnLoadAssetBundle(item.m_AssetName);
    }

    private void UnLoadAssetBundle(string name)
    {
        AssetBundleItem item = null;
        uint crc = ExCRC32.GetCRC32(name);
        if (m_AssetBundleItemDic.TryGetValue(crc, out item) && item != null)
        {
            item.RefCount--;
            if (item.RefCount <= 0 && item.assetBundle != null)
            {
                item.assetBundle.Unload(true);
                item.Reset();
                m_AssetBundleItemPool.Recycle(item);
                m_AssetBundleItemDic.Remove(crc);
            }
        }
    }

    /// <summary>
    /// 根据crc查找ResourceItem
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    public ResourceItem FindResourceItem(uint crc)
    {
        return m_ResourceItemDic[crc];
    }
}

public class AssetBundleItem
{
    public AssetBundle assetBundle = null;
    public int RefCount;

    public void Reset()
    {
        assetBundle = null;
        RefCount = 0;
    }
}

public class ResourceItem
{
    #region AB包相关

    /// <summary>
    /// 资源路径的CRC
    /// </summary>
    public uint m_Crc = 0;

    /// <summary>
    /// 该资源的文件名
    /// </summary>
    public string m_AssetName = String.Empty;

    /// <summary>
    /// 该资源所在的AssetBundle 
    /// </summary>
    public string m_ABName = String.Empty;

    /// <summary>
    /// 该资源所依赖的AssetBundle
    /// </summary>
    public List<string> m_DependAssetBundle = null;

    #endregion

    #region 资源相关

    /// <summary>
    /// 该资源加载完的AB包
    /// </summary>
    public AssetBundle m_AssetBundle = null;

    /// <summary>
    /// 资源对象
    /// </summary>
    public Object m_Obj = null;

    /// <summary>
    /// 资源最后所使用的时间
    /// </summary>
    public float m_LastUseTime = 0.0f;

    /// <summary>
    /// 引用计数
    /// </summary>
    protected int m_RefCount = 0;

    /// <summary>
    /// 引用计数
    /// </summary>
    public int RefCount
    {
        get { return m_RefCount; }
        set
        {
            m_RefCount = value;
            if (m_RefCount<0)
            {
                Debug.LogError("refCount < 0" +"当前引用计数："+m_RefCount +"，"+(m_Obj!=null ? m_Obj.name :"name is null"));
            }
        }
    }

    #endregion
}
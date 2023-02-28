using System;
using System.Collections.Generic;
using Ex;
using UnityEngine;

/// <summary>
/// GameObject资源管理器，加载的所有资源是需要实例化的（需要在Hierarchy上出现GameObject的资源）  
/// </summary>
public class ObjectManager : Singleton<ObjectManager>
{
    /// <summary>
    /// 对象池节点
    /// </summary>
    private Transform RecyclePoolTrs;

    /// <summary>
    /// 场景节点
    /// </summary>
    public Transform SceneTrs;

    /// <summary>
    /// 对象池
    /// </summary>
    protected Dictionary<uint, List<ResourceObj>> m_ObjectPoolDic = new Dictionary<uint, List<ResourceObj>>();

    /// <summary>
    /// 暂存ResObj的Dic 
    /// </summary>
    protected Dictionary<int, ResourceObj> m_ResourceObjDic = new Dictionary<int, ResourceObj>();

    /// <summary>
    /// ResourceObj的类对象池
    /// </summary>
    protected ClassObjectPool<ResourceObj> m_ResourceObjClassPool = ObjectManager.Instance.GetOrCreateClassPool<ResourceObj>(1000);

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="recycleTrs">回收节点</param>
    /// <param name="sceneTrs">场景默认节点</param>
    public void Init(Transform recycleTrs, Transform sceneTrs)
    {
        RecyclePoolTrs = recycleTrs;
        SceneTrs = sceneTrs;
    }

    /// <summary>
    /// 从对象池取对象
    /// </summary>
    /// <param name="crc"></param>
    /// <returns></returns>
    protected ResourceObj GetObjectFromPool(uint crc)
    {
        List<ResourceObj> st = null;
        if (m_ObjectPoolDic.TryGetValue(crc, out st) && st != null && st.Count > 0)
        {
            ResourceObj resObj = st[0];
            st.RemoveAt(0);
            GameObject obj = resObj.m_CloneObj;
            if (!System.Object.ReferenceEquals(obj, null))
            {
#if UNITY_EDITOR
                if (obj.name.EndsWith("(Recycle)"))
                {
                    obj.name = obj.name.Replace("(Recycle)", "");
                }
#endif
            }

            return resObj;
        }

        return null;
    }


    /// <summary>
    /// 同步加载
    /// </summary>
    /// <param name="path"></param>
    /// <param name="bClear"></param>
    /// <returns></returns>
    public GameObject InstantiateObject(string path, bool setSceneObj = false, bool bClear = true)
    {
        uint crc = ExCRC32.GetCRC32(path);
        ResourceObj resourceObj = GetObjectFromPool(crc);
        if (resourceObj == null)
        {
            resourceObj = m_ResourceObjClassPool.Spawn(true);
            resourceObj.m_Crc = crc;
            resourceObj.m_bClear = bClear;
            //ResourceManager提供加载方法
            resourceObj = ResourceManager.Instance.LoadResource(path, resourceObj);

            if (resourceObj.m_ResItem.m_Obj != null)
            {
                resourceObj.m_CloneObj = GameObject.Instantiate(resourceObj.m_ResItem.m_Obj) as GameObject;
            }
        }

        if (setSceneObj)
        {
            resourceObj.m_CloneObj.transform.SetParent(SceneTrs, false);
        }

        int tempID = resourceObj.m_CloneObj.GetInstanceID();
        if (!m_ResourceObjDic.ContainsKey(tempID))
        {
            m_ResourceObjDic.Add(tempID, resourceObj);
        }

        return resourceObj.m_CloneObj;
    }


    public void ReleaseObject(GameObject obj, int maxCacheCount = -1, bool destroyCache = false, bool recycleParent = true)
    {
        if (obj == null)
        {
            return;
        }

        ResourceObj resObj = null;

        int tempID = obj.GetInstanceID();
        if (!m_ResourceObjDic.TryGetValue(tempID, out resObj))
        {
            Debug.Log(obj.name + "对象不是ObjectManager创建的！");
            return;
        }

        if (resObj == null)
        {
            Debug.LogError("缓存的ResourceObj为空！");
            return;
        }

        if (resObj.m_Already)
        {
            Debug.LogError("该对象已经放回对象池了，检测自己是否清空引用");
            return;
        }
        
        
    }


    #region 类对象池的使用

    protected Dictionary<Type, object> m_ClassPoolDic = new Dictionary<Type, object>();

    /// <summary>
    /// 创建类对象池，创建完成以后外面可以保存ClassObjectPool<T>,然后调用Spawn和Recycle来创建和回收类对象
    /// </summary>
    /// <param name="maxcount"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public ClassObjectPool<T> GetOrCreateClassPool<T>(int maxcount) where T : class, new()
    {
        Type type = typeof(T);
        object outObj = null;
        if (!m_ClassPoolDic.TryGetValue(type, out outObj) || outObj == null)
        {
            ClassObjectPool<T> newPool = new ClassObjectPool<T>(maxcount);
            m_ClassPoolDic.Add(type, newPool);
            return newPool;
        }

        return outObj as ClassObjectPool<T>;
    }

    #endregion
}
using System.Collections.Generic;
using Ex;
using UnityEngine;

public class ResourceManager : Singleton<ResourceManager>
{
    public bool m_LoadFormAssetBundle = false;

    /// <summary>
    /// 缓存使用的资源列表
    /// </summary>
    public Dictionary<uint, ResourceItem> AssetDic { get; set; } = new Dictionary<uint, ResourceItem>();

    /// <summary>
    /// 缓存引用计数为零的资源列表，达到缓存最大的时候释放这个列表里面最早没用的资源
    /// </summary>
    protected CMapList<ResourceItem> m_NoRefrenceAssetMapList = new CMapList<ResourceItem>();


    /// <summary>
    /// 同步资源加载，外部直接调用，仅加载不需要实例化的资源，例如Texture,音频等等
    /// </summary>
    /// <param name="path"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T LoadResource<T>(string path) where T : UnityEngine.Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        uint crc = ExCRC32.GetCRC32(path);

        ResourceItem item = GetCacheResourceItem(crc);

        if (item != null)
        {
            return item.m_Obj as T;
        }

        T obj = null;

#if UNITY_EDITOR
        if (!m_LoadFormAssetBundle)
        {
            obj = LoadAssetByEditor<T>(path);
            item = AssetBundleManager.Instance.FindResourceItem(crc);
        }
#endif

        if (obj == null)
        {
            item = AssetBundleManager.Instance.LoadResourceAssetBundle(crc);
            if (item != null && item.m_AssetBundle != null)
            {
                obj = item.m_AssetBundle.LoadAsset<T>(item.m_AssetName);
            }
        }

        CacheResource(path, ref item, crc, obj);
        return obj;
    }

    /// <summary>
    /// 不需要实例化的资源的卸载
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="desctroyObj"></param>
    /// <returns></returns>
    public bool ReleaseResource(Object obj, bool desctroyObj = false)
    {
        if (obj == null)
        {
            return false;
        }

        ResourceItem item = null;

        foreach (ResourceItem res in AssetDic.Values)
        {
            if (res.m_Guid == obj.GetInstanceID())
            {
                item = res;
            }
        }

        if (item == null)
        {
            Debug.LogError("AssetDic 里不存在该资源 ： " + obj.name + " 可能释放了多次");
        }

        item.RefCount--;
        DestroyResourceItem(item, desctroyObj);
        return true;
    }

    /// <summary>
    /// 缓存加载的资源
    /// </summary>
    /// <param name="path"></param>
    /// <param name="item"></param>
    /// <param name="crc"></param>
    /// <param name="obj"></param>
    /// <param name="addRefCount"></param>
    void CacheResource(string path, ref ResourceItem item, uint crc, Object obj, int addRefCount = 1)
    {
        //缓存太多，清楚最早没有使用的资源
        WashOut();

        if (item == null)
        {
            Debug.LogError("ResourceItem is null , path : " + path);
        }

        if (obj == null)
        {
            Debug.LogError("ResourceLoad Fail : " + path);
        }

        item.m_Obj = obj;
        item.m_Guid = obj.GetInstanceID();
        item.m_LastUseTime = Time.realtimeSinceStartup;
        item.RefCount += addRefCount;

        ResourceItem oldItem = null;
        if (AssetDic.TryGetValue(item.m_Crc, out oldItem))
        {
            AssetDic[item.m_Crc] = item;
        }
        else
        {
            AssetDic.Add(item.m_Crc, item);
        }
    }

    /// <summary>
    /// 缓存太多，清除最早没有使用的资源
    /// </summary>
    protected void WashOut()
    {
        //当当前内存使用大于80%,进行清除最早没用的资源
        // {
        //     if (m_NoRefrenceAssetMapList.Size() <= 0)
        //         break;
        //
        //     ResourceItem item = m_NoRefrenceAssetMapList.GetLast();
        //     DestroyResourceItem(item, true);
        //     m_NoRefrenceAssetMapList.RemoveLast();
        // }
    }

    /// <summary>
    /// 回收一个资源
    /// </summary>
    /// <param name="item"></param>
    /// <param name="destroy"></param>
    protected void DestroyResourceItem(ResourceItem item, bool destroyCache = false)
    {
        if (item == null || item.RefCount > 0)
        {
            return;
        }

        if (!AssetDic.Remove(item.m_Crc))
        {
            return;
        }

        if (!destroyCache)
        {
            m_NoRefrenceAssetMapList.InsertToHead(item);
            return;
        }
        
        //释放assetbundle引用
        AssetBundleManager.Instance.ReleaseAsset(item);

        if (item.m_Obj != null)
        {
            item.m_Obj = null;
        }
    }

#if UNITY_EDITOR
    protected T LoadAssetByEditor<T>(string path) where T : UnityEngine.Object
    {
        return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
    }
#endif

    ResourceItem GetCacheResourceItem(uint crc, int addRefCount = 1)
    {
        ResourceItem item = null;
        if (AssetDic.TryGetValue(crc, out item))
        {
            if (item != null)
            {
                item.RefCount += addRefCount;
                item.m_LastUseTime = Time.realtimeSinceStartup;

                // if (item.RefCount <= 1)
                // {
                //     m_NoRefrenceAssetMapList.Remove(item);
                // }
            }
        }

        return item;
    }
}


/// <summary>
/// 双向链表结构节点
/// </summary>
/// <typeparam name="T"></typeparam>
public class DoubleLinkListNode<T> where T : class, new()
{
    /// <summary>
    /// 前一个节点
    /// </summary>
    public DoubleLinkListNode<T> prev = null;

    /// <summary>
    /// 后一个节点
    /// </summary>
    public DoubleLinkListNode<T> next = null;

    /// <summary>
    /// 当前节点
    /// </summary>
    public T t = null;
}

/// <summary>
/// 双向链表结构
/// </summary>
/// <typeparam name="T"></typeparam>
public class DoubleLinkList<T> where T : class, new()
{
    /// <summary>
    /// 表头
    /// </summary>
    public DoubleLinkListNode<T> Head = null;

    /// <summary>
    /// 表尾
    /// </summary>
    public DoubleLinkListNode<T> Tail = null;

    /// <summary>
    /// 双向链表结构类对象池
    /// </summary>
    protected ClassObjectPool<DoubleLinkListNode<T>> m_DoubleLinkNodePool = ObjectManager.Instance.GetOrCreateClassPool<DoubleLinkListNode<T>>(100);

    /// <summary>
    /// 个数
    /// </summary>
    protected int m_Count = 0;

    /// <summary>
    /// 个数
    /// </summary>
    public int Count
    {
        get { return m_Count; }
    }

    /// <summary>
    /// 添加一个节点到头部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinkListNode<T> AddToHeader(T t)
    {
        DoubleLinkListNode<T> pList = m_DoubleLinkNodePool.Spawn(true);
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddToHeader(pList);
    }

    /// <summary>
    /// 添加一个节点到头部
    /// </summary>
    /// <param name="pNode"></param>
    /// <returns></returns>
    public DoubleLinkListNode<T> AddToHeader(DoubleLinkListNode<T> pNode)
    {
        if (pNode == null)
            return null;

        pNode.prev = null;
        if (Head == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            pNode.next = Head;
            Head.prev = pNode;
            Head = pNode;
        }

        m_Count++;
        return Head;
    }

    /// <summary>
    /// 添加节点到尾部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public DoubleLinkListNode<T> AddToTail(T t)
    {
        DoubleLinkListNode<T> pList = m_DoubleLinkNodePool.Spawn(true);
        pList.next = null;
        pList.prev = null;
        pList.t = t;
        return AddToTail(pList);
    }

    /// <summary>
    /// 添加节点到尾部
    /// </summary>
    /// <param name="pNode"></param>
    /// <returns></returns>
    public DoubleLinkListNode<T> AddToTail(DoubleLinkListNode<T> pNode)
    {
        if (pNode == null)
            return null;

        pNode.next = null;

        if (Tail == null)
        {
            Head = Tail = pNode;
        }
        else
        {
            pNode.prev = Tail;
            Tail.next = pNode;
            Tail = pNode;
        }

        m_Count++;
        return Tail;
    }

    /// <summary>
    /// 移除某个节点
    /// </summary>
    /// <param name="pNode"></param>
    public void RemoveNode(DoubleLinkListNode<T> pNode)
    {
        if (pNode == null)
            return;

        if (pNode == Head)
            Head = pNode.next;

        if (pNode == Tail)
            Tail = pNode.prev;

        if (pNode.prev != null)
            pNode.prev.next = pNode.next;

        if (pNode.next != null)
            pNode.next.prev = pNode.prev;

        pNode.next = pNode.prev = null;
        pNode.t = null;
        m_DoubleLinkNodePool.Recycle(pNode);
        m_Count--;
    }

    /// <summary>
    /// 把某个节点移动到头部
    /// </summary>
    /// <param name="pNode"></param>
    public void MoveToHead(DoubleLinkListNode<T> pNode)
    {
        if (pNode == null || pNode == Head)
            return;
        if (pNode.prev == null && pNode.next == null)
            return;

        if (pNode == Tail)
            Tail = pNode.prev;

        if (pNode.prev != null)
            pNode.prev.next = pNode.next;

        if (pNode.next != null)
            pNode.next.prev = pNode.prev;

        pNode.prev = null;
        pNode.next = Head;
        Head.prev = pNode;
        Head = pNode;

        //无效代码////////
        if (Tail == null)
        {
            Tail = Head;
        }
        ////////////////////
    }
}


public class CMapList<T> where T : class, new()
{
    private DoubleLinkList<T> m_DLink = new DoubleLinkList<T>();
    private Dictionary<T, DoubleLinkListNode<T>> m_FindMap = new Dictionary<T, DoubleLinkListNode<T>>();

    ~CMapList()
    {
        Clear();
        List<string> s = new List<string>();
    }

    /// <summary>
    /// 清空链表
    /// </summary>
    public void Clear()
    {
        while (m_DLink.Tail != null)
        {
            Remove(m_DLink.Tail.t);
        }
    }

    /// <summary>
    /// 插入一个节点到表头
    /// </summary>
    /// <param name="t"></param>
    public void InsertToHead(T t)
    {
        DoubleLinkListNode<T> node = null;
        if (m_FindMap.TryGetValue(t, out node) && node != null)
        {
            m_DLink.MoveToHead(node);
            return;
        }

        m_DLink.AddToHeader(t);
        m_FindMap.Add(t, m_DLink.Head);
    }

    /// <summary>
    /// 移除尾部节点
    /// </summary>
    public void RemoveLast()
    {
        if (m_DLink.Tail != null)
        {
            Remove(m_DLink.Tail.t);
        }
    }

    /// <summary>
    /// 移除某个节点
    /// </summary>
    /// <param name="t"></param>
    public void Remove(T t)
    {
        DoubleLinkListNode<T> node = null;
        if (!m_FindMap.TryGetValue(t, out node) || node == null)
        {
            return;
        }

        m_DLink.RemoveNode(node);
        m_FindMap.Remove(t);
    }

    /// <summary>
    /// 获取到尾部节点
    /// </summary>
    /// <returns></returns>
    public T GetLast()
    {
        return m_DLink.Tail == null ? null : m_DLink.Tail.t;
    }


    /// <summary>
    /// 返回节点个数
    /// </summary>
    /// <returns></returns>
    public int Size()
    {
        return m_FindMap.Count;
    }


    /// <summary>
    /// 查找是否存在该节点
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Find(T t)
    {
        DoubleLinkListNode<T> node = null;
        if (!m_FindMap.TryGetValue(t, out node) || node == null)
        {
            return false;
        }

        return true;
    }


    /// <summary>
    /// 刷新某个节点，把节点移动到头部
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    public bool Refresh(T t)
    {
        DoubleLinkListNode<T> node = null;
        if (!m_FindMap.TryGetValue(t, out node) || node == null)
            return false;
        m_DLink.MoveToHead(node);
        return true;
    }
}
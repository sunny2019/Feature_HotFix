using Ex;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    private GameObject obj;

    private void Awake()
    {
        GameObject.DontDestroyOnLoad(gameObject);
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"), transform.Find("SceneTrs"));
    }

    private void Start()
    {
        //ObjectManager.Instance.PreloadGameObject("Assets/GameData/Prefabs/Attack.prefab",20,false);
    }

    private void OnLoadFinish(string path, UnityEngine.Object o, object param1, object param2, object param3)
    {
        ExStopwatch.Stop();
        obj=o as GameObject;
        
        //歇
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ObjectManager.Instance.ReleaseObject(obj);
            obj = null;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            ExStopwatch.Start();
            ObjectManager.Instance.InstantiateObjectAsync("Assets/GameData/Prefabs/Attack.prefab", OnLoadFinish, LoadResPriority.RES_HIGHT,true);

        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ObjectManager.Instance.ReleaseObject(obj, 0, true);
            obj = null;
        }
    }
}
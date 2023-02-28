
using Ex;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    public AudioSource m_Audio;
    private AudioClip _clip;

    private void Awake()
    {
        GameObject.DontDestroyOnLoad(gameObject);
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
        ObjectManager.Instance.Init(transform.Find("RecyclePoolTrs"),transform.Find("SceneTrs"));
        
    }
    private void Start()
    {
        ResourceManager.Instance.PreloadRes("Assets/GameData/Sounds/menusound.mp3");
        //ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Sounds/menusound.mp3", OnLoadFinishe, LoadResPriority.RES_MIDDLE);
        // _clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/menusound.mp3");
        // m_Audio.clip = _clip;
        // m_Audio.Play();
    }

    private void OnLoadFinishe(string path, UnityEngine.Object obj, object param1, object param2, object param3)
    {
        _clip = obj as AudioClip;
        m_Audio.clip = _clip;
        m_Audio.Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            ExStopwatch.Start();
            _clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/menusound.mp3");
            ExStopwatch.Stop();
            m_Audio.clip = _clip;
            m_Audio.Play();
        }else if (Input.GetKeyDown(KeyCode.D))
        {
            ResourceManager.Instance.ReleaseResource(_clip, true);
            _clip = null;
            m_Audio.clip = null;
        }
    }

}
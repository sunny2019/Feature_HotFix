using UnityEngine;
using Object = UnityEngine.Object;

public class GameStart : MonoBehaviour
{
    public AudioSource m_Audio;
    private AudioClip _clip;

    private void Awake()
    {
        AssetBundleManager.Instance.LoadAssetBundleConfig();
        ResourceManager.Instance.Init(this);
    }


    private void Start()
    {
        //ResourceManager.Instance.AsyncLoadResource("Assets/GameData/Sounds/menusound.mp3", OnLoadFinishe, LoadResPriority.RES_MIDDLE);

        _clip = ResourceManager.Instance.LoadResource<AudioClip>("Assets/GameData/Sounds/menusound.mp3");
        m_Audio.clip = _clip;
        m_Audio.Play();
    }

    private void OnLoadFinishe(string path, Object obj, object param1, object param2, object param3)
    {
        _clip = obj as AudioClip;
        m_Audio.clip = _clip;
        m_Audio.Play();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
          
            ResourceManager.Instance.ReleaseResource(_clip, false);
            m_Audio.clip = null;
            _clip = null;
        }
    }

 
}
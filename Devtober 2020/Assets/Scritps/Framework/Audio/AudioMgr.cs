using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AudioMgr : SingletonBase<AudioMgr>
{

    private AudioSource BGMusic = null;

    private float BGMVolume = 1;

    public GameObject SoundObj = null;
    private List<AudioSource> SoundList = new List<AudioSource>();
    public List<AudioClip> audios = new List<AudioClip>();

    private void Awake()
    {
        Object[] sounds = Resources.LoadAll("Audio", typeof(AudioClip));
        foreach (AudioClip audio in sounds)
        {
            audios.Add(audio);
        }
    }

    void Update()
    {
        for(int i = SoundList.Count - 1; i >= 0; --i)
        {
            if(SoundList[i].isPlaying)
            {
                GameObject.Destroy(SoundList[i]);
                SoundList.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="name"></param>
    public void PlayBackMusic(string name)
    {
        if(BGMusic == null)
        {
            GameObject obj = new GameObject();
            obj.name = "BGMusic";
            BGMusic = obj.AddComponent<AudioSource>();
        }
        //异步加载背景音乐并播放
        ResMgr.GetInstance().LoadAsync<AudioClip>(name, (Clip) => {
            BGMusic.clip = Clip;
            BGMusic.volume = BGMVolume;
            BGMusic.loop = true;
            BGMusic.Play();
        });
    }

    public void PlayBackMusic(string name, float v)
    {
        if(BGMusic == null)
        {
            GameObject obj = new GameObject();
            obj.name = "BGMusic";
            BGMusic = obj.AddComponent<AudioSource>();
        }
        //异步加载背景音乐并播放
        ResMgr.GetInstance().LoadAsync<AudioClip>(name, (Clip) => {
            BGMusic.clip = Clip;
            BGMVolume = v;
            BGMusic.volume = BGMVolume;
            BGMusic.loop = true;
            BGMusic.Play();
        });
    }

    public void PlayBackMusic(AudioClip clip)
    {
        if (BGMusic == null)
        {
            BGMusic = new GameObject
            {
                name = "BGMusic"
            }.AddComponent<AudioSource>();
        }
        BGMusic.clip = clip;
        BGMusic.volume = BGMVolume;
        BGMusic.loop = true;
        BGMusic.Play();
    }

    public void PlayBackMusic(AudioClip clip, float v)
    {
        if (BGMusic == null)
        {
            BGMusic = new GameObject
            {
                name = "BGMusic"
            }.AddComponent<AudioSource>();
        }
        BGMusic.clip = clip;
        BGMVolume = v;
        BGMusic.volume = BGMVolume;
        BGMusic.loop = true;
        BGMusic.Play();
    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBackMusic()
    {
        if (BGMusic == null)
            return;
        BGMusic.Pause();
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBackMusic()
    {
        if (BGMusic == null)
            return;
        BGMusic.Stop();
    }

    /// <summary>
    /// 改变背景音乐大小
    /// </summary>
    /// <param name="v"></param>
    public void ChangeBGMValue(float v)
    {
        BGMVolume = v;
        if (BGMusic == null)
            return;
        BGMusic.volume = BGMVolume;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    /// <param name="name">路径</param>
    /// <param name="callback">回调函数</param>
    /// <param name="volume">音量</param>
    /// <param name="IsLoop">是否循环</param>
    public void PlayAudio(string name, float v, bool IsLoop, UnityAction<AudioSource> callback)
    {
        if(SoundObj == null)
        {
            SoundObj = new GameObject("Sound");
        }

        
        //异步加载音效并播放
        ResMgr.GetInstance().LoadAsync<AudioClip>(name, (Clip) => 
        {
            AudioSource source = SoundObj.AddComponent<AudioSource>();
            source.clip = Clip;
            source.volume = v;
            BGMusic.loop = IsLoop;
            source.Play();
            SoundList.Add(source);
            callback?.Invoke(source);
        });
    }

    public void PlayAudio(AudioClip Clip, float v, bool IsLoop, UnityAction<AudioSource> callback = null)
    {
        if (SoundObj == null)
        {
            SoundObj = new GameObject("Sound");
        }
        AudioSource audioSource = new AudioSource();
        if (IsLoop)
        {
            audioSource = this.SoundObj.AddComponent<AudioSource>();
            audioSource.clip = Clip;
            audioSource.volume = v;
            audioSource.loop = IsLoop;
            audioSource.Play();
            SoundList.Add(audioSource);
        }
        else
        {
            bool flag2 = !SoundObj.TryGetComponent(out audioSource);
            if (flag2)
            {
                audioSource = SoundObj.AddComponent<AudioSource>();
                audioSource.PlayOneShot(Clip, v);
            }
            else
            {
                audioSource.PlayOneShot(Clip, v);
            }
        }
        if (callback != null)
        {
            callback.Invoke(audioSource);
        }
    }

    public void PlayAudio(AudioSource source, string name, float v, bool IsLoop, UnityAction<AudioSource> callback)
    {
        AudioClip Clip = null;
        foreach (var item in audios)
        {
            if(item.name == name)
            {
                Clip = item;
                break;
            }
        }
        //异步加载音效并播放
        if(Clip != null)
        {
            if(IsLoop)
            {
                source.clip = Clip;
                source.volume = v;
                BGMusic.loop = IsLoop;
                source.Play();
                SoundList.Add(source);
                callback?.Invoke(source);
            }
            else
            {
                source.PlayOneShot(Clip, v);
                callback?.Invoke(source);
            }
            
        }
    }

    /// <summary>
    /// 更改某一个音效的音量
    /// </summary>
    /// <param name="source">目标音效</param>
    /// <param name="v">目标音量</param>
    public void ChangeAudioVolume(AudioSource source,float v)
    {
        if (source == null)
            return;
        source.volume = v;
    }

    /// <summary>
    /// 将所有的音效都改到某一个值
    /// </summary>
    /// <param name="v">目标音量</param>
    public void ChangeAllAudioVolumeTo(float v)
    {
        for(int i = 0; i < SoundList.Count; i++)
        {
            SoundList[i].volume = v;
        }
    }

    /// <summary>
    /// 将所有的音效都按比例放大或缩小音量
    /// </summary>
    /// <param name="ratio">比例</param>
    public void ChangeAllAudioVolumeByRatio(float ratio)
    {
        for (int i = 0; i < SoundList.Count; i++)
        {
            SoundList[i].volume *= ratio;
        }
    }

    /// <summary>
    /// 停止音效
    /// </summary>
    /// <param name="name"></param>
    public void StopAudio(AudioSource source)
    {
        if(SoundList.Contains(source))
        {
            SoundList.Remove(source);
            source.Stop();
            GameObject.Destroy(source);
        }
    }
}

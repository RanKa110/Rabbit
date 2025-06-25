using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioManager : Singleton<AudioManager>
{
    public GameObject SettingPanel;

    public AudioSource BgmSource;
    public AudioSource SfxSource;

    public Slider BgmSlider;
    public Slider SfxSlider;

    // Start is called before the first frame update
    void Start()
    {
        if(BgmSlider != null)
        {
            BgmSlider.value = BgmSource.volume;

            BgmSlider.onValueChanged.AddListener(SetBgmVolume);
        }
        else
        {
            Debug.LogError("BgmSlider 미할당으로 인해 오류 발생!");
        }

        if (SfxSlider != null)
        {
            SfxSlider.value = SfxSource.volume;

            SfxSlider.onValueChanged.AddListener(SetSfxVolume);
        }
        else
        {
            Debug.LogError("SfxSlider미할당으로 인해 오류 발생!");
        }
    }
    public void Option()
    {
        {
            Debug.Log("Option");

            if (SettingPanel != null)
            {
                SettingPanel.SetActive(true);
                Time.timeScale = 0f;
            }
            else
            {
                Debug.LogWarning("ReplayButtonController: Settings Panel");
            }
        }
    }

    public void RePlay()
    {
        Debug.Log("RePlay");

        if (SettingPanel != null)
        {
            SettingPanel.SetActive(false);
            Time.timeScale = 1f;
        }
        else
        {
            Debug.LogWarning("ReplayButtonController: Settings Panel");
        }
    }

    public void PlayBGM(AudioClip BGM)
    {
        BgmSource.clip = BGM;
        BgmSource.Play();
    }

public void playSfx(AudioClip Sfx)
    {
        SfxSource.clip = Sfx;
        SfxSource.Play();
    }

public void SetBgmVolume(float BV)
    {
        Debug.Log($"현재 음향: {BV}");
        BgmSource.volume = BV;
    }

public void SetSfxVolume(float SV)
    {
        Debug.Log($"현재 효과음: {SV}");
        SfxSource.volume = SV;
    }
}

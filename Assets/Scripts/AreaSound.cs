using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AreaSound : MonoBehaviour
{
    [SerializeField] private int areaSoundIndex;
    private Coroutine decreaseVolumeCoroutine;
    private AudioSource currentAudioSource;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Player>() != null)
        {
            // 停止正在进行的音量降低协程
            if (decreaseVolumeCoroutine != null)
            {
                StopCoroutine(decreaseVolumeCoroutine);
                decreaseVolumeCoroutine = null;
            }

            // 获取对应的AudioSource并重置音量
            if (AudioManager.instance != null && areaSoundIndex < AudioManager.instance.GetSFXArray().Length)
            {
                currentAudioSource = AudioManager.instance.GetSFXArray()[areaSoundIndex];
                if (currentAudioSource != null)
                {
                    currentAudioSource.volume = 1f; // 将音量重置为1
                    AudioManager.instance.PlaySFX(areaSoundIndex, null);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.GetComponent<Player>() != null)
        {
            // 启动逐渐停止播放的协程
            if (currentAudioSource != null)
            {
                if (decreaseVolumeCoroutine != null)
                    StopCoroutine(decreaseVolumeCoroutine);
                
                decreaseVolumeCoroutine = StartCoroutine(DecreaseVolume(currentAudioSource));
            }
        }
    }

    private IEnumerator DecreaseVolume(AudioSource _audio)
    {
        if (_audio == null)
            yield break;

        float defaultVolume = _audio.volume;

        while(_audio.volume > .1f)
        {
            _audio.volume -= _audio.volume * .2f;
            yield return new WaitForSeconds(.25f);

            if(_audio.volume <= .1f)
            {
                _audio.Stop();
                _audio.volume = defaultVolume;
                break;
            }
        }

        decreaseVolumeCoroutine = null;
    }
}

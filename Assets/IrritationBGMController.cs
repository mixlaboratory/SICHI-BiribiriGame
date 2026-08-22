using System.Collections;
using UnityEngine;

public class IrritationBGMController : MonoBehaviour
{
    private enum BGMState
    {
        Normal,
        Tension,
        HighTension
    }

    [Header("参照")]
    [SerializeField]
    private IrritationManager irritationManager;

    [Header("Audio Source")]
    [SerializeField]
    private AudioSource audioSourceA;

    [SerializeField]
    private AudioSource audioSourceB;

    [Header("BGM")]
    [SerializeField]
    private AudioClip normalBGM;

    [SerializeField]
    private AudioClip tensionBGM;

    [SerializeField]
    private AudioClip highTensionBGM;

    [Header("切り替え基準")]
    [SerializeField, Range(0f, 100f)]
    private float tensionThreshold = 40f;

    [SerializeField, Range(0f, 100f)]
    private float highTensionThreshold = 80f;

    [Header("音量")]
    [SerializeField, Range(0f, 1f)]
    private float bgmVolume = 0.5f;

    [Header("クロスフェード")]
    [SerializeField]
    private float fadeDuration = 1.0f;

    private AudioSource currentSource;
    private AudioSource nextSource;

    private BGMState currentState;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (irritationManager == null)
        {
            irritationManager =
                FindFirstObjectByType<IrritationManager>();
        }

        currentSource = audioSourceA;
        nextSource = audioSourceB;

        currentState = BGMState.Normal;

        PlayInitialBGM();

        if (irritationManager != null)
        {
            irritationManager.OnIrritationChanged +=
                OnIrritationChanged;
        }
    }

    private void PlayInitialBGM()
    {
        if (currentSource == null ||
            normalBGM == null)
        {
            return;
        }

        currentSource.clip = normalBGM;
        currentSource.loop = true;
        currentSource.volume = bgmVolume;

        currentSource.Play();
    }

    private void OnIrritationChanged(
        float irritation
    )
    {
        BGMState newState;

        if (irritation >= highTensionThreshold)
        {
            newState = BGMState.HighTension;
        }
        else if (irritation >= tensionThreshold)
        {
            newState = BGMState.Tension;
        }
        else
        {
            newState = BGMState.Normal;
        }

        if (newState == currentState)
        {
            return;
        }

        currentState = newState;

        AudioClip nextClip =
            GetClip(currentState);

        ChangeBGM(nextClip);
    }

    private AudioClip GetClip(
        BGMState state
    )
    {
        switch (state)
        {
            case BGMState.Tension:
                return tensionBGM;

            case BGMState.HighTension:
                return highTensionBGM;

            default:
                return normalBGM;
        }
    }

    private void ChangeBGM(
        AudioClip newClip
    )
    {
        if (newClip == null)
        {
            return;
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine =
            StartCoroutine(
                CrossFade(newClip)
            );
    }

    private IEnumerator CrossFade(
        AudioClip newClip
    )
    {
        nextSource.clip = newClip;
        nextSource.loop = true;
        nextSource.volume = 0f;

        nextSource.Play();

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float ratio =
                Mathf.Clamp01(
                    timer / fadeDuration
                );

            currentSource.volume =
                Mathf.Lerp(
                    bgmVolume,
                    0f,
                    ratio
                );

            nextSource.volume =
                Mathf.Lerp(
                    0f,
                    bgmVolume,
                    ratio
                );

            yield return null;
        }

        currentSource.Stop();
        currentSource.volume = 0f;

        nextSource.volume = bgmVolume;

        AudioSource temp =
            currentSource;

        currentSource =
            nextSource;

        nextSource =
            temp;

        fadeCoroutine = null;
    }

    private void OnDestroy()
    {
        if (irritationManager != null)
        {
            irritationManager.OnIrritationChanged -=
                OnIrritationChanged;
        }
    }
}
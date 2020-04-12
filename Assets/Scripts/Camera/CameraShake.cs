using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera virtualCamera = null;
    CinemachineBasicMultiChannelPerlin virtualCameraNoise;

    float curShakeDuration;
    // Start is called before the first frame update
    void Start()
    {
        if (virtualCamera != null)
            virtualCameraNoise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    // Update is called once per frame
    void Update()
    {
        if (curShakeDuration <= Time.time)
            virtualCameraNoise.m_AmplitudeGain = 0f;
    }

    public void Shake(float shakeAmplitude, float shakeFrequency, float shakeDuration)
    {
        virtualCameraNoise.m_AmplitudeGain = shakeAmplitude;
        virtualCameraNoise.m_FrequencyGain = shakeFrequency;
        curShakeDuration = shakeDuration + Time.time;
    }
}

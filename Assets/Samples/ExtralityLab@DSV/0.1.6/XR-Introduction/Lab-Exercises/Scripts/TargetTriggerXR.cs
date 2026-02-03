using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtralityLab;

[RequireComponent(typeof(Collider)), RequireComponent(typeof(AudioSource))]
public class TargetTriggerXR : MonoBehaviour
{
    public float uiDelay = 3f;
    public Collider marble;
    public TimingRecordingXR timingRecording;
    public TargetGroupWeightControl targetGroupWeightControl;
    public ParticleSystem completeParticleSystem;
    public MqttClientExampleReceiveDigital mqttClient;

    AudioSource m_AudioSource;

    void Awake ()
    {
        m_AudioSource = GetComponent<AudioSource> ();
        if (mqttClient == null && MqttClientExampleReceiveDigital.Instance != null)
        {
            mqttClient = MqttClientExampleReceiveDigital.Instance;
        }
    }

    void OnTriggerEnter (Collider other)
    {
        if (other == marble)
        {
            completeParticleSystem.Play();
            timingRecording.GoalReached (uiDelay);
//            targetGroupWeightControl.ApplySpecificFocus (marble.attachedRigidbody);
            m_AudioSource.PlayOneShot (m_AudioSource.clip);
            mqttClient.PublishLed(true);
        }
    }
}
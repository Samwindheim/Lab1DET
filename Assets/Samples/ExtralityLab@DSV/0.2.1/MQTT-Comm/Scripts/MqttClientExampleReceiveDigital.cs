using System.Collections.Generic;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using UnityEngine;

namespace ExtralityLab
{
    public class MqttClientExampleReceiveDigital : M2MqttUnityClient
    {
        [Header("Topics Config")]
        public string subscribedTopic = "myUnityApp/digital";
        public bool autoSubscribe = false;
        public string publishTopic = "f598/D13_boardLED";

        [Header("Actuators Config")]
        public FlipperXR flipper;
        public bool currentState = false;
        public static MqttClientExampleReceiveDigital Instance { get; private set; }

        private List<string> eventMessages = new List<string>();

        protected override void Awake()
        {
            base.Awake();
            Instance = this;
        }

        protected override void Start()
        {
            base.Start();

            // Add here your custom Start() below:

        }

        protected override void Update()
        {
            base.Update();

            if (eventMessages.Count > 0)
            {
                foreach (string msg in eventMessages)
                {
                    ProcessMessage(msg);
                }
                eventMessages.Clear();
            }
        }

        protected override void OnConnecting()
        {
            base.OnConnecting();
            Debug.Log($"MQTT: subscription {subscribedTopic} connecting to broker on " + brokerAddress + ":" + brokerPort.ToString() + "...\n");
        }

        protected override void OnConnected()
        {
            // base.OnConnected(); // Uncommenting this will autosubscribe to topics
            Debug.Log($"MQTT: subscription {subscribedTopic} connected!");
            if (autoSubscribe)
                SubscribeTopics();
        }

        protected override void SubscribeTopics()
        {
            client.Subscribe(new string[] { subscribedTopic }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
        }

        protected override void UnsubscribeTopics()
        {
            client.Unsubscribe(new string[] { subscribedTopic });
        }

        private void OnDestroy()
        {
            Disconnect();
        }

        protected override void DecodeMessage(string topic, byte[] message)
        {
            string msg = System.Text.Encoding.UTF8.GetString(message);
            // Debug.Log("Received: " + msg);
            eventMessages.Add(msg);
        }

        ////// CALLBACKS from Buttons

        public void SubscribeToMqttTopic()
        {
            SubscribeTopics();
        }

        public void UnsubscribeFromTopic()
        {
            UnsubscribeTopics();
        }

        private void ProcessMessage(string msg)
        {
            Debug.Log($"MQTT Subscription {subscribedTopic} received: " + msg);

            // Remap a string from "1" or "0" into a string "true" or "false".
            if (msg.CompareTo("0") == 0) msg = "true"; // Not-pressed
            if (msg.CompareTo("1") == 0) msg = "false"; // Pressed
            
            bool parseValid = bool.TryParse(msg, out currentState);
            if (parseValid)
            {
                bool currentButtonState = bool.Parse(msg);
                // Activate the flipper
                flipper.activateState = currentButtonState;

                // Activate audio
                if(currentButtonState)
                    flipper.playOneTimeActivateAudio = true;
                else
                    flipper.playOneTimeDeactivateAudio = true;
            }
        }

        public void PublishLed(bool on)
        {
            if (client == null || !client.IsConnected)
                return;

            string payload = on ? "1" : "0";
            client.Publish(
                publishTopic,
                System.Text.Encoding.UTF8.GetBytes(payload),
                MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE,
                false
            );

            if (on)
            {
                // Turn off the light after 3 seconds
                Invoke(nameof(TurnOffLED), 3f);
            }
        }

        private void TurnOffLED()
        {
            PublishLed(false);
        }
        
    }
}
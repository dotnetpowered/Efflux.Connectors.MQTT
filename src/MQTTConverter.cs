using System;
using System.Linq;
using MQTTnet;

namespace Efflux.MQTT
{
    public static class MQTTConverter
    {
        public static EffluxMessage FromMMQTT(MqttApplicationMessage m)
        {
            var em = new EffluxMessage(m.Payload, m.ContentType);
            if (m.PayloadFormatIndicator == MQTTnet.Protocol.MqttPayloadFormatIndicator.Unspecified)
                em.MetaData.ContentType = "application/octet-stream";
            else
            {
                if (string.IsNullOrWhiteSpace(m.ContentType))
                    m.ContentType = "text/plain;UTF8";
            }
            foreach (var prop in m.UserProperties)
                em.MetaData.Properties.Add(prop.Name, prop.Value);

            return em;
        }

        public static MqttApplicationMessage ToMMQTT(EffluxMessage m2)
        {
            var m = new MqttApplicationMessage();

            m.ContentType = m2.MetaData.ContentType;
            m.Payload = m2.PayloadAsBytes().ToArray();
            // When the Payload Format Indicator is set to 1, (MqttPayloadFormatIndicator.CharacterData)
            // a MIME content type descriptor is expected (but not mandatory). Any valid UTF-8 String can be used.
            if (m2.MetaData.ContentType == "application/octet-stream")
                m.PayloadFormatIndicator = MQTTnet.Protocol.MqttPayloadFormatIndicator.Unspecified;
            else
                m.PayloadFormatIndicator = MQTTnet.Protocol.MqttPayloadFormatIndicator.CharacterData;

            m.UserProperties = new System.Collections.Generic.List<MQTTnet.Packets.MqttUserProperty>();
            m.UserProperties.AddRange(
                from key in m2.MetaData.Properties.Keys
                select new MQTTnet.Packets.MqttUserProperty(key, m2.MetaData.Properties[key])
            );

            return m;

            //m.Topic - This would come from the topic where the message is stored/retreived

            // Do I add these to the metadata?

            // For the request-response pattern:
            //m.CorrelationData
            //m.ResponseTopic


            // Very MQTT specific - probably store these in properties

            //m.QualityOfServiceLevel

            //m.SubscriptionIdentifiers

            //m.MessageExpiryInterval
            // Message Expiry Interval
            //A client can set the message expiry interval in seconds for each PUBLISH message individually.
            //This interval defines the period of time that the broker stores the PUBLISH message for any matching
            //subscribers that are not currently connected.When no message expiry interval is set, the broker must
            //store the message for matching subscribers indefinitely.When the retained = true option is set on the
            //PUBLISH message, this interval also defines how long a message is retained on a topic.
            //https://www.hivemq.com/blog/mqtt5-essentials-part4-session-and-message-expiry/
            //  When the session for a client expires, all of the messages that are queued for the client expire with
            // the session, regardless of the individual message expiry status.

            //m.topicalias - Topic Aliases are an integer value that can be used as a
            //substitute for topic names. A sender can set the Topic Alias value in the PUBLISH message,
            //following the topic name. The message receiver then processes the message like any other
            //PUBLISH and persists a mapping between the Integer (Topic Alias) and String (Topic Name).
        }
    }
}
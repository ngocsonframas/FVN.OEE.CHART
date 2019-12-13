﻿using System.Runtime.Serialization;

namespace MSharp.Framework.Services.Globalization
{
    [DataContract]
    internal class GoogleAutoDetectJsonResponseRootObject
    {
        [DataMember]
        public GoogleAutoDetectJsonResponseData data { get; set; }
    }

    [DataContract]
    internal class GoogleAutoDetectJsonResponseData
    {
        [DataMember]
        public GoogleAutoDetectJsonResponseDetection[][] detections { get; set; }
    }

    [DataContract]
    internal class GoogleAutoDetectJsonResponseDetection
    {
        [DataMember]
        public string language { get; set; }
        [DataMember]
        public bool isReliable { get; set; }
        [DataMember]
        public float confidence { get; set; }
    }

    /// <summary>
    /// Response returned by Google API for each auto-detect language request
    /// </summary>
    public class GoogleAutodetectResponse
    {
        /// <summary>ISO Code</summary>
        public string ISOCode { get; private set; }
        /// <summary>Confidence [0;1] about the detection</summary>
        public double? Confidence { get; private set; }
        // public bool IsReliable { get; set; }    // Deprecated

        /// <summary>Language detected based on iso639-1</summary>
        public ILanguage Language
        {
            get
            {
                var iso6391Code = ISOCode.Substring(0, 2).ToLowerInvariant(); // ISO639-1 are two letters code, but for Chinese Google returns 2 different codes (zh-CN for simplified and zh-TW for traditional)
                return Database.Find<ILanguage>(l => l.IsoCode.ToLowerInvariant() == iso6391Code);
            }
        }

        /// <summary>
        /// Initialize a new Google auto-detect response
        /// </summary>
        public GoogleAutodetectResponse(string isoCode, double? confidence)
        {
            ISOCode = isoCode;
            Confidence = confidence;
        }
    }
}
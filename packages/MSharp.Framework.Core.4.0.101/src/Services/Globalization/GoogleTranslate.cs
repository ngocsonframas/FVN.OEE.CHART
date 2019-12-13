﻿using System.Runtime.Serialization;

namespace MSharp.Framework.Services.Globalization
{
    [DataContract]
    internal class GoogleTranslateJsonResponseRootObject
    {
        [DataMember]
        public GoogleTranslateJsonResponseData data { get; set; }
    }

    [DataContract]
    internal class GoogleTranslateJsonResponseData
    {
        [DataMember]
        public GoogleTranslateJsonResponseTranslation[] translations { get; set; }
    }

    [DataContract]
    internal class GoogleTranslateJsonResponseTranslation
    {
        [DataMember]
        public string translatedText { get; set; }
    }
}
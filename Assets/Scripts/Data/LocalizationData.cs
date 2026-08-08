using System;
using System.Collections.Generic;

namespace GachaGame.Data
{
    // Flat key/value language table (JsonUtility can't serialize a top-level Dictionary, so this
    // mirrors it with parallel lists) - the shape LocalizationManager loads from
    // Resources/Localization/{code}.json.
    [Serializable]
    public class LocalizationData
    {
        public string languageCode;
        public List<string> keys = new();
        public List<string> values = new();
    }
}

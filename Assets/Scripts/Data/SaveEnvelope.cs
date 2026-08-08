using System;

namespace GachaGame.Data
{
    // Wraps the serialized save payload with an HMAC so SaveDataValidator can detect tampering
    // independently of the AES layer that keeps the payload confidential.
    [Serializable]
    public class SaveEnvelope
    {
        public string payload;
        public string hmac;
    }
}

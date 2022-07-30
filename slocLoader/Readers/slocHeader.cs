namespace slocLoader.Readers {

    public readonly struct slocHeader {

        public readonly int ObjectCount;
        
        public readonly slocAttributes Attributes;

        public slocHeader(int objectCount, slocAttributes attributes = slocAttributes.None) {
            ObjectCount = objectCount;
            Attributes = attributes;
        }

        public slocHeader(int objectCount, byte attributes) {
            ObjectCount = objectCount;
            Attributes = (slocAttributes) attributes;
        }

    }

}

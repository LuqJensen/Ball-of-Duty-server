namespace AsyncSocket
{
    public struct ReadResult
    {
        public int BytesRead;
        public byte[] Buffer;

        public ReadResult(int bytesRead, byte[] buffer)
        {
            this.BytesRead = bytesRead;
            this.Buffer = buffer;
        }
    }
}
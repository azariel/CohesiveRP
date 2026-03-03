namespace CohesiveRP.Common.Utils
{
    public static class StreamUtils
    {
        // ********************************************************************
        //                            Public
        // ********************************************************************
        public static Stream ToStream(this string content)
        {
            MemoryStream _Stream = new MemoryStream();
            StreamWriter _Writer = new StreamWriter(_Stream);
            _Writer.Write(content);
            _Writer.Flush();
            _Stream.Position = 0;
            return _Stream;
        }
    }
}

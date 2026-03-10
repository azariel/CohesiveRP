namespace CohesiveRP.Common.Utils
{
    public static class TokensUtils
    {
        public static int Count(string content)
        {
            // TODO: implement tokens counter
            return content?.Split().Length ?? -1;
        }
    }
}

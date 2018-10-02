namespace Improbable.Gdk.Core.Utility
{
    public static class CommandRequestIdGenerator
    {
        private static long nextRequestId;

        public static long GetNext()
        {
            nextRequestId++;

            return nextRequestId;
        }
    }
}

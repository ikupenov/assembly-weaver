namespace MonoCecilWeaver.Handlers
{
    public static class LoggerFactory
    {
        public static ILogger CreateLogger(string filePath) => 
            new FileLogger(filePath);
    }
}

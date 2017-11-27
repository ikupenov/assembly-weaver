using System.IO;

namespace MonoCecilWeaver.Handlers
{
    public class FileLogger : ILogger
    {
        private readonly string filePath;

        public FileLogger(string filePath)
        {
            this.filePath = filePath;
        }

        public void Log(string content)
        {
            using (var writer = new StreamWriter(this.filePath, true))
            {
                writer.WriteLine(content);
            }
        }
    }
}

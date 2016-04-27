using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ConfReload
{
    public class Configuration : IDisposable
    {
        private volatile XmlDocument _document;
        private volatile bool _outOfDate = true;
        private readonly FileSystemWatcher _watcher;
        private readonly Object _syncRoot = new object();
        private readonly string _filePath;

        public static Configuration Create(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                throw new Exception("Configuration not found for path " + fileInfo.FullName);
            }
            return new Configuration(filePath);
        }

        private Configuration(string filePath)
        {
            _filePath = filePath;
            RefreshConfiguration();
            var fileInfo = new FileInfo(filePath);
            string confFileName = fileInfo.Name;
            string directoryPath = fileInfo.Directory.FullName;
            _watcher = new FileSystemWatcher(directoryPath);
            _watcher.Changed += (sender, eventArgs) =>
            {
                if (eventArgs.Name == confFileName)
                {
                    RefreshConfiguration();
                }
            };
            _watcher.EnableRaisingEvents = true;
        }

        public void Dispose()
        {
            if (_watcher != null)
            {
                _watcher.Dispose();
            }
        }


        public string GetValue(string key)
        {
            if (_outOfDate)
            {
                RefreshConfiguration();
            }
            var node = _document.SelectSingleNode("*/" + key);
            if (node == null)
            {
                return "unknown";
            }
            return node.InnerText;
        }

        private void RefreshConfiguration()
        {
            lock (_syncRoot)
            {
                _outOfDate = true;
                var doc = new XmlDocument();
                try
                {
                    using (var stream = new FileStream(_filePath, FileMode.Open, FileAccess.Read))
                    {
                        doc.Load(stream);
                    }
                    _document = doc;
                    _outOfDate = false;
                }
                catch (Exception ex)
                {
                    // TODO replace with a common logging call
                    Console.Error.WriteLine("Error while parsing configuration at " + _filePath);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NFluent;
using NUnit.Framework;

namespace ConfReload.Tests
{
    public class ConfigurationTest
    {
        private Configuration _configuration;

        [Test]
        public void Should_read_configuration()
        {
            // given
            _configuration = Configuration.Create("resources/example.config");
            // when
            var url = _configuration.GetValue("url");
            // then
            Check.That(url).IsEqualTo("http://localhost");
        }

        [Test]
        public void Should_get_new_configuration_when_file_is_updated()
        {
            // given
            var path = "resources/update.config";
            _configuration = Configuration.Create(path);
            // when
            WriteFileContent(path, "<conf><url>http://new</url></conf>");
            // then
            var url = _configuration.GetValue("url");
            Check.That(url).IsEqualTo("http://new");
        }

        [Test]
        public void Should_keep_old_configuration_when_new_configuration_invalid()
        {
            // given
            var path = "resources/update2bad.config";
            _configuration = Configuration.Create(path);
            // when
            WriteFileContent(path, "<conf><url>http://new</url></a></conf>");
            // then
            var url = _configuration.GetValue("url");
            Check.That(url).IsEqualTo("http://localhost");
        }

        public void WriteFileContent(string path, string content)
        {
            using (var stream = new FileStream(path, FileMode.Truncate, FileAccess.Write))
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                stream.Write(bytes, 0, bytes.Length);
            }
            Thread.Sleep(300);
        }

        [TearDown]
        public void CleanUpConfiguration()
        {
            if (_configuration != null)
            {
                _configuration.Dispose();
            }
        }
    }
}

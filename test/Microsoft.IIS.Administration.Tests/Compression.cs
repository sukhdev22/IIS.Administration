// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.


namespace Microsoft.IIS.Administration.Tests
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.IO;
    using System.Net.Http;
    using Xunit;

    public class Compression
    {
        public const string TEST_SITE_NAME = "compression_test_site";
        public static readonly string COMPRESSION_URL = $"{Configuration.TEST_SERVER_URL}/api/webserver/http-response-compression";

        [Fact]
        public void ChangeAllProperties()
        {
            using(HttpClient client = ApiHttpClient.Create()) {

                // Web Server Scope
                JObject webServerFeature = GetCompressionFeature(client, null, null);

                ChangeAndRestoreProps(client, webServerFeature);

                // Site Scope
                Sites.EnsureNoSite(client, TEST_SITE_NAME);
                JObject site = Sites.CreateSite(client, TEST_SITE_NAME, 53010, Sites.TEST_SITE_PATH);
                JObject siteFeature = GetCompressionFeature(client, site.Value<string>("name"), null);

                ChangeAndRestoreProps(client, siteFeature);

                // Application Scope
                JObject app = Applications.CreateApplication(client, "test_app", Path.Combine(Sites.TEST_SITE_PATH, "test_application"), site);
                JObject appFeature = GetCompressionFeature(client, site.Value<string>("name"), app.Value<string>("path"));
                Assert.NotNull(appFeature);

                ChangeAndRestoreProps(client, appFeature);

                // Vdir Scope
                JObject vdir = VirtualDirectories.CreateVdir(client, "test_vdir", Path.Combine(Sites.TEST_SITE_PATH, "test_vdir"), site, true);
                JObject vdirFeature = GetCompressionFeature(client, site.Value<string>("name"), vdir.Value<string>("path"));
                Assert.NotNull(vdirFeature);

                ChangeAndRestoreProps(client, vdirFeature);

                // Directory Scope
                var dirName = "test_directory";
                var dirPath = Path.Combine(Sites.TEST_SITE_PATH, dirName);
                if (!Directory.Exists(dirPath)) {
                    Directory.CreateDirectory(dirPath);
                }

                JObject directoryFeature = GetCompressionFeature(client, site.Value<string>("name"), $"/{dirName}");
                Assert.NotNull(directoryFeature);

                ChangeAndRestoreProps(client, directoryFeature);

                Sites.DeleteSite(client, Utils.Self(site));
            }
        }

        private static void ChangeAndRestoreProps(HttpClient client, JObject feature)
        {
            JObject cachedFeature = new JObject(feature);

            feature["directory"] = Sites.TEST_SITE_PATH;
            feature["do_disk_space_limitting"] = !feature.Value<bool>("do_disk_space_limitting");
            feature["max_disk_space_usage"] = feature.Value<long>("max_disk_space_usage") + 1;
            feature["min_file_size"] = feature.Value<long>("min_file_size") + 1;
            feature["do_dynamic_compression"] = !feature.Value<bool>("do_dynamic_compression");
            feature["do_static_compression"] = !feature.Value<bool>("do_static_compression");

            JObject metaData = (JObject)feature["metadata"];
            metaData["override_mode"] = metaData.Value<string>("override_mode") == "allow" ? "deny" : "allow";

            string result;
            Assert.True(client.Patch(Utils.Self(feature), JsonConvert.SerializeObject(feature), out result));

            JObject newFeature = JsonConvert.DeserializeObject<JObject>(result);

            Assert.True(Utils.JEquals<string>(feature, newFeature, "directory"));
            Assert.True(Utils.JEquals<bool>(feature, newFeature, "do_disk_space_limitting"));
            Assert.True(Utils.JEquals<string>(feature, newFeature, "max_disk_space_usage"));
            Assert.True(Utils.JEquals<string>(feature, newFeature, "min_file_size"));
            Assert.True(Utils.JEquals<bool>(feature, newFeature, "do_dynamic_compression"));
            Assert.True(Utils.JEquals<bool>(feature, newFeature, "do_static_compression"));

            Assert.True(Utils.JEquals<string>(feature, newFeature, "metadata.override_mode"));

            Assert.True(client.Patch(Utils.Self(feature), JsonConvert.SerializeObject(cachedFeature), out result));
        }

        public static JObject GetCompressionFeature(HttpClient client, string siteName, string path)
        {
            if (path != null) {
                if (!path.StartsWith("/")) {
                    throw new ArgumentException("path");
                }
            }

            string content;
            if (!client.Get(COMPRESSION_URL + "?scope=" + siteName + path, out content)) {
                return null;
            }

            return Utils.ToJ(content);
        }
    }
}

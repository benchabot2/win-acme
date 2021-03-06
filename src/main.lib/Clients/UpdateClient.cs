﻿using Newtonsoft.Json;
using PKISharp.WACS.Services;
using System;
using System.Threading.Tasks;

namespace PKISharp.WACS.Clients
{
    class UpdateClient
    {
        private readonly ILogService _log;
        private readonly ProxyService _proxy;

        public UpdateClient(ILogService log, ProxyService proxy)
        {
            _log = log;
            _proxy = proxy;
        }

        public async Task CheckNewVersion(RunLevel runLevel)
        {
            try
            {
                var httpClient = _proxy.GetHttpClient();
                var json = await httpClient.GetStringAsync("https://www.win-acme.com/version.json");
                if (string.IsNullOrEmpty(json))
                {
                    throw new Exception("Empty result");
                }
                var data = JsonConvert.DeserializeObject<VersionCheckData>(json);
                if (data == null || data.Latest == null || data.Latest.Build == null)
                {
                    throw new Exception("Invalid result");
                }
                var latestVersion = new Version(data.Latest.Build);
                if (latestVersion > VersionService.SoftwareVersion)
                {
                    var updateInstruction = VersionService.DotNetTool ?
                        "Use \"dotnet tool update win-acme\" to update." : 
                        "Download from https://www.win-acme.com/";
                    _log.Warning($"New version {{latestVersion}} available! {updateInstruction}", latestVersion);
                }
            } 
            catch (Exception ex)
            {
                _log.Error(ex, "Version check failed");
            }
        }

        private class VersionCheckData 
        {
            public VersionData? Latest { get; set; }
        }

        private class VersionData
        {
            public string? Name { get; set; }
            public string? Tag { get; set; }
            public string? Build { get; set; }
        }
    }
}

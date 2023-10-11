﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Lampac.Engine.CORE;
using Microsoft.Extensions.Caching.Memory;
using System;
using Shared.Engine.SISI;
using Shared.Engine.CORE;
using SISI;
using Lampac.Models.SISI;

namespace Lampac.Controllers.Xhamster
{
    public class ViewController : BaseSisiController
    {
        [HttpGet]
        [Route("xmr/vidosik.m3u8")]
        async public Task<ActionResult> Index(string uri)
        {
            if (!AppInit.conf.Xhamster.enable)
                return OnError("disable");

            var proxyManager = new ProxyManager("xmr", AppInit.conf.Xhamster);
            var proxy = proxyManager.Get();

            string memKey = $"xhamster:view:{uri}";
            if (!memoryCache.TryGetValue(memKey, out StreamItem stream_links))
            {
                stream_links = await XhamsterTo.StreamLinks($"{host}/xmr/vidosik.m3u8", AppInit.conf.Xhamster.host, uri, url => HttpClient.Get(url, timeoutSeconds: 10, proxy: proxy));

                if (stream_links?.qualitys == null || stream_links.qualitys.Count == 0)
                    return OnError("stream_links", proxyManager);

                memoryCache.Set(memKey, stream_links, DateTime.Now.AddMinutes(AppInit.conf.multiaccess ? 20 : 5));
            }

            return OnResult(stream_links, AppInit.conf.Xhamster, proxy);
        }
    }
}

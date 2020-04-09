﻿using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using static pmcenter.Methods.H2Helper;

namespace pmcenter
{
    public partial class Methods
    {
        public static partial class UpdateHelper
        {
            public static async Task DownloadLangAsync()
            {
                Log("Starting automatic language file update...", "BOT");
                await DownloadFileAsync(
                    new Uri(Vars.CurrentConf.LangURL),
                    Path.Combine(Vars.AppDirectory, "pmcenter_locale.json")
                ).ConfigureAwait(false);
            }
        }
    }
}

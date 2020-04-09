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
            /// <summary>
            /// Download update to filesystem and extract.
            /// You must run Conf.CheckForUpdatesAsync() in order to pass the latestUpdate argument.
            /// </summary>
            /// <param name="latestUpdate">The information of the latest update</param>
            /// <param name="localizationIndex">The target localization's index</param>
            /// <returns></returns>
            public static async Task DownloadUpdatesAsync(Update2 latestUpdate, int localizationIndex = 0)
            {
                Log("Starting update download... (pmcenter_update.zip)");
                Log($"From address: {latestUpdate.UpdateCollection[localizationIndex].UpdateArchiveAddress}");
                await DownloadFileAsync(
                    new Uri(latestUpdate.UpdateCollection[localizationIndex].UpdateArchiveAddress),
                    Path.Combine(Vars.AppDirectory, "pmcenter_update.zip")
                ).ConfigureAwait(false);
                Log("Download complete. Extracting...");
                using (ZipArchive Zip = ZipFile.OpenRead(Path.Combine(Vars.AppDirectory, "pmcenter_update.zip")))
                {
                    foreach (ZipArchiveEntry Entry in Zip.Entries)
                    {
                        Log($"Extracting: {Path.Combine(Vars.AppDirectory, Entry.FullName)}");
                        Entry.ExtractToFile(Path.Combine(Vars.AppDirectory, Entry.FullName), true);
                    }
                }
                Log("Cleaning up temporary files...");
                File.Delete(Path.Combine(Vars.AppDirectory, "pmcenter_update.zip"));
            }
        }
    }
}

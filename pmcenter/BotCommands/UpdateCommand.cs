﻿using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static pmcenter.Methods;

namespace pmcenter.Commands
{
    internal class UpdateCommand : ICommand
    {
        public bool OwnerOnly => true;

        public string Prefix => "update";

        public async Task<bool> ExecuteAsync(TelegramBotClient botClient, Update update)
        {
            try
            {
                var Latest = Conf.CheckForUpdates();
                var CurrentLocalizedIndex = Conf.GetUpdateInfoIndexByLocale(Latest, Vars.CurrentLang.LangCode);
                if (Conf.IsNewerVersionAvailable(Latest))
                {
                    var UpdateString = Vars.CurrentLang.Message_UpdateAvailable
                        .Replace("$1", Latest.Latest)
                        .Replace("$2", Latest.UpdateCollection[CurrentLocalizedIndex].Details)
                        .Replace("$3", Methods.GetUpdateLevel(Latest.UpdateLevel));
                    _ = await botClient.SendTextMessageAsync(
                        update.Message.From.Id,
                        UpdateString, ParseMode.Markdown,
                        false,
                        Vars.CurrentConf.DisableNotifications,
                        update.Message.MessageId).ConfigureAwait(false);
                    // where difference begins
                    _ = await botClient.SendTextMessageAsync(
                        update.Message.From.Id,
                        Vars.CurrentLang.Message_UpdateProcessing,
                        ParseMode.Markdown,
                        false,
                        Vars.CurrentConf.DisableNotifications,
                        update.Message.MessageId).ConfigureAwait(false);
                    // download compiled package
                    Log("Starting update download... (pmcenter_update.zip)", "BOT");
                    Log($"From address: {Latest.UpdateCollection[CurrentLocalizedIndex].UpdateArchiveAddress}", "BOT");
                    using (var Downloader = new WebClient())
                    {
                        await Downloader.DownloadFileTaskAsync(
                            new Uri(Latest.UpdateCollection[CurrentLocalizedIndex].UpdateArchiveAddress),
                            Path.Combine(Vars.AppDirectory, "pmcenter_update.zip")).ConfigureAwait(false);
                        Log("Download complete. Extracting...", "BOT");
                        using (ZipArchive Zip = ZipFile.OpenRead(Path.Combine(Vars.AppDirectory, "pmcenter_update.zip")))
                        {
                            foreach (ZipArchiveEntry Entry in Zip.Entries)
                            {
                                Log($"Extracting: {Path.Combine(Vars.AppDirectory, Entry.FullName)}", "BOT");
                                Entry.ExtractToFile(Path.Combine(Vars.AppDirectory, Entry.FullName), true);
                            }
                        }
                        if (Vars.CurrentConf.AutoLangUpdate)
                        {
                            Log("Starting automatic language file update...", "BOT");
                            await Downloader.DownloadFileTaskAsync(
                                new Uri(Vars.CurrentConf.LangURL),
                                Path.Combine(Vars.AppDirectory, "pmcenter_locale.json")
                            ).ConfigureAwait(false);
                        }
                    }
                    Log("Cleaning up temporary files...", "BOT");
                    System.IO.File.Delete(Path.Combine(Vars.AppDirectory, "pmcenter_update.zip"));
                    _ = await botClient.SendTextMessageAsync(
                        update.Message.From.Id,
                        Vars.CurrentLang.Message_UpdateFinalizing,
                        ParseMode.Markdown,
                        false,
                        Vars.CurrentConf.DisableNotifications,
                        update.Message.MessageId).ConfigureAwait(false);
                    Log("Exiting program... (Let the daemon do the restart job)", "BOT");
                    ExitApp(0);
                    return true;
                    // end of difference
                }
                else
                {
                    _ = await botClient.SendTextMessageAsync(
                        update.Message.From.Id,
                        Vars.CurrentLang.Message_AlreadyUpToDate
                            .Replace("$1", Latest.Latest)
                            .Replace("$2", Vars.AppVer.ToString())
                            .Replace("$3", Latest.UpdateCollection[CurrentLocalizedIndex].Details),
                        ParseMode.Markdown,
                        false,
                        Vars.CurrentConf.DisableNotifications,
                        update.Message.MessageId).ConfigureAwait(false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                string ErrorString = Vars.CurrentLang.Message_UpdateCheckFailed.Replace("$1", ex.ToString());
                _ = await botClient.SendTextMessageAsync(
                    update.Message.From.Id,
                    ErrorString,
                    ParseMode.Markdown,
                    false,
                    Vars.CurrentConf.DisableNotifications,
                    update.Message.MessageId).ConfigureAwait(false);
                return true;
            }
        }
    }
}

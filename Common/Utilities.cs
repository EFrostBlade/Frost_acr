using AEAssist.Helper;
using AEAssist;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Frost.Common
{
    public static class Utilities
    {
        public static async Task UpdateTriggerlines(string job)
        {
            string currentDirectory = Share.CurrentDirectory;
            string relativePath = Path.Combine(currentDirectory, @"..\..\Triggerlines");
            string triggerlinesPath = Path.GetFullPath(relativePath);

            string listUrl = "http://111.6.43.254:26732/list-triggerlines/";
            string downloadUrl = "http://111.6.43.254:26732/download-triggerlines/";
            string saveDirectory = triggerlinesPath;

            using (HttpClient client = new HttpClient())
            {
                // 获取文件列表
                HttpResponseMessage listResponse = await client.GetAsync($"{listUrl}{job}");
                if (listResponse.IsSuccessStatusCode)
                {
                    string listContent = await listResponse.Content.ReadAsStringAsync();
                    var files = JsonConvert.DeserializeObject<Dictionary<string, List<Dictionary<string, string>>>>(listContent)["files"];

                    // 获取以 [Frost] 开头的 JSON 文件列表
                    var localFiles = Directory.GetFiles(saveDirectory, "[Frost]*.json");
                    int count = 0;


                    // 删除本地不需要的文件
                    foreach (var localFile in localFiles)
                    {
                        var fileName = Path.GetFileName(localFile);
                        if (!files.Exists(f => f["filename"] == fileName))
                        {
                            File.Delete(localFile);
                            LogHelper.Print("更新Frost时间轴", $" {fileName} 已删除");
                        }
                    }
                    // 下载需要更新的文件
                    foreach (var file in files)
                    {
                        string fileName = file["filename"];
                        string fileHash = file["hash"];
                        string localFilePath = Path.Combine(saveDirectory, fileName);


                        if (!File.Exists(localFilePath) || CalculateFileHash(localFilePath) != fileHash)
                        {
                            HttpResponseMessage downloadResponse = await client.GetAsync($"{ downloadUrl}{job}/{fileName}");
                            if (downloadResponse.IsSuccessStatusCode)
                            {
                                byte[] fileBytes = await downloadResponse.Content.ReadAsByteArrayAsync();
                                File.WriteAllBytes(localFilePath, fileBytes);
                                LogHelper.Print("更新Frost时间轴", $" {fileName} 下载成功");
                                count++;
                            }
                            else
                            {
                                LogHelper.Print("更新Frost时间轴", $" {fileName} 下载失败");
                            }
                        }
                    }
                    if (count == 0)
                    {
                        LogHelper.Print("更新Frost时间轴", "所有时间轴已是最新");
                    }
                    else
                    {
                        LogHelper.Print("更新Frost时间轴", $"更新了{files.Count}个时间轴");

                    }
                }
                else
                {
                    LogHelper.Print("更新Frost时间轴", "获取时间轴列表失败");
                }
            }
        }

        private static string CalculateFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }
}

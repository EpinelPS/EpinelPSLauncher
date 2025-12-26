using EpinelPSLauncher.Models;
using EpinelPSLauncher.Utils;
using Google.Protobuf;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ZstdSharp;
using static EpinelPSLauncher.Utils.DataUtils;

namespace EpinelPSLauncher.Clients
{
    public class GameDownloader
    {
        public static GameDownloader Instance { get; }= new();

        private readonly HttpClient client;
        private LauncherVersion? versionInfo;

        private FileListing? fileListing;
        private DecompressedData? fileData;
        private string? chunksUrl;

        public long BytesDownloaded { get; set; }
        public long BytesTotal { get; set; }
        public string DownloadPath { get; set; } = "f:\\";

        public GameDownloader()
        {
            // ignore SSL errors
            var handler = new HttpClientHandler
            {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) =>
                    {
                        return true;
                    }
            };

            client = new(handler);
        }

        public async Task FetchVersionInfoAsync()
        {
            client.DefaultRequestHeaders.Referrer = new Uri("https://www.jupiterlauncher.com/api/v1/fleet.repo.game.RepoSVC/GetVersion");

            var content = new StringContent("{\"game_id\": 16601,\"branch_id\": 1}");
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var versionResponse = await client.PostAsync("https://www.jupiterlauncher.com/api/v1/fleet.repo.game.RepoSVC/GetVersion", content);
            client.DefaultRequestHeaders.Referrer = null;

            var versionResponseString = await versionResponse.Content.ReadAsStringAsync();

            versionInfo = JsonSerializer.Deserialize(versionResponseString, SourceGenerationContext.Default.LauncherVersion) ??
                throw new Exception("failed to deserialize version information");

            chunksUrl = versionInfo.version_info.cos_repo_files[1].cdn_root + "/chunksv2/";
            BytesTotal = long.Parse(versionInfo.version_info.installer_size);
        }

        public async Task FetchManifestAsync()
        {
            if (versionInfo == null) throw new InvalidOperationException("FetchVersionInfoAsync must be called");

            var manifestRequest = await client.GetAsync(versionInfo.version_info.cos_repo_files[1].cdn_root + versionInfo.version_info.cos_repo_files[1].manifest_files[0].file_url);
            var manifestEncryptedBytes = await manifestRequest.Content.ReadAsByteArrayAsync();

            TeaEncryption.Decrypt(manifestEncryptedBytes, Encoding.ASCII.GetBytes(@"3.14159265358979"), out byte[] decryptedManifest);

            ManifestFile ms = new();
            ms.MergeFrom(new CodedInputStream(decryptedManifest));

            if (ms.Data.Length != ms.Header.Size)

            {
                throw new Exception("invalid data length");
            }

            using var decompressor = new Decompressor();
            var decompressed = decompressor.Unwrap(ms.Data.ToArray()).ToArray();

            var encKey = ConvertHexStringToByteArray(versionInfo.version_info.cos_access_info[1].manifest_encrytion_key);

            fileData = new();
            fileData.MergeFrom(new CodedInputStream(decompressed));

            var pathTableRaw = Decrypt(encKey, [.. fileData.StringsEncrypted]);

            fileListing = new();
            fileListing.MergeFrom(new CodedInputStream(pathTableRaw));
        }

        public async Task StartDownloadAsync()
        {
            if (versionInfo == null) throw new InvalidOperationException("FetchVersionInfoAsync must be called");

            if (fileListing == null || fileData == null || chunksUrl == null) throw new InvalidOperationException("FetchManifestAsync must be called first");

            if (!DownloadPath.EndsWith('/'))
                DownloadPath += "/";

            await Parallel.ForEachAsync(fileListing.Paths.Select((x, i) => (Value: x, Index: i)),
                async (x, y) =>
                {
                    await DownloadFileAsync(x.Value, fileData.FileMapping[x.Index]);
                });

            Configuration.Instance.GameVersion = versionInfo.version_info.version_name;
            Configuration.Save();
        }

        private async Task DownloadFileAsync(string item, FileEntry fileData)
        {
            if (fileData.Flags == 16) return;

            item = item.Replace("\\", "/");

            var physicalPath = DownloadPath + item;

            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath) ?? throw new Exception("shouldn't be null"));

            if (File.Exists(physicalPath))
                File.Delete(physicalPath);

            using FileStream fs = File.OpenWrite(physicalPath);
            foreach (var item2 in fileData.Chunks)
            {
                var serverFileName = Convert.ToHexString(item2.Filename.ToByteArray()).ToLower() + item2.DecompressedSize.ToString("x") + ".wgc";
                var serverFilePath = chunksUrl + item2.Filename.ToArray()[0].ToString("x2") + "/" + serverFileName;

                var result = await client.GetStreamAsync(serverFilePath) ?? throw new Exception("failed to fetch " + serverFilePath);

                await WriteChunkAsync(fs, result, item2);
            }
        }

        private async Task WriteChunkAsync(FileStream file, Stream dataStream, ChunkEntry item2)
        {
            byte[] finalResult;
            switch (item2.CompressionType)
            {
                case 0:
                    await dataStream.CopyToAsync(file);
                    BytesDownloaded += item2.CompressedSize;
                    return;
                case 1:
                    try
                    {
                        await DecompressLZMA2(dataStream, (int)item2.DecompressedSize, (int)item2.CompressedSize).CopyToAsync(file);
                        BytesDownloaded += item2.CompressedSize;
                        return;
                    }
                    catch
                    {
                        Debug.WriteLine("lzma failed");
                        return;
                    }
                case 2:
                    finalResult = await DecompressLZ4(dataStream, (int)item2.DecompressedSize, (int)item2.CompressedSize);
                    break;
                case 3:
                    finalResult = await DecompressZSTD(dataStream);
                    break;
                default:
                    throw new NotImplementedException();
            }

            if (item2.DecompressedSize != finalResult.Length)
            {
                throw new Exception($"expected {item2.DecompressedSize} bytes to be decompressed, but got {finalResult.Length} bytes");
            }

            // todo: check hashes

            file.Write(finalResult);
            BytesDownloaded += item2.CompressedSize;
        }
    }
}

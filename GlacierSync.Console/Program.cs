using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Amazon.Glacier;
using Amazon.Glacier.Model;
using Amazon.Runtime;
using Ionic.Zip;

namespace GlacierSync.Console
{
    class Program
    {
        private static string DirectoryToArchive;
        private static string BackupFilePath;
        private static string VaultName;
        private static string ArchiveDescription;
        const long partSize = 4194304;

        static int progressUpdateLen = 0;
        static string progressUpdate = "";

        private static void WriteZipProgress(object sender, SaveProgressEventArgs e)
        {
            if (e.CurrentEntry != null)
            {
                if (e.EventType == ZipProgressEventType.Saving_AfterWriteEntry)
                {
                    System.Console.SetCursorPosition(0, 0);
                    progressUpdateLen = progressUpdate.Length;
                    progressUpdate = string.Format("\rSaving {0} of {1} [{2}]",
                                                   e.EntriesSaved, e.EntriesTotal,
                                                   GetProgressBar(e.EntriesSaved, e.EntriesTotal, 40)).PadRight(progressUpdateLen);
                    System.Console.Write(progressUpdate);
                }
            }
            if (e.EventType == ZipProgressEventType.Saving_Completed)
            {
                System.Console.WriteLine();
            }
        }

        private static void WriteFileUploadProgress(long current, long total)
        {
            System.Console.SetCursorPosition(0, 2);
            progressUpdateLen = progressUpdate.Length;
            progressUpdate = string.Format("\rUploaded: {0} of {1} [{2}]",
                                           current, total, GetProgressBar(current, total, 40)).PadRight(progressUpdateLen);
            System.Console.Write(progressUpdate);
        }

        private static string GetProgressBar(long current, long total, int len, char symbol = '=')
        {
            if (total == 0)
                return "".PadRight(len);

            var percentComplete = (decimal)current / total;
            var numSymbolsToDraw = (int)Math.Floor(percentComplete * len);
            var symbols = new char[numSymbolsToDraw];
            for (var i = 0; i < numSymbolsToDraw; i++)
            {
                symbols[i] = symbol;
            }
            return new string(symbols).PadRight(len);
        }

        public static void Main(string[] args)
        {
            // Required Config Values
            DirectoryToArchive = ConfigurationManager.AppSettings["DirectoryToArchive"];
            if(string.IsNullOrEmpty(DirectoryToArchive))
                throw new ConfigurationException("Please specify the 'DirectoryToArchive' setting in the application configuration file.");

            VaultName = ConfigurationManager.AppSettings["VaultName"];
            if (string.IsNullOrEmpty(VaultName))
                throw new ConfigurationException("Please specify the 'VaultName' setting in the application configuration file.");

            BackupFilePath = ConfigurationManager.AppSettings["BackupFilePath"];
            if (string.IsNullOrEmpty(BackupFilePath))
            {
                BackupFilePath = Path.Combine(Path.GetTempPath(),
                                              Path.GetTempFileName()).Replace(".tmp", ".zip");
            }

            ArchiveDescription = ConfigurationManager.AppSettings["ArchiveDescription"];
            ArchiveDescription = (string.IsNullOrEmpty(ArchiveDescription))
                                     ? string.Format("Archive of {0}", DirectoryToArchive)
                                     : ArchiveDescription;

            using (var backup = new ZipFile())
            {
                backup.AddDirectory(DirectoryToArchive);

                backup.SaveProgress += WriteZipProgress;

                backup.Save(BackupFilePath);
            }

            AmazonGlacier client;
            var partChecksumList = new List<string>();
            try
            {
                using (client = new AmazonGlacierClient(Amazon.RegionEndpoint.USEast1))
                {
                    System.Console.WriteLine("Uploading an archive.");
                    string uploadId = InitiateMultipartUpload(client);
                    partChecksumList = UploadParts(uploadId, client);
                    string archiveId = CompleteMPU(uploadId, client, partChecksumList);
                    System.Console.WriteLine();
                    System.Console.WriteLine("Archive ID: {0}", archiveId);
                }

                File.Delete(BackupFilePath);

                System.Console.WriteLine("Operations successful. To continue, press Enter");
                System.Console.ReadKey();
            }
            catch (AmazonGlacierException e)
            {
                System.Console.WriteLine(e.Message);
            }
            catch (AmazonServiceException e)
            {
                System.Console.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }

        static string InitiateMultipartUpload(AmazonGlacier client)
        {
            var initiateMPUrequest = new InitiateMultipartUploadRequest()
            {
                VaultName = VaultName,
                PartSize = partSize,
                ArchiveDescription = ArchiveDescription
            };

            var initiateMPUresponse = client.InitiateMultipartUpload(initiateMPUrequest);

            return initiateMPUresponse.InitiateMultipartUploadResult.UploadId;
        }

        static List<string> UploadParts(string uploadID, AmazonGlacier client)
        {
            var partChecksumList = new List<string>();
            long currentPosition = 0;
            var buffer = new byte[Convert.ToInt32(partSize)];

            long fileLength = new FileInfo(BackupFilePath).Length;
            WriteFileUploadProgress(currentPosition, fileLength);
            using (var fileToUpload = new FileStream(BackupFilePath, FileMode.Open, FileAccess.Read))
            {
                while (fileToUpload.Position < fileLength)
                {
                    var uploadPartStream = GlacierUtils.CreatePartStream(fileToUpload, partSize);
                    var checksum = TreeHashGenerator.CalculateTreeHash(uploadPartStream);
                    partChecksumList.Add(checksum);
                    // Upload part.
                    var uploadMPUrequest = new UploadMultipartPartRequest()
                    {
                        VaultName = VaultName,
                        Body = uploadPartStream,
                        Checksum = checksum,
                        UploadId = uploadID
                    };
                    uploadMPUrequest.SetRange(currentPosition, currentPosition + uploadPartStream.Length - 1);
                    client.UploadMultipartPart(uploadMPUrequest);
                    currentPosition = currentPosition + uploadPartStream.Length;
                    WriteFileUploadProgress(currentPosition, fileLength);
                }
            }
            return partChecksumList;
        }

        static string CompleteMPU(string uploadID, AmazonGlacier client, List<string> partChecksumList)
        {
            long fileLength = new FileInfo(BackupFilePath).Length;
            var completeMPUrequest = new CompleteMultipartUploadRequest()
            {
                UploadId = uploadID,
                ArchiveSize = fileLength.ToString(),
                Checksum = TreeHashGenerator.CalculateTreeHash(partChecksumList),
                VaultName = VaultName
            };

            var completeMPUresponse = client.CompleteMultipartUpload(completeMPUrequest);
            return completeMPUresponse.CompleteMultipartUploadResult.ArchiveId;
        }
    }
}
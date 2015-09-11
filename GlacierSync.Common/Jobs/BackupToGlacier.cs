using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Amazon.Glacier;
using Amazon.Glacier.Model;
using Amazon.Runtime;
using Ionic.Zip;

using GlacierSync.Common.Utilities;

namespace GlacierSync.Common.Jobs
{
	public class BackupToGlacier
	{
		protected const long partSize = 4194304;
		protected readonly string DirectoryToArchive;
		protected readonly string BackupFilePath;
		protected readonly string VaultName;
		protected readonly string ArchiveDescription;
		protected readonly IFeedback FeedbackProvider;

		public BackupToGlacier (string directoryToArchive, string backupFilePath, string vaultName, string archiveDescription, IFeedback feedbackProvider)
		{
			DirectoryToArchive = directoryToArchive;
			BackupFilePath = backupFilePath;
			VaultName = vaultName;
			ArchiveDescription = archiveDescription;

			FeedbackProvider = feedbackProvider;
		}

		public void Execute()
		{
			var zipper = new DirectoryZipper (DirectoryToArchive, BackupFilePath, FeedbackProvider);
			zipper.Execute ();

			var partChecksumList = new List<string>();
			try
			{
				var config = new AmazonGlacierConfig{
					RegionEndpoint = Amazon.RegionEndpoint.USEast1
				};

				var credentials = new EnvironmentVariablesAWSCredentials();

				using (var client = new AmazonGlacierClient(credentials, config))
				{
					var vaults = client.ListVaults();
					if(!vaults.VaultList.Any(v => v.VaultName == VaultName))
					{
						var createVaultRequest = new CreateVaultRequest(VaultName);
						var createVaultResponse = client.CreateVault(createVaultRequest);
						if(createVaultResponse.HttpStatusCode != System.Net.HttpStatusCode.Created)
						{
							throw new ApplicationException("Error creating new vault.");
						}
					}

					FeedbackProvider.WriteFeedback("Uploading an archive.");
					string uploadId = InitiateMultipartUpload(client);
					partChecksumList = UploadParts(uploadId, client);
					string archiveId = CompleteMPU(uploadId, client, partChecksumList);
					FeedbackProvider.WriteEndOperation(string.Format("Archive ID: {0}", archiveId));
				}

				File.Delete(BackupFilePath);

				FeedbackProvider.WriteEndOperation("Operations successful. To continue, press Enter");
			}
			catch (AmazonGlacierException e)
			{
				FeedbackProvider.WriteErrorFeedback(e.Message);
			}
			catch (AmazonServiceException e)
			{
				FeedbackProvider.WriteErrorFeedback(e.Message);
			}
			catch (Exception e)
			{
				FeedbackProvider.WriteErrorFeedback(e.Message);
			}
		}

		protected void WriteFileUploadProgress(long current, long total)
		{
			var message = string.Format("\rUploaded: {0} of {1}",
			                               current, total);
			FeedbackProvider.WriteFeedbackWithPercent (message, (int)current, (int)total);
		}

		protected string InitiateMultipartUpload(AmazonGlacierClient client)
		{
			var initiateMPUrequest = new InitiateMultipartUploadRequest()
			{
				VaultName = VaultName,
				PartSize = partSize,
				ArchiveDescription = ArchiveDescription
			};

			var initiateMPUresponse = client.InitiateMultipartUpload(initiateMPUrequest);

			return initiateMPUresponse.UploadId;
		}

		protected List<string> UploadParts(string uploadID, AmazonGlacierClient client)
		{
			var partChecksumList = new List<string>();
			long currentPosition = 0;
			//var buffer = new byte[Convert.ToInt32(partSize)];

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

		protected string CompleteMPU(string uploadID, AmazonGlacierClient client, List<string> partChecksumList)
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
			return completeMPUresponse.ArchiveId;
		}
	}
}


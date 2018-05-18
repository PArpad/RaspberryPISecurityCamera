using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;
using Google.Apis.Services;
using Google.Apis.Drive.v3;
using System.IO;
using Google.Apis.Upload;
using System.Security.Cryptography.X509Certificates;
using Google.Apis.Requests;
using Google.Apis.Drive.v3.Data;

namespace RaspberryPiSecurityCam.Services
{
    public class GoogleDriveServices
    {
        private string serviceAccountEmail = "security@raspberrypisecuritycam-185321.iam.gserviceaccount.com";
        private const int KB = 0x400;
        private const int DownloadChunkSize = 256 * KB;
        private static readonly string[] Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive };
        private string secretPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(),"ClientSecrets", "ServiceAccountSecret.p12"));
        public async Task GoogleDriveUploadAsync(string ToUploadFileName,string ToUploadFilePath, string folderName, List<string> emailAddresses)
        {
        //UserCredential credential;

        var certificate = new X509Certificate2(secretPath, "notasecret", X509KeyStorageFlags.Exportable);

        ServiceAccountCredential credential = new ServiceAccountCredential(
            new ServiceAccountCredential.Initializer(serviceAccountEmail)
            {
                Scopes = Scopes
            }.FromCertificate(certificate));

        //using (var stream =
        //    new System.IO.FileStream(secretPath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
        //{
        //    string credPath = System.IO.Directory.GetCurrentDirectory();
        //    credPath = System.IO.Path.Combine(credPath, ".credentials/DriveClient.json");

        //    credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
        //        GoogleClientSecrets.Load(stream).Secrets,
        //        Scopes,
        //        "user",
        //        CancellationToken.None,
        //        new FileDataStore(credPath, true)).Result;
        //}

        // Create the service.
        var service = new DriveService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = "DriveClient",
        });

        string pageToken = null;
        List<Google.Apis.Drive.v3.Data.File> files = new List<Google.Apis.Drive.v3.Data.File>();
        do
        {
            var listRequest = service.Files.List();
            listRequest.Q = "mimeType='application/vnd.google-apps.folder'";
            listRequest.Spaces = "drive";
            listRequest.Fields = "nextPageToken, files(id, name, parents, permissions)";
            listRequest.PageToken = pageToken;
            var result = listRequest.Execute();
            foreach (var file in result.Files)
            {
                Console.WriteLine(String.Format(
                        "Found file: {0} ({1})", file.Name, file.Id));
                files.Add(file);
            }
            pageToken = result.NextPageToken;
        } while (pageToken != null);

        var folderOfToday = DateTime.Now.ToShortDateString();
        var folderOfTodayId = "";

        string folderID = "";
        foreach (Google.Apis.Drive.v3.Data.File f in files)
        {
            if (f.Name == folderName)
            {
                folderID = f.Id;
                foreach (string emailAddress in emailAddresses)
                {
                    bool hasPermission = false;
                    foreach (var permission in f.Permissions)
                    {
                        if (permission.EmailAddress == emailAddress)
                        {
                            hasPermission = true;   
                        }
                    }
                    if (!hasPermission)
                    {
                        SetPermission(folderID, service, emailAddress);
                    }
                }
            }
        }
        if (folderID == "")
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderName,
                MimeType = "application/vnd.google-apps.folder"
            };
            var request = service.Files.Create(fileMetadata);
            request.Fields = "id";
            var file = request.Execute();
            folderID = file.Id;
        }

        foreach (Google.Apis.Drive.v3.Data.File f in files)
        {
            if (f.Name == folderOfToday && f.Parents.Contains(folderID))
            {
                folderOfTodayId = f.Id;
            }
        }
        if (folderOfTodayId == "")
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = folderOfToday,
                MimeType = "application/vnd.google-apps.folder",
                Parents = new List<string>
                {
                    folderID
                }
            };
            var request = service.Files.Create(fileMetadata);
            request.Fields = "id";
            var file = request.Execute();
            folderOfTodayId = file.Id;
        }
        await UploadFileAsync(ToUploadFilePath, folderOfTodayId, credential);
        }
        /// <summary>Uploads file asynchronously.</summary>
        private Task<IUploadProgress> UploadFileAsync(string ToUploadFilePath, string folderID, ServiceAccountCredential credential)
        {
            string[] Scopes = new[] { DriveService.Scope.DriveFile, DriveService.Scope.Drive };

            var service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "UploadFileAsync",
            });

            var title = ToUploadFilePath;
            if (title.LastIndexOf('\\') != -1)
            {
                title = title.Substring(title.LastIndexOf('\\') + 1);
            }



            var uploadStream = new System.IO.FileStream(ToUploadFilePath, System.IO.FileMode.Open,
                System.IO.FileAccess.Read);

            string FileType = MimeKit.MimeTypes.GetMimeType(ToUploadFilePath);
            var file = new Google.Apis.Drive.v3.Data.File { Name = title, Parents = new List<string> { folderID } };

            var insert = service.Files.Create(file, uploadStream, FileType);

            insert.ChunkSize = FilesResource.CreateMediaUpload.MinimumChunkSize * 2;

            var task = insert.UploadAsync();

            task.ContinueWith(t =>
            { }, TaskContinuationOptions.NotOnRanToCompletion);
            task.ContinueWith(t =>
            {
                uploadStream.Dispose();
            });

            return task;
        }

        private void SetPermission(string fileId, DriveService driveService,string emailAddress)
        {
            var batch = new BatchRequest(driveService);
            BatchRequest.OnResponse<Permission> callback = delegate (
                Permission permission,
                RequestError error,
                int index,
                System.Net.Http.HttpResponseMessage message)
            {
                if (error != null)
                {
                    // Handle error
                    Console.WriteLine(error.Message);
                }
                else
                {
                    Console.WriteLine("Permission ID: " + permission.Id);
                }
            };
            Permission userPermission = new Permission()
            {
                Type = "user",
                Role = "writer",
                EmailAddress = emailAddress
            };
            var request = driveService.Permissions.Create(userPermission, fileId);
            request.Fields = "id";
            batch.Queue(request, callback);
            var task = batch.ExecuteAsync();
        }
    }
}


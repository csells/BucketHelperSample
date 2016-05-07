using Google.Storage.V1;
using Google.Storage.V1.Demo;
using System;

namespace BucketHelperSample {
  class Program {
    static void Main(string[] args) {
      if (args.Length != 1) {
        Console.WriteLine("Usage: BucketHelperSample <project-id>");
        return;
      }

      var projectId = args[0];
      ListBucketsAndObjects(projectId);

      Console.WriteLine();
      ListBucketsFilesAndFolders(projectId);
    }

    // list buckets and objects in a flat list w/o taking folders into account
    static void ListBucketsAndObjects(string projectId) {
      var client = StorageClient.Create();

      Console.WriteLine($"Buckets and Objects in {projectId}:");
      foreach (var bucket in client.ListBuckets(projectId)) {
        Console.WriteLine($"  {bucket.Name}/ [<bucket>]");
        foreach (var obj in client.ListObjects(bucket.Name, null)) {
          Console.WriteLine($"    {obj.Name} [{obj.ContentType}]");
        }
      }
    }

    // list buckets, files and folders in a tree
    static void ListBucketsFilesAndFolders(string projectId) {
      var client = StorageClient.Create();

      Console.WriteLine($"Buckets, Files and Folders in {projectId}:");
      foreach (var bucket in client.ListBuckets(projectId)) {
        ListFilesAndFolders(client, bucket.Name);
      }
    }

    // Folders can be objects of zero length of content type
    // application/x-www-form-urlencoded;charset=UTF-8 as used by the Storage Browser:
    // https://console.cloud.google.com/storage/browser?project=YOUR-PROJECT-ID
    // Folders can also be implicit in file names with prefixes containing the delimiter "/",
    // also used by the Storage Browser. The extension methods provided in BrowserHelper know
    // how to deal with both.
    static void ListFilesAndFolders(StorageClient client, string bucket, string parentFolder = "", string indent = "  ") {
      if (parentFolder == "") { Console.WriteLine($"Files and folders in bucket {bucket}:"); }

      string shortName = parentFolder == "" ? bucket : BucketHelper.ShortName(parentFolder);
      Console.WriteLine($"{indent}{shortName}/");
      indent += "  ";

      foreach (var file in client.ListFiles(bucket, parentFolder)) {
        Console.WriteLine($"{indent}{file.ShortName()} [{file.ContentType}]");
      }

      foreach (var folder in client.ListFolders(bucket, parentFolder)) {
        ListFilesAndFolders(client, bucket, folder, indent);
      }
    }

  }
}

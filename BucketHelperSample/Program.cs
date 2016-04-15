using Google;
using Google.Apis.Storage.v1.Data;
using Google.Storage.V1;
using System;
using System.Net;

namespace BucketHelperSample {
  class Program {
    static void Main(string[] args) {
      if (args.Length != 1) {
        Console.WriteLine("Usage: BucketHelperSample <project-id>");
        return;
      }

      // enumerate buckets from https://console.cloud.google.com/storage/browser?project=YOUR-PROJECT-ID
      var projectId = args[0];
      DumpBuckets(projectId);
      Console.WriteLine();
      DumpBucketsTree(projectId);
    }

    static void DumpBuckets(string projectId) {
      var client = StorageClient.Create();

      Console.WriteLine($"Buckets and Objects in {projectId}:");
      foreach (var bucket in client.ListBuckets(projectId)) {
        Console.WriteLine($"  {bucket.Name}/ [<bucket>]");
        foreach (var obj in client.ListObjects(bucket.Name, null)) {
          Console.WriteLine($"    {obj.Name} [{obj.ContentType}]");
        }
      }
    }

    static void DumpBucketsTree(string projectId) {
      var client = StorageClient.Create();
      Console.WriteLine($"Buckets, Files and Folders in {projectId}:");
      foreach (var bucket in client.ListBuckets(projectId)) {
        DumpFolder(client, bucket);
      }
    }

    static void DumpFolder(StorageClient client, Bucket bucket, string parentFolder = "", string indent = "  ") {
      // Folders can be explicit objects or implicit based on the prefixes of files
      string shortName = parentFolder == "" ? bucket.Name : BucketHelper.ShortName(parentFolder);
      string folderType = parentFolder == "" ? "<bucket>" : "";
      if (parentFolder != "") {
        try { folderType = client.GetObject(bucket.Name, parentFolder).ContentType; }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotFound) { folderType = "<implicit>"; }
      }

      Console.WriteLine($"{indent}{shortName}/ [{folderType}]");
      indent += "  ";

      foreach (var file in bucket.Files(client, parentFolder)) {
        Console.WriteLine($"{indent}{file.ShortName()} [{file.ContentType}]");
      }

      foreach (var folder in bucket.Folders(client, parentFolder)) {
        DumpFolder(client, bucket, folder, indent);
      }

    }

  }
}

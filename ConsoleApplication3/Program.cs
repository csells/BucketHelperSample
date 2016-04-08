using Google.Apis.Storage.v1.Data;
using Google.Storage.V1;
using System;
using System.IO;
using System.Linq;

namespace ConsoleApplication3 {
  class Program {
    static void Main(string[] args) {
      var projectId = "firm-site-126023";
      //TestBucketHelpers(projectId);
      EnumBucketsFlat(projectId);
      Console.WriteLine();
      EnumBucketsTree(projectId);
    }

    static void TestBucketHelpers(string projectId) {
      var client = StorageClient.FromApplicationCredentials("Demo").Result;

      // create a new file
      //var bucket = client.ListBuckets(projectId, new ListBucketsOptions { Prefix = projectId }).Single(b => b.Name == projectId);
      //var folder = new BucketFolder(client, bucket, client.GetObject(bucket.Name, "foo/foo-level-2/"));
      // NOTE: would be handy to have a UploadObject method that took a filename instead of a stream
      using (var stream = new FileStream(@"C:\Users\Chris\Downloads\gutter2.jpg", FileMode.Open)) {
        client.UploadObject(projectId, "foo/bar/quux.jpg", "image/jpeg", stream);
      }
    }

    static void EnumBucketsFlat(string projectId) {
      var client = StorageClient.FromApplicationCredentials("Demo").Result;

      Console.WriteLine($"Buckets and Objects in {projectId}:");
      var buckets = client.ListBuckets(projectId);
      foreach (var bucket in buckets) {
        Console.WriteLine($"  {bucket.Name}/");
        var objs = client.ListObjects(bucket.Name, null);
        foreach (var obj in objs) {
          Console.WriteLine($"    {obj.Name} [{obj.ContentType}]");
        }
      }
    }

    static void EnumBucketsTree(string projectId) {
      var client = StorageClient.FromApplicationCredentials("Demo").Result;
      Console.WriteLine($"Buckets, Files and Folders in {projectId}:");
      var buckets = client.ListBuckets(projectId);
      foreach (var bucket in buckets) {
        EnumFolder(new BucketFolder(client, bucket), "  ");
      }
    }

    static void EnumFolder(BucketFolder parent, string indent) {
      Console.WriteLine($"{indent}{parent.ShortName}/");
      indent += "  ";

      foreach (var file in parent.Files) {
        Console.WriteLine($"{indent}{file.ShortName} [{file.Object.ContentType}]");
      }

      foreach (var child in parent.Folders) {
        EnumFolder(child, indent);
      }

    }

  }
}

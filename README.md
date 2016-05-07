# BucketHelperSample
Google Cloud Storage sample to enumerate files and folders explicitly

# Buckets and Files and Folders, Oh My!
Google's [Cloud Storage Browser](https://console.cloud.google.com/storage/browser) perpetrates a fiction of files and folders that doesn't exist. The GCS API only has two concepts: buckets and objects.

A bucket needs a globally unique name and is a container of objects.

An object has a name, a content type and content. The name can be simple, e.g. "foo.txt" or it can have slashes in it, e.g. "foo/bar.txt" or even just "quux/". However, from the GCS API POV, there's no difference -- the only container is a bucket.

For example, I can write a program using [the GCS client lib](http://github.com/googlecloudplatform/gcloud-dotnet) from [NuGet](https://www.nuget.org/packages/Google.Storage.V1/) that looks like this:

```c#
void ListBucketsAndObjects(string projectId) {
  var client = StorageClient.Create();

  foreach (var bucket in client.ListBuckets(projectId)) {
    Console.WriteLine($"{bucket.Name}/");
    foreach (var obj in client.ListObjects(bucket.Name, null)) {
      Console.WriteLine($"  {obj.Name}");
    }
  }
}
```

Given objects with names as described above, this would be the output:

```
csells-bucket-1/
  foo.txt
  foo/bar.txt
  quux/
```

However, if you surf to the [Storage Browser](https://console.cloud.google.com/storage/browser) to see the project with these three objects, you'll see something that looks like a normal file/folder browser:

<img src="http://sellsbrothers.com/public/post-images/gcs-bhelper-1.png" />

# Implicit and Explicit Folders
The Storage Browser has interpreted one of the objects as a file and two of them as folders, one implicit and one explicit. The implicit object folder comes from the slash in "foo/bar.txt"; the slash is used as a delimiter that means "folder" as far as the Storage Browser is concerned.

The explicit folder comes from an object with a name that ends in a slash. You can create one by pressing the Create Folder button in the Storage Explorer or with the following lines of code:

```c#
var client = StorageClient.Create();
client.UploadObject(bucketName, "quux/", "", Stream.Null);

```

# Working with Folders
When you're working with buckets and objects, the ListBuckets and ListObjects methods work just fine. However, if you'd like to navigate the fictional hierarchy of files and folders the way that the Storage Browser does, you can use the BrowserHelper:

```c#
void ListBucketsFilesAndFolders(string projectId) {
  var client = StorageClient.Create();

  foreach (var bucket in client.ListBuckets(projectId)) {
    ListFilesAndFolders(client, bucket.Name);
  }
}

void ListFilesAndFolders(StorageClient client, string bucket, string parentFolder = "", string indent = "") {
  string shortName = parentFolder == "" ? bucket : BucketHelper.ShortName(parentFolder);
  Console.WriteLine($"{indent}{shortName}/");
  indent += "  ";

  foreach (var file in client.ListFiles(bucket, parentFolder)) {
    Console.WriteLine($"{indent}{file.ShortName()}");
  }

  foreach (var folder in client.ListFolders(bucket, parentFolder)) {
    ListFilesAndFolders(client, bucket, folder, indent);
  }
}

```

The output for the same list of objects looks like this:

```
csells-bucket-1/
  foo.txt
  foo/
    bar.txt
  quux/
```

The implicit and explicit folders are folded together into a list of strings at each level, so your code doesn't have to care (although if you do care, a call to StorageClient.GetObject return an object or throw an error depending on whether it's explicit or implicit). When creating your objects, your code doesn't have to explicitly create folders, since implicit folders are first class citizens as far as the BrowserHelper and the Storage Explorer are concerned. However, if you'd like to create a folder explicitly, BrowserHelper provides a helper for that, too:

```c#
var client = StorageClient.Create();
var folderObj = client.CreateFolder(bucketName, "baaz/");

```

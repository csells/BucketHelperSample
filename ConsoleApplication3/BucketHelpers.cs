using Google.Storage.V1;
using System;
using System.Collections.Generic;
using System.Linq;

public class BucketFile {
  // NOTE: the use of the type "Object" is terribly inconvenient in .NET
  Google.Apis.Storage.v1.Data.Object obj;

  public BucketFile(Google.Apis.Storage.v1.Data.Object obj) {
    if (obj == null) { throw new ArgumentNullException("obj"); }
    if (obj.Name.EndsWith("/")) { throw new ArgumentException("file names can't end in /"); }
    this.obj = obj;
  }

  public string ShortName { get { return obj.Name.Split('/').Last(); } }
  public Google.Apis.Storage.v1.Data.Object Object { get { return obj; } }
}

public class BucketFolder {
  StorageClient client;
  string bucket;
  string name;

  public BucketFolder(StorageClient client, string bucket, string name = "") {
    if (client == null) { throw new ArgumentNullException("client"); }
    if (bucket == null) { throw new ArgumentNullException("bucket"); }
    if (!string.IsNullOrEmpty(name) && !name.EndsWith("/")) { throw new ArgumentException("folder names must end in /"); }
    this.client = client;
    this.bucket = bucket;
    this.name = name;
  }

  public string ShortName {
    get {
      return string.IsNullOrEmpty(name) ? bucket : name.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
    }
  }

  // TODO: test with multiple objects in the same folder, both explicitly and implicitly
  public IEnumerable<BucketFolder> Folders {
    get {
      var prefix = name ?? "";
      return client
        .ListObjects(bucket, prefix)
        .Select(o => o.Name.Substring(prefix.Length))
        .Where(n => n.Contains('/'))
        .Select(n => n.Split('/').First())
        .Distinct()
        .Select(n => new BucketFolder(client, bucket, prefix + n + "/"));
    }
  }

  public IEnumerable<BucketFile> Files {
    get {
      var prefix = name ?? "";
      return client
        .ListObjects(bucket, prefix, new ListObjectsOptions { Delimiter = "/" })
        .Where(o => !o.Name.EndsWith("/"))
        .Select(o => new BucketFile(o));
    }
  }
}


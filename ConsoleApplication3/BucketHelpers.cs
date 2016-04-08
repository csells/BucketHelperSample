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
  public Google.Apis.Storage.v1.Data.Object Object {  get { return obj; } }
}

public class BucketFolder {
  StorageClient client;
  string bucket;
  Google.Apis.Storage.v1.Data.Object obj;

  public BucketFolder(StorageClient client, string bucket, Google.Apis.Storage.v1.Data.Object obj = null) {
    if (client == null) { throw new ArgumentNullException("client"); }
    if (bucket == null) { throw new ArgumentNullException("bucket"); }
    if (obj != null && obj.Bucket != bucket) { throw new ArgumentException("obj must be from bucket"); }
    if (obj != null && !obj.Name.EndsWith("/")) { throw new ArgumentException("folder names must end in /"); }
    this.client = client;
    this.bucket = bucket;
    this.obj = obj;
  }

  public string ShortName {
    get {
      return obj == null ? bucket : obj.Name.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Last();
    }
  }

  public Google.Apis.Storage.v1.Data.Object Object { get { return obj; } }

  public IEnumerable<BucketFolder> Folders {
    get {
      return client
        .ListObjects(bucket, obj?.Name)
        .Where(o => o.Name.EndsWith("/") && o.Name.Substring(obj?.Name.Length ?? 0).Sum(c => c == '/' ? 1 : 0) == 1)
        .Select(o => new BucketFolder(client, bucket, o));
    }
  }

  public IEnumerable<BucketFile> Files {
    get {
      return client
        .ListObjects(bucket, obj?.Name, new ListObjectsOptions { Delimiter = "/" })
        .Where(o => !o.Name.EndsWith("/"))
        .Select(o => new BucketFile(o));
    }
  }
}


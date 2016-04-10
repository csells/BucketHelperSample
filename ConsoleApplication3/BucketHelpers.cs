using Google.Apis.Storage.v1.Data;
using Google.Storage.V1;
using System;
using System.Collections.Generic;
using System.Linq;

public static class BucketHelper {
  public const char Delimiter = '/';
  public static IEnumerable<string> Folders(this Bucket bucket, StorageClient client, string parentFolder = "") {
    if (bucket == null) { throw new ArgumentNullException("this"); }
    if (client == null) { throw new ArgumentNullException("client"); }
    if (!string.IsNullOrEmpty(parentFolder) && !parentFolder.EndsWith(Delimiter.ToString())) { throw new ArgumentException("parentFolder must end in " + Delimiter); }
    if (!string.IsNullOrEmpty(parentFolder) && parentFolder == Delimiter.ToString()) { throw new ArgumentException("root folder is \"\", not " + Delimiter); }

    var prefix = parentFolder ?? "";
    return client
      .ListObjects(bucket.Name, prefix)
      .Select(o => o.Name.Substring(prefix.Length))
      .Where(n => n.Contains(Delimiter))
      .Select(n => n.Split(Delimiter).First())
      .Distinct()
      .Select(n => prefix + n + Delimiter);
  }

  public static IEnumerable<Google.Apis.Storage.v1.Data.Object> Files(this Bucket bucket, StorageClient client, string parentFolder = "") {
    if (bucket == null) { throw new ArgumentNullException("this"); }
    if (client == null) { throw new ArgumentNullException("client"); }
    if (!string.IsNullOrEmpty(parentFolder) && !parentFolder.EndsWith(Delimiter.ToString())) { throw new ArgumentException("parentFolder must end in " + Delimiter); }
    if (!string.IsNullOrEmpty(parentFolder) && parentFolder == Delimiter.ToString()) { throw new ArgumentException("root folder is \"\", not " + Delimiter); }

    var prefix = parentFolder ?? "";
    return client
      .ListObjects(bucket.Name, prefix, new ListObjectsOptions { Delimiter = Delimiter.ToString() })
      .Where(o => !o.Name.EndsWith(Delimiter.ToString()));
  }

  public static string ShortName(string name) {
    if (name == null) { throw new ArgumentNullException("name"); }
    return name.Split(new char[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).Last();
  }

  public static string ShortName(this Google.Apis.Storage.v1.Data.Object obj) {
    if (obj == null) { throw new ArgumentNullException("this"); }
    return ShortName(obj.Name);
  }
}


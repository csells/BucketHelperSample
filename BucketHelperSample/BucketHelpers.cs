using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Google.Storage.V1.Demo {
  public static class BucketHelper {
    /// <summary>
    /// The delimited used by the Google Cloud Storage Browser to delimit folder paths
    /// </summary>
    public const char Delimiter = '/';

    /// <summary>
    /// Lists the child folders of a parent folder of a bucket.
    /// </summary>
    /// <remarks>
    /// This method applies the convensions of the Google Cloud Storage Browser at
    /// https://console.cloud.google.com/storage/browser?project=YOUR-PROJECT-ID
    /// A "folder" can be an object with a name that ends in '/' or a "folder" can be implicit
    /// in a file name, e.g. foo/bar/quux.txt. This method knows how to deal with both.
    /// You can use GetObject on the returned folder name to determine if the folder
    /// is explicit or implicit.
    /// </remarks>
    /// <param name="bucket">The bucket to list the folder from. Must not be null or empty.</param>
    /// <param name="folder">The parent folder. Only child folders in this parent folder will be returned.
    /// This parameter must end in '/' or be empty or null. If it's empty or null, the root of the bucket
    /// will be used.</param>
    /// <returns>A sequence of child folder names within the parent folder of the bucket.</returns>
    public static IEnumerable<string> ListFolders(this StorageClient client, string bucket, string folder = "") {
      if (client == null) { throw new ArgumentNullException("this"); }
      if (string.IsNullOrWhiteSpace(bucket)) { throw new ArgumentOutOfRangeException("bucket must be non-empty"); }
      if (!string.IsNullOrEmpty(folder) && !folder.EndsWith(Delimiter.ToString())) { throw new ArgumentException("folder must end in " + Delimiter); }
      if (!string.IsNullOrEmpty(folder) && folder == Delimiter.ToString()) { throw new ArgumentException("root folder is \"\", not " + Delimiter); }

      var prefix = folder ?? "";
      return client
        .ListObjects(bucket, prefix)
        .Select(o => o.Name.Substring(prefix.Length))
        .Where(n => n.Contains(Delimiter))
        .Select(n => n.Split(Delimiter).First())
        .Distinct()
        .Select(n => prefix + n + Delimiter);
    }

    /// <summary>
    /// Lists the files in a folder of a bucket.
    /// </summary>
    /// <remarks>
    /// This method applies the convensions of the Google Cloud Storage Browser at
    /// https://console.cloud.google.com/storage/browser?project=YOUR-PROJECT-ID
    /// </remarks>
    /// <param name="bucket">The bucket to list the files from. Must not be null or empty.</param>
    /// <param name="folder">The parent folder. Only child folders in this parent folder will be returned.
    /// This parameter must end in '/' or be empty or null. If it's empty or null, the root of the bucket
    /// will be used.</param>
    /// <returns>A sequence of file objects within the parent folder of the bucket.</returns>
    public static IEnumerable<Google.Apis.Storage.v1.Data.Object> ListFiles(this StorageClient client, string bucket, string folder = "") {
      if (client == null) { throw new ArgumentNullException("this"); }
      if (string.IsNullOrWhiteSpace(bucket)) { throw new ArgumentOutOfRangeException("bucket must be non-empty"); }
      if (!string.IsNullOrEmpty(folder) && !folder.EndsWith(Delimiter.ToString())) { throw new ArgumentException("folder must end in " + Delimiter); }
      if (!string.IsNullOrEmpty(folder) && folder == Delimiter.ToString()) { throw new ArgumentException("root folder is \"\", not " + Delimiter); }

      var prefix = folder ?? "";
      return client
        .ListObjects(bucket, prefix, new ListObjectsOptions { Delimiter = Delimiter.ToString() })
        .Where(o => !o.Name.EndsWith(Delimiter.ToString()));
    }

    /// <summary>
    /// Returns the short name of the file or folder. 
    /// </summary>
    /// <remarks>
    /// This method applies the convensions of the Google Cloud Storage Browser at
    /// https://console.cloud.google.com/storage/browser?project=YOUR-PROJECT-ID
    /// For example, the short name for "foo/bar.txt" would be "bar.txt" and
    /// the short name for "foo/bar/" would be "bar".
    /// </remarks>
    /// <param name="name">The name of the folder or file. May not be null.</param>
    /// <returns>The short name of the file or folder.</returns>
    public static string ShortName(string name) {
      if (name == null) { throw new ArgumentNullException("name"); }
      return name.Split(new char[] { Delimiter }, StringSplitOptions.RemoveEmptyEntries).Last();
    }

    /// <summary>
    /// Returns the short name of the file or folder object.
    /// </summary>
    /// <remarks>
    /// This method applies the convensions of the Google Cloud Storage Browser at
    /// https://console.cloud.google.com/storage/browser?project=YOUR-PROJECT-ID
    /// For example, the short name for an object named "foo/bar.txt" would be "bar.txt" and
    /// the short name for an object named "foo/bar/" would be "bar".
    /// </remarks>
    /// <returns>The short name of the file or folder.</returns>
    public static string ShortName(this Google.Apis.Storage.v1.Data.Object obj) {
      if (obj == null) { throw new ArgumentNullException("this"); }
      return ShortName(obj.Name);
    }

    /// <summary>
    /// Creates an explicit folder.
    /// </summary>
    /// <param name="bucket">The bucket to create the folder in. Must not be null or empty.</param>
    /// <param name="folder">The path to the folder to create. This parameter must end in '/' and not
    /// be empty or null.</param>
    /// <returns>The newly created folder object.</returns>
    public static Google.Apis.Storage.v1.Data.Object CreateFolder(this StorageClient client, string bucket, string folder) {
      if (client == null) { throw new ArgumentNullException("this"); }
      if (string.IsNullOrWhiteSpace(bucket)) { throw new ArgumentOutOfRangeException("bucket must be non-empty"); }
      if (string.IsNullOrWhiteSpace(folder) || folder[0] == '/') { throw new ArgumentOutOfRangeException("folder must be non-empty"); }
      if (!string.IsNullOrEmpty(folder) && !folder.EndsWith(Delimiter.ToString())) { throw new ArgumentException("folder must end in " + Delimiter); }

      return client.UploadObject(bucket, folder, "", Stream.Null);
    }
  }

}
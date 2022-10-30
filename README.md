<div align="center">
  <h1>MediaFireDownloader</h1>
</div>

## What is MediaFireDownloader?
MediaFireDownloader is a simple console client for downloading a folder and all files from it from MediaFire file hosting without using premium account.

## How to use?

#### Command line arguments
```
MediaFireDownloader <folder key> [destination path]
```

Other optional parameters:

`--threadsCount=20` - number of threads to use for download
`--сookies` - cookie string with format `name=value; name=value`
`--dontUseSsl` - disables the use of https

<!--DontShowOnWebsite #begin-->
#### Example
```
                                  folder key
                                 ⌄⌄⌄⌄⌄⌄⌄⌄⌄⌄⌄⌄
https://www.mediafire.com/folder/rww7bhhi0yc1l/NewFolder
```
The folder with key `rww7bhhi0yc1l` and all files will be downloaded to the C:/Downloads
```
MediaFireDownloader rww7bhhi0yc1l C:/Downloads
```
or to the current folder if destination path is not specified
```
MediaFireDownloader rww7bhhi0yc1l
```

#### If 'The file is blocked or not available'

If the file is locked, inaccessible due to copyright, or for other reasons, you can
1) Copy the folder to your mediafire, after which the download from your copy of folder will become available
2) You also need to find cookies in the browser with the names ukey and user
3) Then run the program with the following parameters

```
MediaFireDownloader <key of your copied folder> --cookies="ukey=insert your ukey cookie; user=insert your user cookie"
```

<!--DontShowOnWebsite #end-->

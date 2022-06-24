<div align="center">
  <h1>MediaFireDownloader</h1>
</div>

## What is MediaFireDownloader?
MediaFireDownloader is a simple console client for downloading a folder and all files from it from MediaFire file hosting without using premium account.

## How to use?

#### Command line arguments
```
MediaFireDownloader <folder key> [destination path] [threads count]
```

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
<!--DontShowOnWebsite #end-->

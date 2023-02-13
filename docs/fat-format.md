# FAT File Format

This file documents the `FAT` file format found on PS1 C&C games. The format will be referred to as a `File Table` from here onwards.

## Purpose

The `File Table` lists all files that the game needs to render graphics, text, audio and video. 

It is designed to be placed on a CD-ROM with two data files that store the data of these listed files.

For example `DATA.FAT` will be placed beside:

- `DATA.MIX`: general files + short audio/video clips
- `DATA.XA`: long play audio and video

## Structure

At the start of a `File Table` is an **8 byte** wide header:

- `MIX File Count` (*4 bytes*): total number of files stored in the associated `.MIX` file - 32-bit unsigned integer
- `XA File Count` (*4 bytes*): same as above, but for the `.XA` file

After the header a **28 byte** wide entry for each file is stored.

A file entry contains:

- `File Name` (*12 bytes*): ASCII string (null terminated) 
- `Padding` (*4 bytes*): Filled with 0 values
- `Offset` (*4 bytes*): Offset in CD-ROM sectors where the file starts inside the data file. `MIX` file sector size is `2048` bytes, `XA` file sector size is `2336` - 32-bit unsigned integer
- `File Size` (*4 bytes*): size of the file in bytes- 32-bit unsigned integer
- `XA File Marker` (*2 bytes*): if set to `0` the file is stored in the `MIX` file, otherwise stored in the `XA` file - 16-bit unsigned integer
- `???` (*2 bytes*): ?- 16-bit unsigned integer?
# FileTransferToolTask

## Overview

FileTransferToolTask is a simple C# console application that copies files in chunks with integrity verification. It uses chunk-level hashing (MD5) and a final file hash comparison (SHA256 or configurable) to ensure the file copy is reliable and accurate.

## Features

- Copy files chunk by chunk to handle large files efficiently.
- Each chunk is hashed at the source and verified at the destination.
- Automatic retries on chunk verification failure (up to 3 times).
- Final file hash verification with configurable hash algorithms (e.g., SHA256, MD5).
- Multi-threaded copying support for improved performance.
- Console output for progress and verification results.

## Usage

```bash
FileTransferToolTask.exe <sourcePath> <destinationDirectory>

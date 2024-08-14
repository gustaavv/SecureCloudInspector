# :material-home: Home

## SecureCloudInspector

Encrypting your data before uploading to cloud drives should be a common practice. However, managing this process is often tedious. Therefore, SecureCloudInspector (SCI) comes to help you.

Unlike other encryption tools, SCI focuses on both data security and data recoverability when it comes to cloud storage.

## Introduction

SCI is a command line tool that encrypt your data before uploading to cloud.

> planning to develop a GUI.

SCI encrypts your data **at folder level**. Let's call the folder you want to encrypt as **source folder**. Then, choose an **encrypted folder**, where the encrypted data is located. Now, users can set the encrypted folder for sync by cloud drive clients. The encrypting process is fully managed by SCI.

```mermaid
flowchart LR
    subgraph local filesystem
        A[source folder] -- SCI --> B[encrypted folder]
    end
    B <-- sync --> C[cloud drive]
```

The decryption process is alike. After you download the encrypted folder, SCI decrypts your data.



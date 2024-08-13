# Encryption Mechanism

## Existing tools 

First, let's talk about existing encryption tools/practices first.

1. Veracrypt: strong (even absolute) data security, encrypting a filesystem into a single file. In terms of cloud storage, a large file needed to be uploaded every time we modify something in the encrypted filesystem, which is bad.
2. Cryptomator: strong data security, encryption is on file level (of a folder we want to encrypt), which is good for cloud storage scenario. But, the key file is stored in the encrypted folder. Nobody can guarantee that cloud drive provider keep the key file safe --- it can be broken. In such case, the whole encrypted data is broken.
3. Using archives: pack the data we want to encrypt into an archive with a password, and then upload it to cloud drive. It is a common practice, but involves manual work. Only this archive is broken if error in cloud drive happens, while other archives remain correct.

## The data need to be encrypted

Second, let's talk about the data we want to encrypt:

- How often do we use them? Just for archiving or using it regularly?
- How secure should they be?
    - absolute secure against security agencies --- you are doing something evil. Do you really think a program is enough to "protect" you?
    - secure against cloud drive providers --- Justice is on our side. Those cloud drive providers will definitely use our data for advertisements and training AI without our grant.
    - secure against other users of the computer --- a common usecase.
    - Whatever --- nothing hurts if leaked. Why are you reading this document now?

## About SCI

Now, let's talk about SCI.

SCI's encryption is on file level, using an archive to encrypt each file. The folder structure is the same, while names of folders and files are Hexadecimal string (results from hash functions, e.g. sha256, sha1). An example:

Source folder:

```
.
└── film
    ├── documentary
    │   ├── film1.avi
    │   └── film2.avi
    └── action
        └── film3.avi
```

Encrypted folder:

```
.
└── d0607f7a
    ├── 3708de48
    │   ├── 79bb81ea.rar
    │   └── 7c62a2d7.rar
    └── bd938c68
        └── a2bea7e8.rar
```

> You may notice (really?🤨) the encrypted names are the first 4 bytes of the sha256 result of the filename. Don't worry.
> This is just an example. The real encrypted names are different.

In terms of data security, **the purpose of SCI is to be secure against cloud drive providers (nobody else)**. There is a password for each source folder, inputted by the user, say `pwd`. The real password for every archive is a function `pwd_func(md5(pwd), sha1(pwd), sha256(pwd), filename)` (filename is also hashed first). So, `pwd` can be short and easy to remember, while the archive's password is hard to brute-force attack.

In terms of data recoverability, SCI takes advantage of rar files' recovery record. Therefore, every archive itself is recoverable. In addition, users can manually extract the archives even if SCI no longer exists.

Lastly, the metadata of a source folder(`pwd`,folder structure, name of each file etc.) is stored **in plaintext**. This follows the purpose of SCI as long as the cloud drive providers do not get these data.

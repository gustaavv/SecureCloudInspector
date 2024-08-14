# :simple-letsencrypt: Encryption Mechanism

## Existing tools 

First, let's talk about existing encryption tools/practices.

### Veracrypt

It guarantees strong (even absolute) data security, encrypting a filesystem into a single file. In terms of cloud storage, a large file needed to be uploaded every time we modify something in the encrypted filesystem, which is bad.

### Cryptomator

It has strong data security and encryption is on file level (of a folder we want to encrypt), which is good for cloud storage scenario. But, the key file is stored in the encrypted folder. Nobody can guarantee that cloud drive provider keep the key file safe --- it can be broken. In such case, the whole encrypted data is broken.

###  Using archives

Pack the data we want to encrypt into an archive with a password, and then upload it to cloud drive. It is a common practice, but involves manual work. Only this archive is broken if error in cloud drive happens, while other archives remain correct.

## Our data

Second, let's talk about the data we want to encrypt:

- How often do we use them? Just for archiving or using it regularly?
- How secure should they be?
    - Absolute secure against security agencies --- You are doing something evil. Do you really think a program is enough to "protect" you?
    - Secure against cloud drive providers --- Justice is on our side. Those cloud drive providers will definitely use our data for advertisements and training AI without our grant.
    - Secure against other users of the computer --- A common use case. Linux already does this for us, while it is hard to do privilege management on Windows.
    - Whatever --- nothing hurts if leaked. Why are you reading this document now? 🧐

## SCI

Now, let's talk about SCI.

SCI's encryption is on file level (like Cryptomator), using an archive to encrypt each file. The folder structure is the same, while names of folders and files are Hexadecimal string (results from hash functions).

???+ note "Hash Functions"
  
    (Cryptographic) Hash Functions are functions that take inputs of random length and outputs fixed-length bit strings. Common Hash Functions are MD5, SHA1, SHA256. 

    One nice thing about Hash Functions is that given the output, **it is almost impossible to find the original input**. Therefore, if the filenames contains text that will be banned when being censored, hashing it is a good practice. Even if the cloud drive providers knows that a filename is the output of a Hash Function, they can do nothing about it. 


Here is an example:

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

???+ note

    You may notice (really?🤨) the encrypted names are the first 4 bytes of the sha256 result of the filename. Don't worry. This is just an example. The real encrypted names are different.

In terms of data security, **the purpose of SCI is to be secure against cloud drive providers (nobody else)**. There is a password for each source folder, inputted by the user, say `pwd`. The real password for every archive is a function `pwd_func(md5(pwd), sha1(pwd), sha256(pwd), filename)` (filename is also hashed first). So, `pwd` can be short and easy to remember, while the archive's password is hard to brute-force attack.

In terms of data recoverability, SCI takes advantage of rar files' recovery record. Therefore, every archive itself is recoverable. In addition, users can manually extract the archives even if SCI no longer exists.

Lastly, the [databases](../gettingStarted/concepts.md#Database) are stored **in plaintext**. This follows the purpose of SCI as long as the cloud drive providers do not get these files.

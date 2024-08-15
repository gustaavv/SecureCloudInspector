# :octicons-terminal-24: SCI-CLI

## Use cases

Dictionary-like documents won't make you know how to use SCI-CLI. So, it is better to list common use cases that contain those commands. Feel free to check the details of those commands in the [Commands overview](#commands-overview) section below.

### 1. Routinely backup data

TBD

### 2. Download data to another device

TBD

## Commands overview

SCI-CLI works in **interactive mode**. So, all the commands are listed below, nothing else.

```mermaid
flowchart LR
%% name convention for id:
%% 'v' means verb
%% 'o' means options
%% 'e' means explanation
%% o21 means the 1st option of the 2nd verb
%% e21 means the explanation of o21
    exe[SCICLI.exe]
%% config
    exe --> v1[config]
    v1 --> o11("--init") -.- e11[[run this command first after downloading SCI-CLI]]
%% database 
    exe --> v2[db]
    v2 --> o21("--create") -.- e21[[create a database]]
    v2 --> o22("--rename") -.- e22[[rename a database]]
    v2 --> o23("--delete") -.- e23[[delete a database]]
    v2 --> o24("--list") -.- e24[[list all the databases]]
    v2 --> o25("--search") -.- e25[[search files in a database, useful when <br/> users want to download a specific file <br/> from cloud drives]]
%% encrypt
    exe --> v3[enc] -.- e3[[update db and encrypt data incrementally]]
%% decrypt
    exe --> v4[dec] -.- e4[[decrypt data downloaded from cloud drives to a folder]]
%% util
    exe --> v5[util]
    v5 --> o51("--cmp_dir") -.- e51[[compare two directories, useful when <br/> comparing source folder and decrypted folder]]
    v5 --> o52("--cal_pwd") -.- e52[[calculate the password of an encrypted archive <br/> if users want to manually extract the archive]]
```

E.g. If you want to list all the databases:

```
.\SCICLI.exe db --list
```















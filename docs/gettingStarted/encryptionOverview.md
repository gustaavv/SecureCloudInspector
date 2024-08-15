# :material-telescope: Encryption Overview

1. SCI encrypts data on a file basis (not a whole folder) using rar archives. Each file is packed into a rar archive with strong password protection.
2. SCI obscures both filenames and folder names with random Hexadecimal string.
3. SCI keeps the original folder structure.

Here is an example:

=== "Source folder"

    ```
    .
    └── film
        ├── documentary
        │   ├── film1.avi
        │   └── film2.avi
        └── action
            └── film3.avi
    ```

=== "Encrypted folder"

    ```
    .
    └── d0607f7a
        ├── 3708de48
        │   ├── 79bb81ea.rar
        │   └── 7c62a2d7.rar
        └── bd938c68
            └── a2bea7e8.rar
    ```
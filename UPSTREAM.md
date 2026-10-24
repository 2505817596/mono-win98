# Pinned upstream sources

The patch series is verified against exactly these revisions. Mono's `main`
branch has been frozen since 2024-08-27, so the commit below is also the
current tip at the time of writing.

| Component | Repository | Revision |
|---|---|---|
| Mono 6.13.0 | https://github.com/mono/mono | `0f53e9e151d92944cacab3e24ac359410c606df6` (2024-08-27) |
| corefx (submodule) | https://github.com/mono/corefx | `c4eeab9fc2faa0195a812e552cd73ee298d39386` |
| bockbuild (submodule) | https://github.com/mono/bockbuild | `1c3bc7a1d43557e47fbc91f38da250ab7506815e` |

Source archives:

```
https://codeload.github.com/mono/mono/tar.gz/0f53e9e151d92944cacab3e24ac359410c606df6
https://codeload.github.com/mono/corefx/tar.gz/c4eeab9fc2faa0195a812e552cd73ee298d39386
```

**Important:** GitHub source tarballs do not contain submodules. The corefx
tree is required for the `System.IO.FileSystem` patches and for building the
BCL; either download it separately (as above) into `mono-src/external/corefx`,
or clone with submodules:

```
git clone --depth 1 --recurse-submodules --shallow-submodules \
    https://github.com/mono/mono.git mono-src
cd mono-src
git checkout 0f53e9e151d92944cacab3e24ac359410c606df6
git submodule update --init --recursive
```

The series was checked by applying it to this tree and comparing the result
with the reference working tree: all 25 patches apply cleanly and reproduce
the changes byte-for-byte (line endings normalised).  A few mono files are
stored with CRLF endings (e.g. `TextBoxTextRenderer.cs`); the patches are
LF-normalised, so `win98/verify-patches.sh` converts the files touched by
the series to LF before applying.  This only changes line endings.

CI runs the same check on every push (see `.github/workflows/build.yml`).

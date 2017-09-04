﻿// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System.IO;

namespace SixLabors.ImageSharp.IO
{
 #if !NETSTANDARD1_1
    /// <summary>
    /// A simple interface representing the filesystem.
    /// </summary>
    internal interface IFileSystem
    {
        /// <summary>
        /// Returns a readable stream as defined by the path.
        /// </summary>
        /// <param name="path">Path to the file to open.</param>
        /// <returns>A stream representing the file to open.</returns>
        Stream OpenRead(string path);

        /// <summary>
        /// Creates or opens a file and returns it as a writeable stream as defined by the path.
        /// </summary>
        /// <param name="path">Path to the file to open.</param>
        /// <returns>A stream representing the file to open.</returns>
        Stream Create(string path);
    }
#endif
}

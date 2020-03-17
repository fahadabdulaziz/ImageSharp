// Copyright (c) Six Labors and contributors.
// Licensed under the Apache License, Version 2.0.

using System;
using System.IO;

using Moq;

using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.IO;
using SixLabors.ImageSharp.PixelFormats;

namespace SixLabors.ImageSharp.Tests
{
    public partial class ImageTests
    {
        public abstract class ImageLoadTestBase : IDisposable
        {
            protected Image<Rgba32> localStreamReturnImageRgba32;

            protected Image<Bgra4444> localStreamReturnImageAgnostic;

            protected Mock<IImageDecoder> localDecoder;

            protected IImageFormatDetector localMimeTypeDetector;

            protected Mock<IImageFormat> localImageFormatMock;

            protected Mock<IImageInfo> localImageInfoMock;

            protected readonly string MockFilePath = Guid.NewGuid().ToString();

            internal readonly Mock<IFileSystem> LocalFileSystemMock = new Mock<IFileSystem>();

            protected readonly TestFileSystem topLevelFileSystem = new TestFileSystem();

            public Configuration LocalConfiguration { get; }

            public TestFormat TestFormat { get; } = new TestFormat();

            /// <summary>
            /// Gets the top-level configuration in the context of this test case.
            /// It has <see cref="TestFormat"/> registered.
            /// </summary>
            public Configuration TopLevelConfiguration { get; }

            public byte[] Marker { get; }

            public MemoryStream DataStream { get; }

            public byte[] DecodedData { get; private set; }

            protected ImageLoadTestBase()
            {
                this.localStreamReturnImageRgba32 = new Image<Rgba32>(1, 1);
                this.localStreamReturnImageAgnostic = new Image<Bgra4444>(1, 1);

                this.localImageInfoMock = new Mock<IImageInfo>();
                this.localImageFormatMock = new Mock<IImageFormat>();

                var detector = new Mock<IImageInfoDetector>();
                detector.Setup(x => x.Identify(It.IsAny<Configuration>(), It.IsAny<Stream>())).Returns(this.localImageInfoMock.Object);
                this.localDecoder = detector.As<IImageDecoder>();
                this.localMimeTypeDetector = new MockImageFormatDetector(this.localImageFormatMock.Object);

                this.localDecoder.Setup(x => x.Decode<Rgba32>(It.IsAny<Configuration>(), It.IsAny<Stream>()))
                    .Callback<Configuration, Stream>((c, s) =>
                        {
                            using (var ms = new MemoryStream())
                            {
                                s.CopyTo(ms);
                                this.DecodedData = ms.ToArray();
                            }
                        })
                    .Returns(this.localStreamReturnImageRgba32);

                this.localDecoder.Setup(x => x.Decode(It.IsAny<Configuration>(), It.IsAny<Stream>()))
                    .Callback<Configuration, Stream>((c, s) =>
                        {
                            using (var ms = new MemoryStream())
                            {
                                s.CopyTo(ms);
                                this.DecodedData = ms.ToArray();
                            }
                        })
                    .Returns(this.localStreamReturnImageAgnostic);

                this.LocalConfiguration = new Configuration();
                this.LocalConfiguration.ImageFormatsManager.AddImageFormatDetector(this.localMimeTypeDetector);
                this.LocalConfiguration.ImageFormatsManager.SetDecoder(this.localImageFormatMock.Object, this.localDecoder.Object);

                this.TopLevelConfiguration = new Configuration(this.TestFormat);

                this.Marker = Guid.NewGuid().ToByteArray();
                this.DataStream = this.TestFormat.CreateStream(this.Marker);

                this.LocalFileSystemMock.Setup(x => x.OpenRead(this.MockFilePath)).Returns(this.DataStream);
                this.topLevelFileSystem.AddFile(this.MockFilePath, this.DataStream);
                this.LocalConfiguration.FileSystem = this.LocalFileSystemMock.Object;
                this.TopLevelConfiguration.FileSystem = this.topLevelFileSystem;
            }

            public void Dispose()
            {
                // Clean up the global object;
                this.localStreamReturnImageRgba32?.Dispose();
                this.localStreamReturnImageAgnostic?.Dispose();
            }
        }
    }
}

// Copyright (c) Six Labors and contributors.
// Licensed under the GNU Affero General Public License, Version 3.

using Xunit;

namespace SixLabors.ImageSharp.Tests.Processing.Effects
{
    using SixLabors.ImageSharp.Processing;
    using SixLabors.ImageSharp.Processing.Processors.Filters;

    public class ContrastTest : BaseImageOperationsExtensionTest
    {
        [Fact]
        public void Contrast_amount_ContrastProcessorDefaultsSet()
        {
            this.operations.Contrast(1.5F);
            ContrastProcessor processor = this.Verify<ContrastProcessor>();

            Assert.Equal(1.5F, processor.Amount);
        }

        [Fact]
        public void Contrast_amount_rect_ContrastProcessorDefaultsSet()
        {
            this.operations.Contrast(1.5F, this.rect);
            ContrastProcessor processor = this.Verify<ContrastProcessor>(this.rect);

            Assert.Equal(1.5F, processor.Amount);
        }
    }
}

// Copyright (c) Six Labors and contributors.
// Licensed under the GNU Affero General Public License, Version 3.

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Tests.Processing;
using SixLabors.ImageSharp.Tests.TestUtilities;
using Xunit;

namespace SixLabors.ImageSharp.Tests
{
    public class GraphicOptionsDefaultsExtensionsTests
    {
        [Fact]
        public void SetDefaultOptionsOnProcessingContext()
        {
            var option = new GraphicsOptions();
            var config = new Configuration();
            var context = new FakeImageOperationsProvider.FakeImageOperations<Rgba32>(config, null, true);

            context.SetGraphicsOptions(option);

            // sets the prop on the processing context not on the configuration
            Assert.Equal(option, context.Properties[typeof(GraphicsOptions)]);
            Assert.DoesNotContain(typeof(GraphicsOptions), config.Properties.Keys);
        }

        [Fact]
        public void UpdateDefaultOptionsOnProcessingContext_AlwaysNewInstance()
        {
            var option = new GraphicsOptions()
            {
                BlendPercentage = 0.9f
            };
            var config = new Configuration();
            var context = new FakeImageOperationsProvider.FakeImageOperations<Rgba32>(config, null, true);
            context.SetGraphicsOptions(option);

            context.SetGraphicsOptions(o =>
            {
                Assert.Equal(0.9f, o.BlendPercentage); // has origional values
                o.BlendPercentage = 0.4f;
            });

            var returnedOption = context.GetGraphicsOptions();
            Assert.Equal(0.4f, returnedOption.BlendPercentage);
            Assert.Equal(0.9f, option.BlendPercentage); // hasn't been mutated
        }

        [Fact]
        public void SetDefaultOptionsOnConfiguration()
        {
            var option = new GraphicsOptions();
            var config = new Configuration();

            config.SetGraphicsOptions(option);

            Assert.Equal(option, config.Properties[typeof(GraphicsOptions)]);
        }

        [Fact]
        public void UpdateDefaultOptionsOnConfiguration_AlwaysNewInstance()
        {
            var option = new GraphicsOptions()
            {
                BlendPercentage = 0.9f
            };
            var config = new Configuration();
            config.SetGraphicsOptions(option);

            config.SetGraphicsOptions(o =>
            {
                Assert.Equal(0.9f, o.BlendPercentage); // has origional values
                o.BlendPercentage = 0.4f;
            });

            var returnedOption = config.GetGraphicsOptions();
            Assert.Equal(0.4f, returnedOption.BlendPercentage);
            Assert.Equal(0.9f, option.BlendPercentage); // hasn't been mutated
        }

        [Fact]
        public void GetDefaultOptionsFromConfiguration_SettingNullThenReturnsNewInstance()
        {
            var config = new Configuration();

            var options = config.GetGraphicsOptions();
            Assert.NotNull(options);
            config.SetGraphicsOptions((GraphicsOptions)null);

            var options2 = config.GetGraphicsOptions();
            Assert.NotNull(options2);

            // we set it to null should now be a new instance
            Assert.NotEqual(options, options2);
        }

        [Fact]
        public void GetDefaultOptionsFromConfiguration_IgnoreIncorectlyTypesDictionEntry()
        {
            var config = new Configuration();

            config.Properties[typeof(GraphicsOptions)] = "wronge type";
            var options = config.GetGraphicsOptions();
            Assert.NotNull(options);
            Assert.IsType<GraphicsOptions>(options);
        }

        [Fact]
        public void GetDefaultOptionsFromConfiguration_AlwaysReturnsInstance()
        {
            var config = new Configuration();

            Assert.DoesNotContain(typeof(GraphicsOptions), config.Properties.Keys);
            var options = config.GetGraphicsOptions();
            Assert.NotNull(options);
        }

        [Fact]
        public void GetDefaultOptionsFromConfiguration_AlwaysReturnsSameValue()
        {
            var config = new Configuration();

            var options = config.GetGraphicsOptions();
            var options2 = config.GetGraphicsOptions();
            Assert.Equal(options, options2);
        }

        [Fact]
        public void GetDefaultOptionsFromProcessingContext_AlwaysReturnsInstance()
        {
            var config = new Configuration();
            var context = new FakeImageOperationsProvider.FakeImageOperations<Rgba32>(config, null, true);

            var ctxOptions = context.GetGraphicsOptions();
            Assert.NotNull(ctxOptions);
        }

        [Fact]
        public void GetDefaultOptionsFromProcessingContext_AlwaysReturnsInstanceEvenIfSetToNull()
        {
            var config = new Configuration();
            var context = new FakeImageOperationsProvider.FakeImageOperations<Rgba32>(config, null, true);

            context.SetGraphicsOptions((GraphicsOptions)null);
            var ctxOptions = context.GetGraphicsOptions();
            Assert.NotNull(ctxOptions);
        }

        [Fact]
        public void GetDefaultOptionsFromProcessingContext_FallbackToConfigsInstance()
        {
            var option = new GraphicsOptions();
            var config = new Configuration();
            config.SetGraphicsOptions(option);
            var context = new FakeImageOperationsProvider.FakeImageOperations<Rgba32>(config, null, true);

            var ctxOptions = context.GetGraphicsOptions();
            Assert.Equal(option, ctxOptions);
        }

        [Fact]
        public void GetDefaultOptionsFromProcessingContext_IgnoreIncorectlyTypesDictionEntry()
        {
            var config = new Configuration();
            var context = new FakeImageOperationsProvider.FakeImageOperations<Rgba32>(config, null, true);
            context.Properties[typeof(GraphicsOptions)] = "wronge type";
            var options = context.GetGraphicsOptions();
            Assert.NotNull(options);
            Assert.IsType<GraphicsOptions>(options);
        }
    }
}

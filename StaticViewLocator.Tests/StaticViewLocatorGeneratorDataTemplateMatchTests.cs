using System;
using System.Threading.Tasks;
using StaticViewLocator.Tests.TestHelpers;
using Xunit;

namespace StaticViewLocator.Tests;

public class StaticViewLocatorGeneratorDataTemplateMatchTests
{
    [Fact]
    public async Task DefaultGeneratedMatchRequiresAResolvedViewFactory()
    {
        const string source = """
using Avalonia.Controls;
using StaticViewLocator;

namespace TestApp.ViewModels
{
    public abstract class WidgetViewModelBase { }
    public sealed class WidgetViewModel<T> : WidgetViewModelBase { }
}


namespace TestApp.Views
{
    public sealed class WidgetView : UserControl { }
}

namespace TestApp
{
    [StaticViewMapping(typeof(ViewModels.WidgetViewModelBase), typeof(Views.WidgetView))]
    [StaticViewLocator(GenerateIDataTemplate = true)]
    public partial class ViewLocator { }
}
""";

        var generated = await StaticViewLocatorGeneratorVerifier.GetGeneratedSourcesAsync(source);
        var locatorSource = generated["ViewLocator_StaticViewLocator.cs"];

        Assert.Contains(
            "[typeof(TestApp.ViewModels.WidgetViewModel<>)] = () => new TestApp.Views.WidgetView()",
            locatorSource,
            StringComparison.Ordinal);
        Assert.Contains(
            "instance is TestApp.ViewModels.WidgetViewModelBase",
            locatorSource,
            StringComparison.Ordinal);
        Assert.Contains(
            "return TryGetResolvedViewFactory(data, out _);",
            locatorSource,
            StringComparison.Ordinal);
        Assert.DoesNotContain("s_missingViews.ContainsKey", locatorSource, StringComparison.Ordinal);
        Assert.DoesNotContain("GetGenericTypeDefinition()", locatorSource, StringComparison.Ordinal);
    }
}

---
order: 4.1
title: 本地化
---

本文档站点的本地化基于 AntDesign.Extensions.Localization 类库实现，主要提供可交互本地化服务，能够集成官方和第三方的本地化提供者实现在运行时无刷新切换语言。另外还实现了简单的嵌入 JSON 提供者。

## 安装

```shell
dotnet add package AntDesign.Extensions.Localization
```

### 使用可交互本地化组件

- 在 `Program.cs` 文件中添加以下代码：

    ```csharp
    builder.Services.AddInteractiveStringLocalizer();
    services.AddLocalization(options =>
    {
        options.ResourcesPath = "Resources";
    });

    ```

- 在项目的 `Resources` 目录下创建多语言文件，格式为 `{ResourceName}.{language}.resx`，例如 `Index.en-US.resx` 和 `Index.zh-CN.resx`。
  
  Index.en-US.resx:
  
  | 键 | 值 |
  | ---- | ---- |
  | Hello | Hello! |
  | Goodbye | Goodbye! |

  Index.zh-CN.resx:
  
  | 键 | 值 |
  | ---- | ---- |
  | Hello | Hello! |
  | Goodbye | Goodbye! |
  
- 使用时在 razor 注入 IStringLocalizer<T> 服务，例如：

    ```razor
    @inject IStringLocalizer<Index> localizer

    <p>@localizer["Hello"]</p>
    <p>@localizer["Goodbye"]</p>
    ```

- 需要使用第三方 Localization 提供者，或者其他配置，可参考 [官方文档](https://learn.microsoft.com/zh-cn/aspnet/core/blazor/globalization-localization?view=aspnetcore-8.0&WT.mc_id=DT-MVP-5003987)。

### 使用简单嵌入 JSON 提供者

虽然我们推荐官方的本地化方案，但是也可以用本组件文档站点的方案，利用简单嵌入 JSON 提供者实现多语言。需要注意的是，它仅支持直接注入 IStringLocalizer，不支持其泛型，本地化文件也仅支持单一文件。

- 在 `Program.cs` 文件中添加以下代码，此方法内部已调用 `AddInteractiveStringLocalizer`。

    ```csharp
    builder.Services.AddSimpleEmbeddedJsonLocalization(options =>
    {
        options.ResourcesPath = "Resources";
    });
    ```

- 在项目的 `Resources` 目录下创建多语言文件，格式为 `{language}.json`，例如 `en-US.json` 和 `zh-CN.json`。

    ```json
    // en-US.json
    {
        "Hello": "Hello!",
        "Goodbye": "Goodbye!"
    }

    // zh-CN.json
    {
        "Hello": "你好!",
        "Goodbye": "再见!"
    }
    ```

- 把JSON文件的生成操作设置为嵌入的资源
  
  ```xml
    <ItemGroup>
        <EmbeddedResource Include="Resources\*.json" />
    </ItemGroup>
  ```
  
- 使用时在 razor 注入 IStringLocalizer 服务（没有泛型），例如：

    ```razor
    @inject IStringLocalizer localizer

    <p>@localizer["Hello"]</p>
    <p>@localizer["Goodbye"]</p>
    ```


### 可交互地切换 UI 上的语言

```razor
@implements IDisposable
@inject ILocalizationService LocalizationService

<Button OnClick="()=>LocalizationService.SetLanguage("en-US")" >English</Button>
<Button OnClick="()=>LocalizationService.SetLanguage("zh-CN")" >中文</Button>

@code {

    protected override void OnInitialized()
    {
        LocalizationService.LanguageChanged += OnLanguageChanged;
    }
    private void OnLanguageChanged(object sender, CultureInfo args)
    {
        InvokeAsync(StateHasChanged);
    }
    public void Dispose()
    {
        LocalizationService.LanguageChanged -= OnLanguageChanged;
    }
}
```

### 路由上的语言标识

从本站点建立之初，就实现了路由的语言标记，有如下特点：

1. 进入页面时可根据浏览器环境，自动在路由上添加 `{locale}` 参数，例如 `/en-US/Index`。
2. 可以根据路由上已有标识切换语言，因此要切换语言时只需跳转到对应语言的路径即可。

以下是实现方式：

- 首先，在 `Routes.razor` 文件实现 `Router` 组件的 `OnNavigateAsync` 方法，调用 `LocalizationService.SetLanguage` 方法切换语言。

```razor
@using System.Reflection
@using System.Globalization

<ConfigProvider>
    <Router AppAssembly="typeof(App).Assembly" OnNavigateAsync="OnNavigateAsync">
        <Found Context="routeData">
            <RouteView RouteData="routeData" DefaultLayout="typeof(MainLayout)" />
            <FocusOnNavigate RouteData="routeData" Selector="h1" />
        </Found>
        <NotFound>
            <Result Status="404" />
        </NotFound>
    </Router>
    <AntContainer />
</ConfigProvider>

@inject ILocalizationService LocalizationService;
@inject NavigationManager NavigationManager;
@code{
    async Task OnNavigateAsync(NavigationContext navigationContext)
    {
        var relativeUri = navigationContext.Path;
        var currentCulture = LocalizationService.CurrentCulture;

        var segment = relativeUri.IndexOf('/') > 0 ? relativeUri.Substring(0, relativeUri.IndexOf('/')) : relativeUri;

        if (string.IsNullOrWhiteSpace(segment))
        {
            NavigationManager.NavigateTo($"{currentCulture.Name}/{relativeUri}");
            return;
        }
        else
        {
            if (segment.IsIn("zh-CN", "en-US"))
            {
                LocalizationService.SetLanguage(CultureInfo.GetCultureInfo(segment));
            }
            else if (currentCulture.Name.IsIn("zh-CN", "en-US"))
            {
                NavigationManager.NavigateTo($"{currentCulture.Name}/{relativeUri}");
            }
            else
            {
                NavigationManager.NavigateTo($"en-US/{relativeUri}");
                return;
            }
        }
    }
}
```

- 最后，给页面组件统一添加 `{locale}` 参数，使切换后的路由匹配对应的页面。

```razor
@page "/{locale}/Index"

@inject IStringLocalizer<Index> localizer

<p>@localizer["Hello"]</p>

@code {

    [Parameter]
    public string Locale { get; set; }
}

```
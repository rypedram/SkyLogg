using Microsoft.AspNetCore.Components.WebAssembly.Services;

namespace SkyLogg.Client.Core.Components.Pages.Dashboard;

public partial class DashboardPage
{
    [AutoInject] LazyAssemblyLoader lazyAssemblyLoader = default!;

    private bool isLoadingAssemblies = true;

    protected override async Task OnInitAsync()
    {
        await base.OnInitAsync();

        try
        {
            if (AppPlatform.IsBrowser)
            {
                await lazyAssemblyLoader.LoadAssembliesAsync([
                    "Newtonsoft.Json.wasm",
                    "System.Private.Xml.wasm"]
                    );
            }
        }
        finally
        {
            isLoadingAssemblies = false;
        }
    }

}

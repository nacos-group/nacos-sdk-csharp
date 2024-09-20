# nacos-sdk-csharp.Extensions.ServiceDiscovery

nacos-sdk-csharp.Extensions.ServiceDiscovery provides service registration and discovery With Microsoft.Extensions.ServiceDiscovery.

```csharp
builder.Services.AddServiceDiscovery(o =>
{
    o.RefreshPeriod = TimeSpan.FromSeconds(60);
})
.AddConfigurationServiceEndpointProvider()
.AddNacosSrvServiceEndpointProvider();
```

## Links

* [Documentation](https://nacos-sdk-csharp.readthedocs.io/en/latest/)
* [nacos-sdk-csharp GitHub](https://github.com/nacos-group/nacos-sdk-csharp)
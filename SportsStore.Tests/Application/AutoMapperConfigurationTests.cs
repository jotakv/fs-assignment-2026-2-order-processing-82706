using AutoMapper;
using SportsStore.Application.Mapping;

namespace SportsStore.Tests.Application;

public class AutoMapperConfigurationTests
{
    [Fact]
    public void Application_Mapping_Profile_Is_Valid()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<StoreMappingProfile>());
        config.AssertConfigurationIsValid();
    }
}

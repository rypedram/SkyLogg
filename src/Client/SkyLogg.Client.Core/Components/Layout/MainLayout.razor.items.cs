namespace SkyLogg.Client.Core.Components.Layout;

public partial class MainLayout
{
    private List<BitNavItem> navPanelItems = [];

    [AutoInject] protected IStringLocalizer<AppStrings> localizer = default!;
    [AutoInject] protected IAuthorizationService authorizationService = default!;

    private async Task SetNavPanelItems(ClaimsPrincipal authUser)
    {
        navPanelItems =
        [
            new()
            {
                Text = localizer[nameof(AppStrings.Home)],
                IconName = BitIconName.Home,
                Url = PageUrls.Home,
            }
        ];

        var (manageAirports, manageAircraft, manageCrew, manageCities, manageCountries, manageTimeZones) = await (
            authorizationService.IsAuthorized(authUser!, AppFeatures.BaseInfo.ManageAirports),
            authorizationService.IsAuthorized(authUser!, AppFeatures.BaseInfo.ManageAircraft),
            authorizationService.IsAuthorized(authUser!, AppFeatures.BaseInfo.ManageCrew),
            authorizationService.IsAuthorized(authUser!, AppFeatures.BaseInfo.ManageCities),
            authorizationService.IsAuthorized(authUser!, AppFeatures.BaseInfo.ManageCountries),
            authorizationService.IsAuthorized(authUser!, AppFeatures.BaseInfo.ManageTimeZones));

        if (await authorizationService.IsAuthorized(authUser!, AppFeatures.Logbook.ManageFlightLogs))
        {
            navPanelItems.Add(new()
            {
                Text = localizer[nameof(AppStrings.Logbook)],
                IconName = BitIconName.AirplaneSolid,
                ChildItems =
                [
                    new()
                    {
                        Text = localizer[nameof(AppStrings.LogbookDashboard)],
                        IconName = BitIconName.ViewDashboard,
                        Url = PageUrls.Logbook,
                    },
                    new()
                    {
                        Text = localizer[nameof(AppStrings.FlightLogs)],
                        IconName = BitIconName.List,
                        Url = PageUrls.FlightLogs,
                        AdditionalUrls =
                        [
                            PageUrls.AddOrEditFlightLog,
                            $"{PageUrls.FlightLogDetail}/",
                        ],
                    },
                    new()
                    {
                        Text = localizer[nameof(AppStrings.FlightSummary)],
                        IconName = BitIconName.BarChartVerticalFill,
                        Url = PageUrls.FlightSummary,
                    },
                    new()
                    {
                        Text = localizer[nameof(AppStrings.FlightMap)],
                        IconName = BitIconName.MapPin,
                        Url = PageUrls.FlightMap,
                    },
                    new()
                    {
                        Text = localizer[nameof(AppStrings.Reports)],
                        IconName = BitIconName.ReportDocument,
                        Url = PageUrls.FlightReports,
                    },
                    new()
                    {
                        Text = localizer[nameof(AppStrings.FlightImport)],
                        IconName = BitIconName.Upload,
                        Url = PageUrls.FlightImport,
                    },
                ],
            });
        }

        if (manageAirports || manageAircraft || manageCrew || manageCities || manageCountries || manageTimeZones)
        {
            BitNavItem baseInfoItem = new()
            {
                Text = localizer[nameof(AppStrings.BaseInfo)],
                IconName = BitIconName.Dictionary,
                ChildItems = []
            };

            navPanelItems.Add(baseInfoItem);

            if (manageAirports)
            {
                baseInfoItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.Airports)],
                    IconName = BitIconName.Globe,
                    Url = PageUrls.BaseInfoAirports,
                });
            }

            if (manageCountries)
            {
                baseInfoItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.Countries)],
                    IconName = BitIconName.MapLayers,
                    Url = PageUrls.BaseInfoCountries,
                });
            }

            if (manageTimeZones)
            {
                baseInfoItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.TimeZones)],
                    IconName = BitIconName.WorldClock,
                    Url = PageUrls.BaseInfoTimeZones,
                });
            }

            if (manageCities)
            {
                baseInfoItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.Cities)],
                    IconName = BitIconName.CityNext,
                    Url = PageUrls.BaseInfoCities,
                });
            }

            if (manageAircraft)
            {
                baseInfoItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.Aircrafts)],
                    IconName = BitIconName.Airplane,
                    Url = PageUrls.BaseInfoAircraft,
                });
            }

            if (manageCrew)
            {
                baseInfoItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.CrewMembers)],
                    IconName = BitIconName.People,
                    Url = PageUrls.BaseInfoCrew,
                });
            }
        }

        if (await authorizationService.IsAuthorized(authUser!, AppFeatures.AdminPanel.Dashboard))
        {
            navPanelItems.Add(new()
            {
                Text = localizer[nameof(AppStrings.Dashboard)],
                IconName = BitIconName.BarChartVerticalFill,
                Url = PageUrls.Dashboard,
            });
        }

        navPanelItems.Add(new()
        {
            Text = localizer[nameof(AppStrings.Terms)],
            IconName = BitIconName.EntityExtraction,
            Url = PageUrls.Terms,
        });

        navPanelItems.Add(new()
        {
            Text = localizer[nameof(AppStrings.About)],
            IconName = BitIconName.Info,
            Url = PageUrls.About,
        });

        var (manageRoles, manageUsers, manageAiPrompt) = await (authorizationService.IsAuthorized(authUser!, AppFeatures.Management.ManageRoles),
            authorizationService.IsAuthorized(authUser!, AppFeatures.Management.ManageUsers),
            authorizationService.IsAuthorized(authUser!, AppFeatures.Management.ManageAiPrompt));

        if (manageRoles || manageUsers || manageAiPrompt)
        {
            BitNavItem managementItem = new()
            {
                Text = localizer[nameof(AppStrings.Management)],
                IconName = BitIconName.SettingsSecure,
                ChildItems = []
            };

            navPanelItems.Add(managementItem);

            if (manageRoles)
            {
                managementItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.UserGroups)],
                    IconName = BitIconName.WorkforceManagement,
                    Url = PageUrls.Roles,
                });
            }

            if (manageUsers)
            {
                managementItem.ChildItems.Add(new()
                {
                    Text = localizer[nameof(AppStrings.Users)],
                    IconName = BitIconName.SecurityGroup,
                    Url = PageUrls.Users,
                });
            }
        }

        if (authUser.IsAuthenticated())
        {
            navPanelItems.Add(new()
            {
                Text = localizer[nameof(AppStrings.Settings)],
                IconName = BitIconName.Equalizer,
                Url = PageUrls.Settings,
                AdditionalUrls =
                [
                    $"{PageUrls.Settings}/{PageUrls.SettingsSections.Profile}",
                    $"{PageUrls.Settings}/{PageUrls.SettingsSections.Account}",
                    $"{PageUrls.Settings}/{PageUrls.SettingsSections.Tfa}",
                    $"{PageUrls.Settings}/{PageUrls.SettingsSections.Sessions}",
                    $"{PageUrls.Settings}/{PageUrls.SettingsSections.UpgradeAccount}",
                ]
            });
        }
    }
}

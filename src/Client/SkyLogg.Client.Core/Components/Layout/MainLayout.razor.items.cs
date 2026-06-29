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

        if (await authorizationService.IsAuthorized(authUser!, AppFeatures.Logbook.ManageFlightLogs))
        {
            navPanelItems.AddRange(
            [
                new()
                {
                    Text = localizer[nameof(AppStrings.LogbookDashboard)],
                    IconName = BitIconName.Airplane,
                    Url = PageUrls.Logbook,
                },
                new()
                {
                    Text = localizer[nameof(AppStrings.FlightLogs)],
                    IconName = BitIconName.BookAnswers,
                    Url = PageUrls.FlightLogs,
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
                    Text = localizer[nameof(AppStrings.FlightImport)],
                    IconName = BitIconName.Upload,
                    Url = PageUrls.FlightImport,
                },
                new()
                {
                    Text = localizer[nameof(AppStrings.Reports)],
                    IconName = BitIconName.ReportDocument,
                    Url = PageUrls.FlightReports,
                },
            ]);
        }

        if (await authorizationService.IsAuthorized(authUser!, AppFeatures.Logbook.ManageCrew))
        {
            navPanelItems.Add(new()
            {
                Text = localizer[nameof(AppStrings.CrewMembers)],
                IconName = BitIconName.People,
                Url = PageUrls.CrewMembers,
            });
        }

        if (await authorizationService.IsAuthorized(authUser!, AppFeatures.Logbook.ManageFleet))
        {
            navPanelItems.AddRange(
            [
                new()
                {
                    Text = localizer[nameof(AppStrings.Aircrafts)],
                    IconName = BitIconName.Airplane,
                    Url = PageUrls.AircraftList,
                },
                new()
                {
                    Text = localizer[nameof(AppStrings.Airports)],
                    IconName = BitIconName.Globe,
                    Url = PageUrls.Airports,
                },
            ]);
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

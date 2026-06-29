namespace SkyLogg.Shared.Features.Logbook;

/// <summary>
/// Standard aviation crew position / qualification type.
/// </summary>
public enum CrewPositionType : byte
{
    CPT = 0,
    FO = 1,
    IP = 2,
    SIC = 3,
    FE = 4,
    PU = 5,
    FA = 6,
    OBS = 7,
    OTHER = 99,
}

#!/usr/bin/env python3
"""Generates geo seed data C# files for SkyLogg."""
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / "src/Server/SkyLogg.Server.Api/Features/Logbook/Seed"
ROOT.mkdir(parents=True, exist_ok=True)

countries = [
    ("AF", "AFG", "Afghanistan"), ("AL", "ALB", "Albania"), ("DZ", "DZA", "Algeria"), ("AS", "ASM", "American Samoa"),
    ("AD", "AND", "Andorra"), ("AO", "AGO", "Angola"), ("AI", "AIA", "Anguilla"), ("AQ", "ATA", "Antarctica"),
    ("AG", "ATG", "Antigua and Barbuda"), ("AR", "ARG", "Argentina"), ("AM", "ARM", "Armenia"), ("AW", "ABW", "Aruba"),
    ("AU", "AUS", "Australia"), ("AT", "AUT", "Austria"), ("AZ", "AZE", "Azerbaijan"), ("BS", "BHS", "Bahamas"),
    ("BH", "BHR", "Bahrain"), ("BD", "BGD", "Bangladesh"), ("BB", "BRB", "Barbados"), ("BY", "BLR", "Belarus"),
    ("BE", "BEL", "Belgium"), ("BZ", "BLZ", "Belize"), ("BJ", "BEN", "Benin"), ("BM", "BMU", "Bermuda"),
    ("BT", "BTN", "Bhutan"), ("BO", "BOL", "Bolivia"), ("BQ", "BES", "Bonaire, Sint Eustatius and Saba"),
    ("BA", "BIH", "Bosnia and Herzegovina"), ("BW", "BWA", "Botswana"), ("BV", "BVT", "Bouvet Island"),
    ("BR", "BRA", "Brazil"), ("IO", "IOT", "British Indian Ocean Territory"), ("BN", "BRN", "Brunei Darussalam"),
    ("BG", "BGR", "Bulgaria"), ("BF", "BFA", "Burkina Faso"), ("BI", "BDI", "Burundi"), ("CV", "CPV", "Cabo Verde"),
    ("KH", "KHM", "Cambodia"), ("CM", "CMR", "Cameroon"), ("CA", "CAN", "Canada"), ("KY", "CYM", "Cayman Islands"),
    ("CF", "CAF", "Central African Republic"), ("TD", "TCD", "Chad"), ("CL", "CHL", "Chile"), ("CN", "CHN", "China"),
    ("CX", "CXR", "Christmas Island"), ("CC", "CCK", "Cocos (Keeling) Islands"), ("CO", "COL", "Colombia"),
    ("KM", "COM", "Comoros"), ("CG", "COG", "Congo"), ("CD", "COD", "Congo, Democratic Republic of the"),
    ("CK", "COK", "Cook Islands"), ("CR", "CRI", "Costa Rica"), ("CI", "CIV", "Cote d'Ivoire"), ("HR", "HRV", "Croatia"),
    ("CU", "CUB", "Cuba"), ("CW", "CUW", "Curacao"), ("CY", "CYP", "Cyprus"), ("CZ", "CZE", "Czechia"),
    ("DK", "DNK", "Denmark"), ("DJ", "DJI", "Djibouti"), ("DM", "DMA", "Dominica"), ("DO", "DOM", "Dominican Republic"),
    ("EC", "ECU", "Ecuador"), ("EG", "EGY", "Egypt"), ("SV", "SLV", "El Salvador"), ("GQ", "GNQ", "Equatorial Guinea"),
    ("ER", "ERI", "Eritrea"), ("EE", "EST", "Estonia"), ("SZ", "SWZ", "Eswatini"), ("ET", "ETH", "Ethiopia"),
    ("FK", "FLK", "Falkland Islands"), ("FO", "FRO", "Faroe Islands"), ("FJ", "FJI", "Fiji"), ("FI", "FIN", "Finland"),
    ("FR", "FRA", "France"), ("GF", "GUF", "French Guiana"), ("PF", "PYF", "French Polynesia"),
    ("TF", "ATF", "French Southern Territories"), ("GA", "GAB", "Gabon"), ("GM", "GMB", "Gambia"), ("GE", "GEO", "Georgia"),
    ("DE", "DEU", "Germany"), ("GH", "GHA", "Ghana"), ("GI", "GIB", "Gibraltar"), ("GR", "GRC", "Greece"),
    ("GL", "GRL", "Greenland"), ("GD", "GRD", "Grenada"), ("GP", "GLP", "Guadeloupe"), ("GU", "GUM", "Guam"),
    ("GT", "GTM", "Guatemala"), ("GG", "GGY", "Guernsey"), ("GN", "GIN", "Guinea"), ("GW", "GNB", "Guinea-Bissau"),
    ("GY", "GUY", "Guyana"), ("HT", "HTI", "Haiti"), ("HM", "HMD", "Heard Island and McDonald Islands"),
    ("VA", "VAT", "Holy See"), ("HN", "HND", "Honduras"), ("HK", "HKG", "Hong Kong"), ("HU", "HUN", "Hungary"),
    ("IS", "ISL", "Iceland"), ("IN", "IND", "India"), ("ID", "IDN", "Indonesia"), ("IR", "IRN", "Iran"),
    ("IQ", "IRQ", "Iraq"), ("IE", "IRL", "Ireland"), ("IM", "IMN", "Isle of Man"), ("IL", "ISR", "Israel"),
    ("IT", "ITA", "Italy"), ("JM", "JAM", "Jamaica"), ("JP", "JPN", "Japan"), ("JE", "JEY", "Jersey"),
    ("JO", "JOR", "Jordan"), ("KZ", "KAZ", "Kazakhstan"), ("KE", "KEN", "Kenya"), ("KI", "KIR", "Kiribati"),
    ("KP", "PRK", "Korea, Democratic People's Republic of"), ("KR", "KOR", "Korea, Republic of"), ("KW", "KWT", "Kuwait"),
    ("KG", "KGZ", "Kyrgyzstan"), ("LA", "LAO", "Lao People's Democratic Republic"), ("LV", "LVA", "Latvia"),
    ("LB", "LBN", "Lebanon"), ("LS", "LSO", "Lesotho"), ("LR", "LBR", "Liberia"), ("LY", "LBY", "Libya"),
    ("LI", "LIE", "Liechtenstein"), ("LT", "LTU", "Lithuania"), ("LU", "LUX", "Luxembourg"), ("MO", "MAC", "Macao"),
    ("MG", "MDG", "Madagascar"), ("MW", "MWI", "Malawi"), ("MY", "MYS", "Malaysia"), ("MV", "MDV", "Maldives"),
    ("ML", "MLI", "Mali"), ("MT", "MLT", "Malta"), ("MH", "MHL", "Marshall Islands"), ("MQ", "MTQ", "Martinique"),
    ("MR", "MRT", "Mauritania"), ("MU", "MUS", "Mauritius"), ("YT", "MYT", "Mayotte"), ("MX", "MEX", "Mexico"),
    ("FM", "FSM", "Micronesia"), ("MD", "MDA", "Moldova"), ("MC", "MCO", "Monaco"), ("MN", "MNG", "Mongolia"),
    ("ME", "MNE", "Montenegro"), ("MS", "MSR", "Montserrat"), ("MA", "MAR", "Morocco"), ("MZ", "MOZ", "Mozambique"),
    ("MM", "MMR", "Myanmar"), ("NA", "NAM", "Namibia"), ("NR", "NRU", "Nauru"), ("NP", "NPL", "Nepal"),
    ("NL", "NLD", "Netherlands"), ("NC", "NCL", "New Caledonia"), ("NZ", "NZL", "New Zealand"), ("NI", "NIC", "Nicaragua"),
    ("NE", "NER", "Niger"), ("NG", "NGA", "Nigeria"), ("NU", "NIU", "Niue"), ("NF", "NFK", "Norfolk Island"),
    ("MK", "MKD", "North Macedonia"), ("MP", "MNP", "Northern Mariana Islands"), ("NO", "NOR", "Norway"), ("OM", "OMN", "Oman"),
    ("PK", "PAK", "Pakistan"), ("PW", "PLW", "Palau"), ("PS", "PSE", "Palestine, State of"), ("PA", "PAN", "Panama"),
    ("PG", "PNG", "Papua New Guinea"), ("PY", "PRY", "Paraguay"), ("PE", "PER", "Peru"), ("PH", "PHL", "Philippines"),
    ("PN", "PCN", "Pitcairn"), ("PL", "POL", "Poland"), ("PT", "PRT", "Portugal"), ("PR", "PRI", "Puerto Rico"),
    ("QA", "QAT", "Qatar"), ("RE", "REU", "Reunion"), ("RO", "ROU", "Romania"), ("RU", "RUS", "Russian Federation"),
    ("RW", "RWA", "Rwanda"), ("BL", "BLM", "Saint Barthelemy"), ("SH", "SHN", "Saint Helena, Ascension and Tristan da Cunha"),
    ("KN", "KNA", "Saint Kitts and Nevis"), ("LC", "LCA", "Saint Lucia"), ("MF", "MAF", "Saint Martin (French part)"),
    ("PM", "SPM", "Saint Pierre and Miquelon"), ("VC", "VCT", "Saint Vincent and the Grenadines"), ("WS", "WSM", "Samoa"),
    ("SM", "SMR", "San Marino"), ("ST", "STP", "Sao Tome and Principe"), ("SA", "SAU", "Saudi Arabia"),
    ("SN", "SEN", "Senegal"), ("RS", "SRB", "Serbia"), ("SC", "SYC", "Seychelles"), ("SL", "SLE", "Sierra Leone"),
    ("SG", "SGP", "Singapore"), ("SX", "SXM", "Sint Maarten (Dutch part)"), ("SK", "SVK", "Slovakia"),
    ("SI", "SVN", "Slovenia"), ("SB", "SLB", "Solomon Islands"), ("SO", "SOM", "Somalia"), ("ZA", "ZAF", "South Africa"),
    ("GS", "SGS", "South Georgia and the South Sandwich Islands"), ("SS", "SSD", "South Sudan"), ("ES", "ESP", "Spain"),
    ("LK", "LKA", "Sri Lanka"), ("SD", "SDN", "Sudan"), ("SR", "SUR", "Suriname"), ("SJ", "SJM", "Svalbard and Jan Mayen"),
    ("SE", "SWE", "Sweden"), ("CH", "CHE", "Switzerland"), ("SY", "SYR", "Syrian Arab Republic"), ("TW", "TWN", "Taiwan"),
    ("TJ", "TJK", "Tajikistan"), ("TZ", "TZA", "Tanzania"), ("TH", "THA", "Thailand"), ("TL", "TLS", "Timor-Leste"),
    ("TG", "TGO", "Togo"), ("TK", "TKL", "Tokelau"), ("TO", "TON", "Tonga"), ("TT", "TTO", "Trinidad and Tobago"),
    ("TN", "TUN", "Tunisia"), ("TR", "TUR", "Turkiye"), ("TM", "TKM", "Turkmenistan"), ("TC", "TCA", "Turks and Caicos Islands"),
    ("TV", "TUV", "Tuvalu"), ("UG", "UGA", "Uganda"), ("UA", "UKR", "Ukraine"), ("AE", "ARE", "United Arab Emirates"),
    ("GB", "GBR", "United Kingdom"), ("US", "USA", "United States"), ("UM", "UMI", "United States Minor Outlying Islands"),
    ("UY", "URY", "Uruguay"), ("UZ", "UZB", "Uzbekistan"), ("VU", "VUT", "Vanuatu"), ("VE", "VEN", "Venezuela"),
    ("VN", "VNM", "Viet Nam"), ("VG", "VGB", "Virgin Islands (British)"), ("VI", "VIR", "Virgin Islands (U.S.)"),
    ("WF", "WLF", "Wallis and Futuna"), ("EH", "ESH", "Western Sahara"), ("YE", "YEM", "Yemen"), ("ZM", "ZMB", "Zambia"),
    ("ZW", "ZWE", "Zimbabwe"),
]

legacy = {
    "US": "c1000001-0000-4000-8000-000000000001",
    "GB": "c1000001-0000-4000-8000-000000000002",
    "FR": "c1000001-0000-4000-8000-000000000003",
    "DE": "c1000001-0000-4000-8000-000000000004",
    "IR": "c1000001-0000-4000-8000-000000000005",
    "AE": "c1000001-0000-4000-8000-000000000006",
    "ES": "c1000001-0000-4000-8000-000000000007",
}

lines = [
    "namespace SkyLogg.Server.Api.Features.Logbook.Seed;",
    "",
    "internal static partial class CountrySeedData",
    "{",
    "    internal static Country[] GetCountries() =>",
    "    [",
]
idx = 8
for iso2, iso3, name in countries:
    gid = legacy.get(iso2, f"c1000001-0000-4000-8000-{idx:012d}")
    if iso2 not in legacy:
        idx += 1
    name_esc = name.replace('"', '\\"')
    lines.append(f'        new() {{ Id = Guid.Parse("{gid}"), Iso2 = "{iso2}", Iso3 = "{iso3}", Name = "{name_esc}" }},')
lines += ["    ];", "}"]
(ROOT / "CountrySeedData.cs").write_text("\n".join(lines), encoding="utf-8")
print(f"Wrote {len(countries)} countries")

# IANA canonical time zones (comprehensive set)
timezones = [
    ("Africa/Abidjan", "Abidjan", "+00:00"), ("Africa/Accra", "Accra", "+00:00"), ("Africa/Addis_Ababa", "Addis Ababa", "+03:00"),
    ("Africa/Algiers", "Algiers", "+01:00"), ("Africa/Cairo", "Cairo", "+02:00"), ("Africa/Casablanca", "Casablanca", "+01:00"),
    ("Africa/Johannesburg", "Johannesburg", "+02:00"), ("Africa/Lagos", "Lagos", "+01:00"), ("Africa/Nairobi", "Nairobi", "+03:00"),
    ("America/Anchorage", "Anchorage", "-09:00"), ("America/Argentina/Buenos_Aires", "Buenos Aires", "-03:00"),
    ("America/Bogota", "Bogota", "-05:00"), ("America/Caracas", "Caracas", "-04:00"), ("America/Chicago", "Chicago", "-06:00"),
    ("America/Denver", "Denver", "-07:00"), ("America/Halifax", "Halifax", "-04:00"), ("America/Los_Angeles", "Los Angeles", "-08:00"),
    ("America/Mexico_City", "Mexico City", "-06:00"), ("America/New_York", "New York", "-05:00"), ("America/Phoenix", "Phoenix", "-07:00"),
    ("America/Sao_Paulo", "Sao Paulo", "-03:00"), ("America/Toronto", "Toronto", "-05:00"), ("America/Vancouver", "Vancouver", "-08:00"),
    ("Asia/Baghdad", "Baghdad", "+03:00"), ("Asia/Baku", "Baku", "+04:00"), ("Asia/Bangkok", "Bangkok", "+07:00"),
    ("Asia/Beirut", "Beirut", "+02:00"), ("Asia/Colombo", "Colombo", "+05:30"), ("Asia/Damascus", "Damascus", "+03:00"),
    ("Asia/Dhaka", "Dhaka", "+06:00"), ("Asia/Dubai", "Dubai", "+04:00"), ("Asia/Hong_Kong", "Hong Kong", "+08:00"),
    ("Asia/Jakarta", "Jakarta", "+07:00"), ("Asia/Jerusalem", "Jerusalem", "+02:00"), ("Asia/Karachi", "Karachi", "+05:00"),
    ("Asia/Kolkata", "Kolkata", "+05:30"), ("Asia/Kuala_Lumpur", "Kuala Lumpur", "+08:00"), ("Asia/Kuwait", "Kuwait", "+03:00"),
    ("Asia/Manila", "Manila", "+08:00"), ("Asia/Muscat", "Muscat", "+04:00"), ("Asia/Qatar", "Qatar", "+03:00"),
    ("Asia/Riyadh", "Riyadh", "+03:00"), ("Asia/Seoul", "Seoul", "+09:00"), ("Asia/Shanghai", "Shanghai", "+08:00"),
    ("Asia/Singapore", "Singapore", "+08:00"), ("Asia/Taipei", "Taipei", "+08:00"), ("Asia/Tehran", "Tehran", "+03:30"),
    ("Asia/Tokyo", "Tokyo", "+09:00"), ("Asia/Yerevan", "Yerevan", "+04:00"), ("Atlantic/Reykjavik", "Reykjavik", "+00:00"),
    ("Australia/Adelaide", "Adelaide", "+09:30"), ("Australia/Brisbane", "Brisbane", "+10:00"), ("Australia/Darwin", "Darwin", "+09:30"),
    ("Australia/Melbourne", "Melbourne", "+10:00"), ("Australia/Perth", "Perth", "+08:00"), ("Australia/Sydney", "Sydney", "+10:00"),
    ("Europe/Amsterdam", "Amsterdam", "+01:00"), ("Europe/Athens", "Athens", "+02:00"), ("Europe/Belgrade", "Belgrade", "+01:00"),
    ("Europe/Berlin", "Berlin", "+01:00"), ("Europe/Brussels", "Brussels", "+01:00"), ("Europe/Bucharest", "Bucharest", "+02:00"),
    ("Europe/Budapest", "Budapest", "+01:00"), ("Europe/Copenhagen", "Copenhagen", "+01:00"), ("Europe/Dublin", "Dublin", "+00:00"),
    ("Europe/Helsinki", "Helsinki", "+02:00"), ("Europe/Istanbul", "Istanbul", "+03:00"), ("Europe/Lisbon", "Lisbon", "+00:00"),
    ("Europe/London", "London", "+00:00"), ("Europe/Madrid", "Madrid", "+01:00"), ("Europe/Moscow", "Moscow", "+03:00"),
    ("Europe/Oslo", "Oslo", "+01:00"), ("Europe/Paris", "Paris", "+01:00"), ("Europe/Prague", "Prague", "+01:00"),
    ("Europe/Rome", "Rome", "+01:00"), ("Europe/Stockholm", "Stockholm", "+01:00"), ("Europe/Vienna", "Vienna", "+01:00"),
    ("Europe/Warsaw", "Warsaw", "+01:00"), ("Europe/Zurich", "Zurich", "+01:00"), ("Pacific/Auckland", "Auckland", "+12:00"),
    ("Pacific/Fiji", "Fiji", "+12:00"), ("Pacific/Honolulu", "Honolulu", "-10:00"), ("UTC", "Coordinated Universal Time", "+00:00"),
]

tz_lines = [
    "namespace SkyLogg.Server.Api.Features.Logbook.Seed;",
    "",
    "internal static partial class TimeZoneSeedData",
    "{",
    "    internal static GeoTimeZone[] GetTimeZones() =>",
    "    [",
]
for i, (iana, display, offset) in enumerate(timezones, 1):
    gid = f"a1000001-0000-4000-8000-{i:012d}"
    tz_lines.append(f'        new() {{ Id = Guid.Parse("{gid}"), IanaId = "{iana}", DisplayName = "{display}", UtcOffset = "{offset}" }},')
tz_lines += ["    ];", "}"]
(ROOT / "TimeZoneSeedData.cs").write_text("\n".join(tz_lines), encoding="utf-8")
print(f"Wrote {len(timezones)} timezones")

# Map country iso2 -> id
country_ids = {}
idx = 8
for iso2, iso3, name in countries:
    gid = legacy.get(iso2, f"c1000001-0000-4000-8000-{idx:012d}")
    if iso2 not in legacy:
        idx += 1
    country_ids[iso2] = gid

# Map iana -> tz id
tz_ids = {iana: f"a1000001-0000-4000-8000-{i:012d}" for i, (iana, _, _) in enumerate(timezones, 1)}

cities = [
    ("New York", "US", "America/New_York", 1),
    ("Los Angeles", "US", "America/Los_Angeles", 2),
    ("Chicago", "US", "America/Chicago", 3),
    ("Atlanta", "US", "America/New_York", 4),
    ("Denver", "US", "America/Denver", 5),
    ("London", "GB", "Europe/London", 6),
    ("Paris", "FR", "Europe/Paris", 7),
    ("Frankfurt", "DE", "Europe/Berlin", 8),
    ("Tehran", "IR", "Asia/Tehran", 9),
    ("Dubai", "AE", "Asia/Dubai", 10),
    ("Madrid", "ES", "Europe/Madrid", 11),
    ("Tokyo", "JP", "Asia/Tokyo", 12),
    ("Singapore", "SG", "Asia/Singapore", 13),
    ("Hong Kong", "HK", "Asia/Hong_Kong", 14),
    ("Sydney", "AU", "Australia/Sydney", 15),
    ("Toronto", "CA", "America/Toronto", 16),
    ("Istanbul", "TR", "Europe/Istanbul", 17),
    ("Moscow", "RU", "Europe/Moscow", 18),
    ("Beijing", "CN", "Asia/Shanghai", 19),
    ("Mumbai", "IN", "Asia/Kolkata", 20),
    ("Bangkok", "TH", "Asia/Bangkok", 21),
    ("Johannesburg", "ZA", "Africa/Johannesburg", 22),
    ("Sao Paulo", "BR", "America/Sao_Paulo", 23),
    ("Mexico City", "MX", "America/Mexico_City", 24),
    ("Amsterdam", "NL", "Europe/Amsterdam", 25),
    ("Rome", "IT", "Europe/Rome", 26),
    ("Zurich", "CH", "Europe/Zurich", 27),
    ("Vienna", "AT", "Europe/Vienna", 28),
    ("Brussels", "BE", "Europe/Brussels", 29),
    ("Copenhagen", "DK", "Europe/Copenhagen", 30),
    ("Stockholm", "SE", "Europe/Stockholm", 31),
    ("Oslo", "NO", "Europe/Oslo", 32),
    ("Helsinki", "FI", "Europe/Helsinki", 33),
    ("Athens", "GR", "Europe/Athens", 34),
    ("Lisbon", "PT", "Europe/Lisbon", 35),
    ("Dublin", "IE", "Europe/Dublin", 36),
    ("Warsaw", "PL", "Europe/Warsaw", 37),
    ("Prague", "CZ", "Europe/Prague", 38),
    ("Budapest", "HU", "Europe/Budapest", 39),
    ("Bucharest", "RO", "Europe/Bucharest", 40),
    ("Cairo", "EG", "Africa/Cairo", 41),
    ("Riyadh", "SA", "Asia/Riyadh", 42),
    ("Doha", "QA", "Asia/Qatar", 43),
    ("Kuwait City", "KW", "Asia/Kuwait", 44),
    ("Baghdad", "IQ", "Asia/Baghdad", 45),
    ("Karachi", "PK", "Asia/Karachi", 46),
    ("Seoul", "KR", "Asia/Seoul", 47),
    ("Taipei", "TW", "Asia/Taipei", 48),
    ("Manila", "PH", "Asia/Manila", 49),
    ("Jakarta", "ID", "Asia/Jakarta", 50),
    ("Kuala Lumpur", "MY", "Asia/Kuala_Lumpur", 51),
    ("Auckland", "NZ", "Pacific/Auckland", 52),
    ("Vancouver", "CA", "America/Vancouver", 53),
    ("Miami", "US", "America/New_York", 54),
    ("San Francisco", "US", "America/Los_Angeles", 55),
    ("Seattle", "US", "America/Los_Angeles", 56),
    ("Boston", "US", "America/New_York", 57),
    ("Houston", "US", "America/Chicago", 58),
    ("Dallas", "US", "America/Chicago", 59),
    ("Las Vegas", "US", "America/Los_Angeles", 60),
]

city_lines = [
    "namespace SkyLogg.Server.Api.Features.Logbook.Seed;",
    "",
    "internal static partial class CitySeedData",
    "{",
    "    internal static City[] GetCities() =>",
    "    [",
]
for name, iso2, iana, num in cities:
    cid = f"d1000001-0000-4000-8000-{num:012d}"
    city_lines.append(
        f'        new() {{ Id = Guid.Parse("{cid}"), Name = "{name}", CountryId = Guid.Parse("{country_ids[iso2]}"), TimeZoneId = Guid.Parse("{tz_ids[iana]}"), IsActive = true }},'
    )
city_lines += ["    ];", "}"]
(ROOT / "CitySeedData.cs").write_text("\n".join(city_lines), encoding="utf-8")
print(f"Wrote {len(cities)} cities")

airports = [
    ("KJFK", "JFK", "John F Kennedy Intl", 1, "US", 40.6413, -73.7781, 13, 1),
    ("KLAX", "LAX", "Los Angeles Intl", 2, "US", 33.9416, -118.4085, 125, 2),
    ("KORD", "ORD", "Chicago O'Hare Intl", 3, "US", 41.9742, -87.9073, 672, 3),
    ("KATL", "ATL", "Hartsfield-Jackson Atlanta Intl", 4, "US", 33.6407, -84.4277, 1026, 4),
    ("KDEN", "DEN", "Denver Intl", 5, "US", 39.8561, -104.6737, 5431, 5),
    ("EGLL", "LHR", "London Heathrow", 6, "GB", 51.4700, -0.4543, 83, 6),
    ("LFPG", "CDG", "Charles de Gaulle", 7, "FR", 49.0097, 2.5479, 392, 7),
    ("EDDF", "FRA", "Frankfurt Main", 8, "DE", 50.0379, 8.5622, 364, 8),
    ("OIII", "THR", "Mehrabad Intl", 9, "IR", 35.6892, 51.3134, 3962, 9),
    ("OIIE", "IKA", "Imam Khomeini Intl", 9, "IR", 35.4161, 51.1522, 3305, 9),
    ("OMDB", "DXB", "Dubai Intl", 10, "AE", 25.2532, 55.3657, 62, 10),
    ("LEMD", "MAD", "Adolfo Suarez Madrid-Barajas", 11, "ES", 40.4983, -3.5676, 1998, 11),
    ("RJTT", "HND", "Tokyo Haneda", 12, "JP", 35.5494, 139.7798, 35, 12),
    ("WSSS", "SIN", "Singapore Changi", 13, "SG", 1.3644, 103.9915, 22, 13),
    ("VHHH", "HKG", "Hong Kong Intl", 14, "HK", 22.3080, 113.9185, 28, 14),
    ("YSSY", "SYD", "Sydney Kingsford Smith", 15, "AU", -33.9399, 151.1753, 21, 15),
    ("CYYZ", "YYZ", "Toronto Pearson", 16, "CA", 43.6777, -79.6248, 569, 16),
    ("LTFM", "IST", "Istanbul Airport", 17, "TR", 41.2753, 28.7519, 325, 17),
    ("UUEE", "SVO", "Sheremetyevo Intl", 18, "RU", 55.9726, 37.4146, 622, 18),
    ("ZBAA", "PEK", "Beijing Capital Intl", 19, "CN", 40.0799, 116.6031, 116, 19),
    ("VABB", "BOM", "Chhatrapati Shivaji Intl", 20, "IN", 19.0896, 72.8656, 39, 20),
    ("VTBS", "BKK", "Suvarnabhumi Intl", 21, "TH", 13.6900, 100.7501, 5, 21),
    ("FAOR", "JNB", "O.R. Tambo Intl", 22, "ZA", -26.1392, 28.2460, 5558, 22),
    ("SBGR", "GRU", "Sao Paulo Guarulhos Intl", 23, "BR", -23.4356, -46.4731, 2461, 23),
    ("MMMX", "MEX", "Mexico City Intl", 24, "MX", 19.4363, -99.0721, 7316, 24),
    ("EHAM", "AMS", "Amsterdam Schiphol", 25, "NL", 52.3105, 4.7683, -11, 25),
    ("LIRF", "FCO", "Rome Fiumicino", 26, "IT", 41.8003, 12.2389, 15, 26),
    ("LSZH", "ZRH", "Zurich Airport", 27, "CH", 47.4647, 8.5492, 1416, 27),
    ("LOWW", "VIE", "Vienna Intl", 28, "AT", 48.1103, 16.5697, 600, 28),
    ("EBBR", "BRU", "Brussels Airport", 29, "BE", 50.9014, 4.4844, 184, 29),
    ("EKCH", "CPH", "Copenhagen Airport", 30, "DK", 55.6180, 12.6508, 17, 30),
    ("ESSA", "ARN", "Stockholm Arlanda", 31, "SE", 59.6519, 17.9186, 137, 31),
    ("ENGM", "OSL", "Oslo Gardermoen", 32, "NO", 60.1939, 11.1004, 681, 32),
    ("EFHK", "HEL", "Helsinki Vantaa", 33, "FI", 60.3172, 24.9633, 179, 33),
    ("LGAV", "ATH", "Athens Intl", 34, "GR", 37.9364, 23.9445, 308, 34),
    ("LPPT", "LIS", "Lisbon Portela", 35, "PT", 38.7813, -9.1359, 374, 35),
    ("EIDW", "DUB", "Dublin Airport", 36, "IE", 53.4264, -6.2499, 242, 36),
    ("EPWA", "WAW", "Warsaw Chopin", 37, "PL", 52.1657, 20.9671, 362, 37),
    ("LKPR", "PRG", "Vaclav Havel Prague", 38, "CZ", 50.1008, 14.2600, 1247, 38),
    ("LHBP", "BUD", "Budapest Ferenc Liszt", 39, "HU", 47.4298, 19.2611, 495, 39),
    ("LROP", "OTP", "Henri Coanda Bucharest", 40, "RO", 44.5711, 26.0850, 314, 40),
    ("HECA", "CAI", "Cairo Intl", 41, "EG", 30.1219, 31.4056, 382, 41),
    ("OERK", "RUH", "King Khalid Intl", 42, "SA", 24.9576, 46.6988, 2049, 42),
    ("OTBD", "DOH", "Hamad Intl", 43, "QA", 25.2731, 51.6081, 13, 43),
    ("OKBK", "KWI", "Kuwait Intl", 44, "KW", 29.2266, 47.9689, 206, 44),
    ("ORBI", "BGW", "Baghdad Intl", 45, "IQ", 33.2625, 44.2344, 114, 45),
    ("OPKC", "KHI", "Jinnah Intl", 46, "PK", 24.9065, 67.1608, 100, 46),
    ("RKSI", "ICN", "Incheon Intl", 47, "KR", 37.4602, 126.4407, 23, 47),
    ("RCTP", "TPE", "Taiwan Taoyuan Intl", 48, "TW", 25.0797, 121.2342, 106, 48),
    ("RPLL", "MNL", "Ninoy Aquino Intl", 49, "PH", 14.5086, 121.0198, 23, 49),
    ("WIII", "CGK", "Soekarno-Hatta Intl", 50, "ID", -6.1256, 106.6559, 32, 50),
    ("WMKK", "KUL", "Kuala Lumpur Intl", 51, "MY", 2.7456, 101.7099, 69, 51),
    ("NZAA", "AKL", "Auckland Airport", 52, "NZ", -37.0082, 174.7850, 23, 52),
    ("CYVR", "YVR", "Vancouver Intl", 53, "CA", 49.1967, -123.1815, 14, 53),
    ("KMIA", "MIA", "Miami Intl", 54, "US", 25.7959, -80.2870, 8, 54),
    ("KSFO", "SFO", "San Francisco Intl", 55, "US", 37.6213, -122.3790, 13, 55),
    ("KSEA", "SEA", "Seattle-Tacoma Intl", 56, "US", 47.4502, -122.3088, 433, 56),
    ("KBOS", "BOS", "Boston Logan Intl", 57, "US", 42.3656, -71.0096, 20, 57),
    ("KIAH", "IAH", "George Bush Intercontinental", 58, "US", 29.9902, -95.3368, 97, 58),
    ("KDFW", "DFW", "Dallas Fort Worth Intl", 59, "US", 32.8998, -97.0403, 607, 59),
    ("KLAS", "LAS", "Harry Reid Intl", 60, "US", 36.0840, -115.1537, 2181, 60),
]

country_names = {iso2: name for iso2, _, name in countries}

ap_lines = [
    "namespace SkyLogg.Server.Api.Features.Logbook.Seed;",
    "",
    "internal static partial class AirportSeedData",
    "{",
    "    internal static Airport[] GetAirports() =>",
    "    [",
]
for icao, iata, name, city_num, iso2, lat, lon, elev, aid in airports:
    name_esc = name.replace("'", "\\'")
    ap_lines.append(
        f'        new() {{ Id = Guid.Parse("b1000001-0000-4000-8000-{aid:012d}"), ICAO = "{icao}", IATA = "{iata}", Name = "{name_esc}", CityId = Guid.Parse("d1000001-0000-4000-8000-{city_num:012d}"), CountryId = Guid.Parse("{country_ids[iso2]}"), Country = "{country_names[iso2]}", Latitude = {lat}, Longitude = {lon}, ElevationFt = {elev}, IsActive = true }},'
    )
ap_lines += ["    ];", "}"]
(ROOT / "AirportSeedData.cs").write_text("\n".join(ap_lines), encoding="utf-8")
print(f"Wrote {len(airports)} airports")

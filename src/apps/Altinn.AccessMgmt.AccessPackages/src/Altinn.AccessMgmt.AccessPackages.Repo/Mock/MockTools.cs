using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Altinn.AccessMgmt.AccessPackages.Repo.Mock;

static class MockTools
{
    /// <summary>
    /// Random
    /// </summary>
    public static readonly Random Rnd = new();

    private static readonly List<string> FirstNames = new()
    {
        "Marius",
        "Ivar",
        "Trond",
        "Geir",
        "Herman",
        "Oda",
        "Pernille",
        "Aleksander",
        "Alexandra",
        "Andreas",
        "Anders",
        "Anne",
        "Anna",
        "Benedicte",
        "Bjørn",
        "Dagfinn",
        "Dhana",
        "Ernst",
        "Johnny",
        "Mona",
        "Hanne",
        "Håvard",
        "Henning",
        "Jon",
        "Kjetil",
        "Martin",
        "Mette",
        "Naila",
        "Ragnhild",
        "Randi",
        "Remi",
        "Rune",
        "Simen",
        "Simon",
        "Siri",
        "Anna",
        "Sneha",
        "Sondre",
        "Sophie",
        "Thomas",
        "Lars",
        "Vegard",
        "Bård",
        "William",
        "Mattias",
        "Nora",
        "Nicoline",
        "Nicolai",
        "Tonje",
        "Bente",
        "Mikkel",
        "Nina",
    };

    private static readonly List<string> LastNames = new()
    {
        "Thuen",
        "Tryti",
        "Johnsen",
        "Isnes",
        "Nilsen",
        "Vedeler",
        "Risbakk",
        "Sørli",
        "Langfors",
        "Olsen",
        "Gopalswamy",
        "Lyngøy",
        "Heintz",
        "Kørra",
        "Lauritsen",
        "Saltnes",
        "Andersen",
        "Normann",
        "Øye",
        "Gunnerud",
        "Grytten",
        "Raza",
        "Tafjord",
        "Mansæterbak",
        "Løvoll",
        "Marhaug",
        "Larsen",
        "Rekkedal",
        "Ellefsen",
        "Kristiansen",
        "Zahl",
        "Sirure",
        "Wittek",
        "Arntsen",
        "Knoph",
        "Leirvik",
        "Nyeng",
        "Hansen",
        "Fransen",
        "Mortensen",
        "Bording",
        "Pedersen",
        "Larsen",
        "Herskedal",
    };
    
    public static (string FirstName, string LastName, DateTime BirthDate) GeneratePerson()
    {
        string firstName = FirstNames[Rnd.Next(FirstNames.Count)];
        string lastName = LastNames[Rnd.Next(LastNames.Count)];
        DateTime birthDate = GenerateRandomBirthDate(1960, 2010);

        return (firstName, lastName, birthDate);
    }

    private static DateTime GenerateRandomBirthDate(int startYear, int endYear)
    {
        int year = Rnd.Next(startYear, endYear + 1);
        int month = Rnd.Next(1, 13);
        int day = Rnd.Next(1, DateTime.DaysInMonth(year, month) + 1);

        return new DateTime(year, month, day);
    }

    public static readonly Dictionary<string, (List<string> Adjectives, List<string> Elements, List<string> Nouns)> IndustryTerms = new()
        {
            {
                "Bygg, anlegg og eiendom",
                (
                    new List<string> { "Solid", "Sterk", "Robust", "Moderne", "Stolt", "Trygg", "Høy", "Stabil", "Kraftig", "Effektiv", "Bærekraftig", "Rask", "Smart", "Fleksibel", "Tidløs" },
                    new List<string> { "Mur", "Betong", "Stål", "Tre", "Grunn", "Tårn", "Stein", "Fasade", "Søyle", "Tak", "Bygg", "Havn", "Port", "Plattform", "Bro" },
                    new List<string> { "Gruppen", "Partner", "Prosjekt", "Konstruksjon", "Utvikling", "Drift", "Eiendom", "Anlegg", "Selskap", "Design", "Entreprenør", "Teknikk", "Montasje", "Struktur", "Arkitektur" }
                )
            },
            {
                "Energi, vann, avløp og avfall",
                (
                    new List<string> { "Grønn", "Ren", "Bærekraftig", "Effektiv", "Kraftig", "Stabil", "Fornybar", "Trygg", "Smart", "Sterk", "Dynamisk", "Sirkulær", "Miljøvennlig", "Klar", "Rask" },
                    new List<string> { "Strøm", "Vann", "Vind", "Sol", "Bølge", "Energi", "Kraft", "Avfall", "Bio", "Resirk", "Netto", "Flom", "Driv", "Batteri", "Gass" },
                    new List<string> { "Tjenester", "Løsninger", "Drift", "Selskap", "Nettverk", "Teknologi", "Energi", "Partner", "Gruppen", "Utvikling", "System", "Innovasjon", "Forvaltning", "Distribusjon", "Anlegg" }
                )
            },
            {
                "Handel, overnatting og servering",
                (
                    new List<string> { "Moderne", "Lokal", "Smakfull", "Koselig", "Trivelig", "Urban", "Rik", "Fargerik", "Eksklusiv", "Hyggelig", "Tradisjonell", "Luksuriøs", "Bærekraftig", "Frisk", "Vennlig" },
                    new List<string> { "Marked", "Kjøkken", "Hotell", "Mat", "Servering", "Bistro", "Butikk", "Gård", "Hus", "Brygge", "Café", "Smie", "Vare", "Stasjon", "Torg" },
                    new List<string> { "Gruppen", "Drift", "Tjenester", "Opplevelse", "Partner", "Konsept", "Selskap", "Gjestfrihet", "Kompaniet", "Matglede", "Handel", "Overnatting", "Utvikling", "Servering", "Reiseliv" }
                )
            },
            {
                "Helse, pleie, omsorg og vern",
                (
                    new List<string> { "Trygg", "Omsorgsfull", "Sterk", "Helsebringende", "Frisk", "Balansert", "Hjertevarm", "Rask", "Bærekraftig", "Fleksibel", "Vennlig", "Rolig", "Støttende", "Sunn", "Betryggende" },
                    new List<string> { "Helse", "Pleie", "Omsorg", "Trygghet", "Klinikk", "Vern", "Behandling", "Terapi", "Rehab", "Støtte", "Liv", "Balanse", "Velferd", "Trivsel", "Fokus" },
                    new List<string> { "Tjenester", "Partner", "Senter", "Klinikk", "Omsorg", "Gruppen", "Selskap", "Forvaltning", "Utvikling", "Kompetanse", "Helsehus", "Netverk", "Stiftelse", "Institusjon", "Behandling" }
                )
            },
            {
                "Industrier",
                (
                    new List<string> { "Kraftig", "Solid", "Effektiv", "Industriell", "Bærekraftig", "Smart", "Robust", "Moderne", "Avansert", "Høyteknologisk", "Presis", "Sterk", "Dynamisk", "Innovativ", "Fleksibel" },
                    new List<string> { "Maskin", "Stål", "Verk", "Fabrikk", "Produksjon", "Kraft", "Teknologi", "Prosess", "Metall", "Robot", "Automat", "Verksted", "Logistikk", "Drift", "System" },
                    new List<string> { "Gruppen", "Partner", "Industriselskap", "Produksjon", "Teknikk", "Utvikling", "Drift", "Logistikk", "Montasje", "System", "Forvaltning", "Automatisering", "Teknologi", "Konstruksjon", "Prosjekt" }
                )
            },
            {
                "Jordbruk, skogbruk, jakt, fiske og akvakultur",
                (
                    new List<string> { "Grønn", "Naturlig", "Bærekraftig", "Frisk", "Lokal", "Ren", "Økologisk", "Vilt", "Sunn", "Fruktbar", "Tradisjonell", "Sterk", "Landlig", "Frodig", "Rik" },
                    new List<string> { "Jord", "Skog", "Fjord", "Elv", "Fangst", "Fisk", "Hav", "Åker", "Gård", "Kyst", "Natur", "Hage", "Vann", "Laks", "Brygge" },
                    new List<string> { "Partner", "Gruppen", "Landbruk", "Forvaltning", "Tjenester", "Produksjon", "Utvikling", "Nettverk", "Selskap", "Drift", "Havbruk", "Økologi", "Fiskeri", "Akvakultur", "Konsult" }
                )
            },
            {
                "Kultur og frivillighet",
                (
                    new List<string> { "Kreativ", "Fargerik", "Mangfoldig", "Inkluderende", "Dynamisk", "Engasjerende", "Åpen", "Samfunnsorientert", "Bærekraftig", "Tradisjonell", "Moderne", "Historisk", "Stolt", "Vennlig", "Innovativ" },
                    new List<string> { "Kultur", "Scene", "Festival", "Forum", "Galleri", "Klubb", "Samfunn", "Teater", "Musikk", "Møteplass", "Historie", "Tradisjon", "Netverk", "Verksted", "Fellesskap" },
                    new List<string> { "Forening", "Stiftelse", "Initiativ", "Organisasjon", "Prosjekt", "Gruppe", "Komité", "Selskap", "Utvikling", "Arena", "Tjenester", "Forum", "Drift", "Partner", "Fond" }
                )
            },
            {
                "Oppvekst og utdanning",
                (
                    new List<string> { "Lærende", "Kreativ", "Inkluderende", "Trygg", "Utviklende", "Nysgjerrig", "Støttende", "Dynamisk", "Inspirerende", "Fleksibel", "Åpen", "Engasjerende", "Moderne", "Fremtidsrettet", "Bærekraftig" },
                    new List<string> { "Skole", "Barnehage", "Læring", "Kunnskap", "Akademi", "Utvikling", "Fokus", "Pedagogikk", "Kompetanse", "Miljø", "Samfunn", "Institutt", "Kurs", "Verksted", "Veiledning" },
                    new List<string> { "Senter", "Partner", "Tjenester", "Utvikling", "Kompetanse", "Skolenettverk", "Akademi", "Forvaltning", "Læringshus", "Organisasjon", "Prosjekt", "Stiftelse", "Plattform", "Drift", "Samspill" }
                )
            },
            {
                "Transport og lagring",
                (
                    new List<string> { "Rask", "Effektiv", "Trygg", "Pålitelig", "Fleksibel", "Dynamisk", "Sterk", "Moderne", "Bærekraftig", "Kraftig", "Global", "Smart", "Robust", "Stabil", "Presis" },
                    new List<string> { "Frakt", "Logistikk", "Transport", "Havn", "Terminal", "Rute", "Bil", "Skip", "Fly", "Last", "Kjede", "Kran", "Bane", "Depot", "Lager" },
                    new List<string> { "Partner", "Gruppen", "Tjenester", "Logistikk", "Utvikling", "Forvaltning", "Transport", "Distribusjon", "Selskap", "Nettverk", "System", "Prosjekt", "Terminaldrift", "Fraktservice", "Depot" }
                )
            },
            {
                "Andre tjenesteytende næringer",
                (
                    new List<string> { "Fleksibel", "Effektiv", "Bærekraftig", "Moderne", "Dynamisk", "Smart", "Innovativ", "Trygg", "Rask", "Global", "Profesjonell", "Allsidig", "Pålitelig", "Sterk", "Stabil" },
                    new List<string> { "Tjeneste", "Løsning", "Netverk", "Rådgivning", "Konsult", "Forvaltning", "System", "Drift", "Partner", "Utvikling", "Plattform", "Service", "Strategi", "Innovasjon", "Kompetanse" },
                    new List<string> { "Selskap", "Gruppen", "Tjenester", "Partner", "Konsult", "Prosjekt", "Forvaltning", "Systemer", "Nettverk", "Drift", "Utvikling", "Organisasjon", "Plattform", "Løsning", "Fond" }
                )
            }
        };

    public static string GenerateOrganizationNumber()
    {
        int[] digits = new int[9];
        for (int i = 0; i < 8; i++)
        {
            digits[i] = Rnd.Next(0, 10);
        }

        digits[8] = CalculateMod11CheckDigit(digits);
        return string.Join("", digits);
    }

    private static int CalculateMod11CheckDigit(int[] digits)
    {
        int[] weights = { 3, 2, 7, 6, 5, 4, 3, 2 };
        int sum = 0;

        for (int i = 0; i < 8; i++)
        {
            sum += digits[i] * weights[i];
        }

        int remainder = sum % 11;
        if (remainder == 0)
            return 0;
        else if (11 - remainder == 10)
            return 0; // Erstatt med 0 hvis sjekksifferet blir 10, som ofte brukes i generiske eksempler
        else
            return 11 - remainder;
    }

    public static string GenerateCompanyName(string industry)
    {
        if (IndustryTerms.TryGetValue(industry, out var terms))
        {
            var adjective = terms.Adjectives[Rnd.Next(terms.Adjectives.Count)];
            var element = terms.Elements[Rnd.Next(terms.Elements.Count)];
            var noun = terms.Nouns[Rnd.Next(terms.Nouns.Count)];

            return $"{adjective} {element} {noun} AS";
        }
        return "Ugyldig bransje spesifisert";
    }

}

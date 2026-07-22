namespace IsleBridge.Sdk;

/// <summary>
/// Known EVRIMA species short names for <see cref="IBridgeClient.SwapAsync"/>. These are the
/// short names the plugin resolves via its <c>speciesClassTemplate</c> (contract §6); you can
/// still pass any raw string or a full class path (detected by a <c>/</c>). Using these
/// constants instead of hand-typed literals catches typos at compile time.
/// <para>
/// Rosters change between patches. <see cref="Playable"/> reflects the shipped 0.21.x roster;
/// <see cref="InDevelopment"/> is announced/testing content that is provisional and may change
/// or never ship. A non-playable or misspelled class instantly crashes the target's client, so
/// the plugin whitelists against <c>allowedClasses</c> — treat these as convenience, not a
/// guarantee of acceptance.
/// </para>
/// </summary>
public static class Species
{
    // --- Carnivores -------------------------------------------------------------------
    public const string Tyrannosaurus = "Tyrannosaurus";
    public const string Allosaurus = "Allosaurus";
    public const string Carnotaurus = "Carnotaurus";
    public const string Ceratosaurus = "Ceratosaurus";
    public const string Baryonyx = "Baryonyx";
    public const string Dilophosaurus = "Dilophosaurus";
    public const string Herrerasaurus = "Herrerasaurus";
    public const string Austroraptor = "Austroraptor";
    public const string Troodon = "Troodon";
    public const string Omniraptor = "Omniraptor";

    // --- Semi-aquatic -----------------------------------------------------------------
    public const string Deinosuchus = "Deinosuchus";

    // --- Flyer ------------------------------------------------------------------------
    public const string Pteranodon = "Pteranodon";

    // --- Herbivores -------------------------------------------------------------------
    public const string Triceratops = "Triceratops";
    public const string Stegosaurus = "Stegosaurus";
    public const string Diabloceratops = "Diabloceratops";
    public const string Kentrosaurus = "Kentrosaurus";
    public const string Tenontosaurus = "Tenontosaurus";
    public const string Maiasaura = "Maiasaura";
    public const string Pachycephalosaurus = "Pachycephalosaurus";
    public const string Dryosaurus = "Dryosaurus";
    public const string Hypsilophodon = "Hypsilophodon";

    // --- Omnivores --------------------------------------------------------------------
    public const string Beipiaosaurus = "Beipiaosaurus";
    public const string Gallimimus = "Gallimimus";

    // --- In development / announced (provisional; may change or never ship) -----------
    public const string Spinosaurus = "Spinosaurus";
    public const string Giganotosaurus = "Giganotosaurus";
    public const string Suchomimus = "Suchomimus";
    public const string Parasaurolophus = "Parasaurolophus";
    public const string Avaceratops = "Avaceratops";
    public const string Oviraptor = "Oviraptor";
    public const string Magyarosaurus = "Magyarosaurus";

    /// <summary>Species that ship in the current (0.21.x) Evrima roster.</summary>
    public static readonly IReadOnlyList<string> Playable =
    [
        Tyrannosaurus, Allosaurus, Carnotaurus, Ceratosaurus, Baryonyx, Dilophosaurus,
        Herrerasaurus, Austroraptor, Troodon, Omniraptor, Deinosuchus, Pteranodon,
        Triceratops, Stegosaurus, Diabloceratops, Kentrosaurus, Tenontosaurus, Maiasaura,
        Pachycephalosaurus, Dryosaurus, Hypsilophodon, Beipiaosaurus, Gallimimus
    ];

    /// <summary>Announced or in-testing species; provisional and subject to change.</summary>
    public static readonly IReadOnlyList<string> InDevelopment =
    [
        Spinosaurus, Giganotosaurus, Suchomimus, Parasaurolophus, Avaceratops, Oviraptor,
        Magyarosaurus
    ];
    
    public static readonly IReadOnlyList<string> All = InDevelopment.Concat(Playable).ToList();

    /// <summary>True if <paramref name="species"/> is a currently shipped short name (case-insensitive).</summary>
    public static bool IsPlayable(string species) =>
        Playable.Contains(species, StringComparer.OrdinalIgnoreCase);

    /// <summary>True if <paramref name="species"/> is any known short name — shipped or in development.</summary>
    public static bool IsKnown(string species) =>
        IsPlayable(species) || InDevelopment.Contains(species, StringComparer.OrdinalIgnoreCase);
    
    public static string FriendlyName(string species)
    {
        return All.FirstOrDefault(p =>
            species.Contains(p, StringComparison.InvariantCultureIgnoreCase)) ?? species;
    }
}

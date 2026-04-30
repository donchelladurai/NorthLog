namespace NorthLog.Wpf.Helpers;

/* To the interviewers:
     FormationHelper is everything wrong with putting domain knowledge in a static
     helper class:
    
       * Hardcoded list of formation tops that should obviously live in the
         database (or at the very least a config file).

        * Magic strings ("FORTIES", "BRENT") used as keys, with no enum, no const or
         case-sensitive matching that quietly breaks when data changes.

       * No way to add a new field without recompiling.
       
       * Returns a tuple of (depth, name) — primitive obsession; should be a value
         object on the domain.
    
     In the good half, formation tops would be on the Field aggregate, loaded with
     EF, and exposed via a query. */

public static class FormationHelper
{
    public static (decimal Depth, string Name)[] GetTops(string fieldKey)
    {
        // This is keyed off a field name we ALSO store in the database.
        // The two will drift apart the first time someone renames a field.
        if (fieldKey == "FORTIES")
            return new[]
            {
                (1850m, "Top Forties Sand"),
                (2100m, "Base Forties Sand")
            };

        if (fieldKey == "BRENT")
            return new[]
            {
                (2600m, "Top Brent Group"),
                (2800m, "Top XYZ")
            };

        return Array.Empty<(decimal, string)>();
    }
}
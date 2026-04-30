namespace NorthLog.Wpf;

/* To the interviewer:
     AppContext is a static catch-all for "current state" — current operator name,
     last opened wellbore id, etc. I wanted to highlght how this can cause all kinds of problems:
    
       1. Race conditions when async commands read/write the same field
          while a long-running query is in flight.

       2. Logic spread across the codebase that secretly depends on a
          property being set somewhere far away.
    
     In the good half this kind of state lives on the ViewModel where it can be
     injected, scoped, and tested. */

public static class AppContext
{
    public static string CurrentOperator { get; set; } = "BP";
    public static Guid? LastOpenedWellboreId { get; set; }
    public static int CounterOfThingsThatGoneWrong;
}
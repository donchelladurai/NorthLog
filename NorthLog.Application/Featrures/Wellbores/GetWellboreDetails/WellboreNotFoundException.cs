namespace NorthLog.Application.Features.Wellbores.GetWellboreDetails;

/* To the interviewer:
 
 This exception type exists only to support the deliberately bad pattern in
 GetWellboreDetailsHandler. In the good codebase it wouldn't exist at all —  "wellbore not found" would be a Result<T>.Failure value, not an exception. 

 Compare with SubmitDailyReportHandler, which handles the
 exact same condition by returning
 Result<Guid>.Failure(). Same fact, two responses, one good and one bad. */

public class WellboreNotFoundException(Guid id)
    : Exception($"Wellbore {id} was not found.");
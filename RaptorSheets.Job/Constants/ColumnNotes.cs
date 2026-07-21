using System.Diagnostics.CodeAnalysis;

namespace RaptorSheets.Job.Constants;

[ExcludeFromCodeCoverage]
public static class ColumnNotes
{
    // Applications
    public const string ApplicationDate = "Date you submitted the application.";
    public const string ApplicationCompany = "Company name for the role.";
    public const string ApplicationJobTitle = "Job title exactly as posted.";
    public const string ApplicationPosting = "Link to the job posting.";
    public const string ApplicationSite = "Where you found or applied to the job.";
    public const string ApplicationInterviewCount = "Auto-calculated count of interviews linked to this application.";
    public const string ApplicationDecision = "Current status or final outcome of the application.";
    public const string ApplicationDecisionDate = "Date a final decision was received (if available).";
    public const string ApplicationDaysActive = "Auto-calculated days between apply date and decision (or today if pending).";
    public const string ApplicationNotes = "Free-form notes about this application.";
    public const string ApplicationPayLow = "Minimum salary/pay listed for the role.";
    public const string ApplicationPayHigh = "Maximum salary/pay listed for the role.";
    public const string ApplicationPayAvg = "Auto-calculated midpoint between Pay Low and Pay High.";
    public const string ApplicationLocation = "Role location (Remote, Hybrid, or city/state).";
    public const string ApplicationSchedule = "Work schedule type (Full-time, Contract, etc.).";
    public const string ApplicationDuplicate = "Auto-calculated duplicate count for the same Company + Job Title.";
    public const string ApplicationKey = "Auto-generated internal key used to link related records.";

    // Interviews
    public const string InterviewDate = "Interview date.";
    public const string InterviewStartTime = "Interview start time.";
    public const string InterviewEndTime = "Interview end time.";
    public const string InterviewDuration = "Total interview duration (for example 01:00).";
    public const string InterviewCompany = "Company for this interview.";
    public const string InterviewJobTitle = "Job title associated with the interview.";
    public const string InterviewType = "Interview type (Phone Screen, Technical, etc.).";
    public const string InterviewRound = "Auto-calculated interview round number for the application.";
    public const string InterviewRecruiterName = "Primary recruiter or coordinator name.";
    public const string InterviewRecruiterContact = "Recruiter contact info (email, phone, or profile link).";
    public const string InterviewAttendees = "Interviewers or attendees (comma-separated).";
    public const string InterviewOutcome = "Outcome of this interview.";
    public const string InterviewNotes = "Free-form notes from the interview.";
    public const string InterviewDuplicate = "Auto-calculated duplicate count for the same Company + Job Title.";
    public const string InterviewKey = "Auto-generated internal key used to link related records.";
}

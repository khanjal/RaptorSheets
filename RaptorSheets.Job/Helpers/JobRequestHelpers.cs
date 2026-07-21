using Google.Apis.Sheets.v4.Data;
using RaptorSheets.Core.Entities;
using RaptorSheets.Core.Helpers;
using RaptorSheets.Core.Mappers;
using RaptorSheets.Job.Entities;

namespace RaptorSheets.Job.Helpers;

/// <summary>
/// Job-specific wiring on top of Core's generic entity-change request builders. Only the
/// user-input sheets have change wrappers; reference sheets are formula-calculated and read-only.
/// </summary>
public static class JobRequestHelpers
{
    // APPLICATIONS
    public static List<Request> ChangeApplicationSheetData(List<ApplicationEntity> applications, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(applications, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<ApplicationEntity>.MapToRowData));

    // INTERVIEWS
    public static List<Request> ChangeInterviewSheetData(List<InterviewEntity> interviews, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(interviews, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<InterviewEntity>.MapToRowData));

    // COMPANY DETAILS
    public static List<Request> ChangeCompanyDetailSheetData(List<CompanyDetailEntity> details, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(details, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<CompanyDetailEntity>.MapToRowData));

    // POSITION DETAILS
    public static List<Request> ChangePositionDetailSheetData(List<PositionDetailEntity> details, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(details, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<PositionDetailEntity>.MapToRowData));

    // SETUP
    public static List<Request> ChangeSetupSheetData(List<SetupEntity> setup, PropertyEntity? sheetProperties)
        => GoogleRequestHelpers.ChangeSheetData(setup, sheetProperties, (entities, props) =>
            GoogleRequestHelpers.CreateUpdateCellRequests(entities, props, GenericSheetMapper<SetupEntity>.MapToRowData));
}

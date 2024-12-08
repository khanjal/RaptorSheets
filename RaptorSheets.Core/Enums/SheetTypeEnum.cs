namespace RaptorSheets.Core.Enums;

public enum SheetTypeEnum
{
    /// <summary>
    /// A sheet containing a grid. This is the default type.
    /// </summary>
    GRID,

    /// <summary>
    /// A sheet containing a single embedded object such as an EmbeddedChart.
    /// </summary>
    OBJECT,

    /// <summary>
    /// A sheet containing a DataSource.
    /// </summary>
    DATASOURCE
}

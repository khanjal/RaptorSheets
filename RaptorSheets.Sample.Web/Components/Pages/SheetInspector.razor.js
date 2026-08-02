// Co-located Blazor JS module (loaded via IJSRuntime.InvokeAsync<IJSObjectReference>("import", ...) -
// see SheetInspector.razor.cs) rather than a <script> tag in App.razor, so this never loads on pages
// that don't need it.
//
// Measures two things CSS can't compute on its own because they're content-dependent, not fixed:
// the column-letter header row's rendered height (for stacking the frozen header row directly below
// it) and the row-index column's rendered width (row numbers like "9" vs "200" render at different
// widths, and the frozen column A needs to sit exactly to the right of whatever that width actually
// is). Both are written onto the grid container as CSS custom properties that SheetInspector.razor.css
// consumes.
export function updateStickyOffsets(container) {
    if (!container) {
        return;
    }

    // getBoundingClientRect(), not offsetHeight/offsetWidth - offsetHeight is an integer that can be a
    // pixel off from what's actually painted once border-collapse:collapse merges the shared border
    // between thead's last row and tbody's first row, which showed up as a visible gap between the
    // column-letter header and the frozen row 1 stacked directly below it. getBoundingClientRect
    // reflects the exact rendered box (sub-pixel precision included), so the frozen row lands exactly
    // where the header's real edge is instead of a rounded approximation of it.
    const thead = container.querySelector('table thead');
    if (thead) {
        container.style.setProperty('--sheet-inspector-thead-height', thead.getBoundingClientRect().height + 'px');
    }

    const firstHeaderCell = container.querySelector('table thead th:first-child');
    if (firstHeaderCell) {
        container.style.setProperty('--sheet-inspector-first-col-width', firstHeaderCell.getBoundingClientRect().width + 'px');
    }
}
